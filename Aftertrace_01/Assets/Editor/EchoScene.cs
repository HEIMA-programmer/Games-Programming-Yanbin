using EchoShift;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EchoShift.EditorTools
{
    /// <summary>
    /// Builds the playable Level_01 scene: URP camera, global light, bloom volume,
    /// UI canvas, parallax background, and the five areas laid out left to right with
    /// all cross-references wired. Saves to Assets/_Scenes/Level_01.unity.
    /// </summary>
    public static class EchoScene
    {
        static Transform levelT;
        static int groundLayer;
        static Material lit;
        static Material unlit;
        static Material particle;

        public static void Build()
        {
            groundLayer = EchoBuildUtils.EnsurePhysicsLayer(EchoBuildUtils.GroundLayer);
            lit = EchoBuildUtils.LoadMaterial(EchoMaterials.LitName);
            unlit = EchoBuildUtils.LoadMaterial(EchoMaterials.UnlitName);
            particle = EchoBuildUtils.LoadMaterial(EchoMaterials.ParticleName);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject cam = BuildCamera();
            BuildGlobalLight();
            BuildBloomVolume();
            EchoUI.GameplayUIRefs ui = EchoUI.BuildGameplayCanvas();
            BuildGameManager(ui, "Sector 01 — Awakening", "Level_02",
                "I remember... this was my home.", "", 3);

            var levelGO = new GameObject("Level");
            levelT = levelGO.transform;

            GameObject player = InstantiatePlayer();
            cam.GetComponent<CameraFollow>().target = player.transform;

            BuildBackground(cam.transform);
            BuildGeometry();

            EchoBuildUtils.EnsureFolder(EchoBuildUtils.SceneDir);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, EchoBuildUtils.ScenePath);
        }

        // ----------------------------------------------------------- core rig
        static GameObject BuildCamera()
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            go.transform.position = new Vector3(0f, 1f, -10f);
            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.4f; // zoomed-in for denser composition (was 6.5)
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = EchoBuildUtils.ColBackground;
            go.AddComponent<AudioListener>();

            var data = go.AddComponent<UniversalAdditionalCameraData>();
            data.renderPostProcessing = true;

            go.AddComponent<CameraShake>();
            go.AddComponent<CameraFollow>();
            return go;
        }

        static void BuildGlobalLight()
        {
            var go = new GameObject("Global Light 2D");
            EchoBuildUtils.AddGlobalLight(go, Color.white, 0.12f);
        }

        static void BuildBloomVolume()
        {
            var go = new GameObject("Bloom Volume");
            var vol = go.AddComponent<Volume>();
            vol.isGlobal = true;
            vol.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(EchoBuildUtils.BloomProfilePath);
        }

        static GameObject InstantiatePlayer()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{EchoBuildUtils.PrefabDir}/Player.prefab");
            var player = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            player.transform.position = new Vector3(0f, 0.6f, 0f);
            return player;
        }

        // In-game UI (HUD / pause / victory) is built by EchoUI.BuildGameplayCanvas().
        static void BuildGameManager(EchoUI.GameplayUIRefs ui, string levelName, string nextScene,
            string narrative, string finalMessage, int totalFragments)
        {
            var go = new GameObject("GameManager");
            var gm = go.AddComponent<GameManager>();
            gm.levelName = levelName;
            gm.nextSceneName = nextScene;
            gm.narrativeLine = narrative;
            gm.finalMessage = finalMessage;
            gm.totalFragments = totalFragments;
            gm.hud = ui.hud;
            gm.pauseMenu = ui.pause;
            gm.victoryScreen = ui.victory;
            gm.vignette = ui.vignette;
            gm.flashImage = ui.flash;
            gm.hitFlashImage = ui.hitFlash;
        }

        // ----------------------------------------------------------- background
        static void BuildBackground(Transform cameraTransform)
        {
            Random.InitState(20240517);
            var bg = new GameObject("Background");
            EchoBuildUtils.BuildAtmosphere(bg.transform, cameraTransform, 136f, unlit);

            // Note: foreground richness comes from terrain + platforms + gameplay objects,
            // not a cluttered parallax backdrop (the reference keeps backgrounds mostly black).
        }

        // ----------------------------------------------------------- level geometry
        static void BuildGeometry()
        {
            // Shared terrain Tilemap painted from the kit's real rock tiles (VISUAL only;
            // collision stays on each block's BoxCollider). Must precede the SolidBlocks.
            EchoTilemap.BeginTerrain(levelT, lit);

            // --- floors & walls (top surface y = 0 unless noted) ---
            SolidBlock(-4.6f, 3f, 1f, 14f, "LeftWall");
            SolidBlock(2f, -1f, 12f, 2f, "GroundA");           // x -4..8
            SolidBlock(9.5f, -2.0f, 3.4f, 1.2f, "PitFloor");   // recoverable pit under the gap
            SolidBlock(55.75f, -1f, 91.5f, 2f, "GroundB");     // x 10..101.5 (continues under the lift shaft)
            SolidBlock(102f, 2.5f, 1.0f, 5f, "Cliff");         // blocks the ground route in Area 5
            SolidBlock(118.5f, 4f, 33f, 2f, "Ledge");          // high ending ledge, top y 5 (x 102..135)
            SolidBlock(135.5f, 6f, 1f, 16f, "RightWall");

            // --- Area 1: guide particles toward the right ---
            CreateGuideParticles(new Vector3(3f, 0.25f, 0f));

            // --- Area 2: one plate opens one door ---
            var plate2 = Inst("PressurePlate", new Vector3(20f, 0.13f, 0f));
            var door2 = Inst("Door", new Vector3(25f, 1f, 0f));
            DoorWall(25f);
            door2.GetComponent<Door>().requiredPlates = new[] { plate2.GetComponent<PressurePlate>() };
            Gate(27f, door2.GetComponent<Door>());
            RPrompt(new Vector3(20f, 2.3f, 0f));

            // --- Area 3: timed run, plate far from door ---
            var plate3 = Inst("PressurePlate", new Vector3(36f, 0.13f, 0f));
            var door3 = Inst("Door", new Vector3(60f, 1f, 0f));
            DoorWall(60f);
            door3.GetComponent<Door>().requiredPlates = new[] { plate3.GetComponent<PressurePlate>() };
            Gate(62f, door3.GetComponent<Door>());

            // --- Area 4: two plates, latching door ---
            var plateA = Inst("PressurePlate", new Vector3(66f, 0.13f, 0f));
            var plateB = Inst("PressurePlate", new Vector3(74f, 0.13f, 0f));
            var door4 = Inst("Door", new Vector3(82f, 1f, 0f));
            DoorWall(82f);
            var d4 = door4.GetComponent<Door>();
            d4.requiredPlates = new[] { plateA.GetComponent<PressurePlate>(), plateB.GetComponent<PressurePlate>() };
            d4.requireAll = true;
            d4.latch = true;
            Gate(84f, d4);

            // --- Area 5: vertical puzzle → rise to the ending ledge ---
            var plate5 = Inst("PressurePlate", new Vector3(95f, 0.13f, 0f));
            var platform = Inst("MovingPlatform", new Vector3(98f, 0.4f, 0f));
            var mp5 = platform.GetComponent<MovingPlatform>();
            mp5.plate = plate5.GetComponent<PressurePlate>();
            mp5.moveSpeed = 2.0f;   // slow enough to board before it climbs away
            mp5.riseHeight = 4.6f;

            Inst("EndArch", new Vector3(126f, 6f, 0f));
            Inst("Collectible", new Vector3(120f, 6.3f, 0f)); // level-ending fragment (endsLevel=true by default)

            // --- optional fragments (off the critical path) ---
            SolidBlock(16f, 2f, 2f, 0.4f, "FragLedge2");
            Inst("Collectible", new Vector3(16f, 2.9f, 0f)).GetComponent<Collectible>().endsLevel = false;
            SolidBlock(70f, 1.8f, 2f, 0.4f, "FragLedge4");
            Inst("Collectible", new Vector3(70f, 2.6f, 0f)).GetComponent<Collectible>().endsLevel = false;

            // --- corridor environmental narrative ---
            EchoBuildUtils.CreateWallNarrative(levelT, new Vector3(30f, 3.2f, 0f), "Echo Protocol: Active");
            EchoBuildUtils.CreateWallNarrative(levelT, new Vector3(88f, 3.4f, 0f), "Core Access — Authorized Personnel Only");

            // --- ambient mood lights ---
            AmbientLight(6f, 4f, EchoBuildUtils.ColPlayer, 0.35f, 6f);
            AmbientLight(30f, 4.5f, new Color(1f, 0.8f, 0.5f, 1f), 0.3f, 6f);
            AmbientLight(48f, 5f, EchoBuildUtils.ColPlayer, 0.3f, 7f);
            AmbientLight(72f, 4.5f, new Color(1f, 0.8f, 0.5f, 1f), 0.3f, 6f);
            AmbientLight(90f, 4f, EchoBuildUtils.ColPlayer, 0.3f, 6f);
            AmbientLight(118f, 8f, EchoBuildUtils.ColEnd, 0.4f, 8f);
        }

        // ----------------------------------------------------------- helpers
        static GameObject SolidBlock(float cx, float cy, float w, float h, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(levelT, false);
            go.transform.localPosition = new Vector3(cx, cy, 0f);
            go.layer = groundLayer;

            // Visual: paint the kit's real rock tiles (top crest / dithered body / base) onto
            // the shared terrain Tilemap — replaces the old single tiled sprite + white edge.
            EchoTilemap.PaintSolid(cx, cy, w, h);

            // Collision unchanged: per-block BoxCollider (gameplay identical to before).
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(w, h);

            // Sparse kit crates resting on wide ground (the reference's "stuff on the floor").
            EchoBuildUtils.PlaceGroundProps(go, w, h, unlit);
            return go;
        }

        static void DoorWall(float x) => SolidBlock(x, 4.5f, 0.7f, 5f, "DoorWall");

        static void Gate(float x, Door door)
        {
            var go = new GameObject("Gate");
            go.transform.SetParent(levelT, false);
            go.transform.localPosition = new Vector3(x, 1.2f, 0f);
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1.2f, 3.5f);
            go.AddComponent<ProgressionTrigger>().doorToClose = door;
        }

        static GameObject Inst(string prefabName, Vector3 pos)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{EchoBuildUtils.PrefabDir}/{prefabName}.prefab");
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.transform.SetParent(levelT, false);
            go.transform.localPosition = pos;
            return go;
        }

        static void AmbientLight(float x, float y, Color c, float intensity, float radius)
        {
            var go = new GameObject("AmbientLight");
            go.transform.SetParent(levelT, false);
            go.transform.localPosition = new Vector3(x, y, 0f);
            EchoBuildUtils.AddPointLight(go, c, intensity, radius);
        }

        static void RPrompt(Vector3 pos)
        {
            var root = new GameObject("RPrompt");
            root.transform.SetParent(levelT, false);
            root.transform.localPosition = pos;

            var keycap = EchoBuildUtils.SpriteGO("Keycap", EchoBuildUtils.LoadSprite("keycap"), "Foreground", 5, root.transform, unlit);
            keycap.transform.localScale = Vector3.one * 1.3f;
            var pg = keycap.AddComponent<PulseGlow>();
            pg.target = keycap.GetComponent<SpriteRenderer>();
            pg.pulseAlpha = false;
            pg.minScale = 0.92f;
            pg.maxScale = 1.08f;

            var rgo = new GameObject("R");
            rgo.transform.SetParent(root.transform, false);
            rgo.transform.localPosition = new Vector3(0f, 0f, -0.02f);
            var tmp = rgo.AddComponent<TextMeshPro>();
            tmp.text = "R";
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 6f;
            tmp.color = Color.white;
            tmp.rectTransform.sizeDelta = new Vector2(3f, 3f);
            var mr = rgo.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.sortingLayerID = SortingLayer.NameToID("Foreground");
                mr.sortingOrder = 6;
            }

            root.AddComponent<FloatingBob>();
        }

        static void CreateGuideParticles(Vector3 pos)
        {
            var go = new GameObject("GuideParticles");
            go.transform.SetParent(levelT, false);
            go.transform.localPosition = pos;

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true;
            main.duration = 4f;
            main.startLifetime = 4f;
            main.startSpeed = 0f;
            main.startSize = 0.13f;
            main.startColor = new Color(0f, 0.83f, 1f, 0.5f);
            main.maxParticles = 120;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = true;

            var em = ps.emission;
            em.rateOverTime = 6f;

            var sh = ps.shape;
            sh.enabled = true;
            sh.shapeType = ParticleSystemShapeType.Box;
            sh.scale = new Vector3(0.6f, 0.25f, 0.1f);

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.World;
            vel.x = new ParticleSystem.MinMaxCurve(0.8f);

            var colOL = ps.colorOverLifetime;
            colOL.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.6f, 0.3f), new GradientAlphaKey(0f, 1f) });
            colOL.color = grad;

            var rend = go.GetComponent<ParticleSystemRenderer>();
            rend.sharedMaterial = particle;
            rend.sortingLayerName = "Foreground";
            rend.sortingOrder = 3;
        }
    }
}
