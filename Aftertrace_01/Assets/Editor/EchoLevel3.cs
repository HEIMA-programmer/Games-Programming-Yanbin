using EchoShift;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace EchoShift.EditorTools
{
    /// <summary>
    /// Builds Level_03 "The Core": warm-toned climax combining echo + enemy + moving
    /// platforms. Four areas: gauntlet remix, mirror room (2 plates + latch door),
    /// decoy corridor, and the memory-core ending. Areas串联 via short descents.
    /// </summary>
    public static class EchoLevel3
    {
        static Transform levelT;
        static int groundLayer;
        static Material lit;
        static Material unlit;
        static readonly Color Warm = new Color(1f, 0.7f, 0.4f, 1f);
        static readonly Color Cyan = new Color(0f, 0.83f, 1f, 1f);

        public static void Build()
        {
            groundLayer = EchoBuildUtils.EnsurePhysicsLayer(EchoBuildUtils.GroundLayer);
            lit = EchoBuildUtils.LoadMaterial(EchoMaterials.LitName);
            unlit = EchoBuildUtils.LoadMaterial(EchoMaterials.UnlitName);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject cam = BuildCamera();
            EchoBuildUtils.AddGlobalLight(new GameObject("Global Light 2D"), new Color(1f, 0.95f, 0.85f, 1f), 0.16f);
            var vol = new GameObject("Bloom Volume").AddComponent<Volume>();
            vol.isGlobal = true;
            vol.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(EchoBuildUtils.BloomProfilePath);

            EchoUI.GameplayUIRefs ui = EchoUI.BuildGameplayCanvas();
            BuildGameManager(ui, "Sector 03 — The Core", "MainMenu",
                "I remember now.\nThis lab... I built it.\nThey left. But I remained.\nWaiting. Echoing.",
                "Thank you for playing. More memories await...", 3);

            var levelGO = new GameObject("Level");
            levelT = levelGO.transform;

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{EchoBuildUtils.PrefabDir}/Player.prefab");
            var player = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            player.transform.position = new Vector3(-2f, 0.6f, 0f);
            cam.GetComponent<CameraFollow>().target = player.transform;

            BuildBackground(cam.transform);
            BuildGeometry();

            EchoBuildUtils.EnsureFolder(EchoBuildUtils.SceneDir);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, EchoBuildUtils.Level3ScenePath);
        }

        static void BuildGeometry()
        {
            EchoTilemap.BeginTerrain(levelT, lit);

            SolidBlock(-4.6f, 3f, 1f, 14f, "LeftWall");

            // ---- Area 1: Gauntlet Remix (plate -> rising platform -> upper enemy) ----
            SolidBlock(6f, -1f, 20f, 2f, "GroundA");          // x -4..16
            SolidBlock(16.5f, 1f, 1f, 4f, "BlockWallA");      // y0..3 wall (forces using the platform)
            SolidBlock(15.75f, 4f, 11.5f, 1f, "UpperA");      // upper walkway top y4.5 (x10..21.5), clear above P1
            Checkpoint(-2f);
            var plate1 = Inst("PressurePlate", new Vector3(3f, 0.13f, 0f));
            ConfigPlate(Inst("MovingPlatform", new Vector3(9f, 0.4f, 0f)), plate1, 3.8f, 3.2f); // top ~4.5 = UpperA
            Drone(14f, 5.6f, 4f, 2.6f);
            SolidBlock(22f, 2.5f, 2f, 1f, "StepA1");          // top y3
            SolidBlock(24f, 1f, 2f, 1f, "StepA2");            // top y1.5 -> ground B
            SolidBlock(9f, 6.6f, 1.6f, 0.4f, "FragLedgeA");
            InstFrag(9f, 7.3f);

            // ---- Area 2: Echo Lift (optional fragment via a clone-held plate) ----
            // Simplified from the old "mirror room": the door here gated nothing (the ground is
            // open) and its one-way gate locked permanently, so both are gone. What remains is a
            // clean optional echo puzzle — hold plateA with a recorded clone, ride the raised
            // platform up to the ledge, and jump for the fragment. Skippable (off the main path).
            SolidBlock(34f, -1f, 24f, 2f, "GroundB");         // x 22..46, top y0 (open walk-through)
            Checkpoint(24f);
            var plateA = Inst("PressurePlate", new Vector3(27f, 0.13f, 0f));
            ConfigPlate(Inst("MovingPlatform", new Vector3(30f, 0.4f, 0f)), plateA, 3.3f, 3f); // top ~y4 = MidLedgeB
            SolidBlock(36f, 3.5f, 10f, 1f, "MidLedgeB");      // ledge top y4 (x31..41)
            InstFrag(34f, 6.3f);       // above the ledge: a jump from MidLedgeB (apex ~2.87) grabs it
            Arrow(27f, 2f, -45f);      // points from plateA toward the rising platform
            Arrow(34f, 5.2f, 0f);      // points up at the fragment

            // ---- Area 3: Stealth Corridor (detection + cover + decoy) ----
            // These drones CATCH you if their cone holds you in sight too long (a detection meter
            // in GameManager), not only on contact — so standing on a ledge no longer trivialises
            // it. Cover pillars block line of sight: wait in a pillar's shadow, time the cone
            // sweeps, or record a clone — the drones lock their cones onto the decoy (clone-
            // priority) and leave your lane unwatched. Open above, so you can hop a pillar, but
            // you're briefly exposed at the apex (a short look is fine; lingering gets you caught).
            SolidBlock(54f, -1f, 24f, 2f, "GroundC");           // lower lane x42..66, top y0
            SolidBlock(48f, 1.2f, 0.7f, 2.4f, "CoverC1");       // LOS blocker, top y2.4 — shadows the entrance from drone 1
            SolidBlock(56f, 1.2f, 0.7f, 2.4f, "CoverC2");       // LOS blocker, top y2.4 — mid shadow pocket before drone 2
            Checkpoint(44f);
            // drones patrol the gaps between cover; detects:true makes their cones lethal-on-sight
            Drone(52f, 0.9f, 3f, 3.0f, false, 4f, true);        // patrol x49..55
            Drone(59f, 0.9f, 2.5f, 3.0f, false, 4f, true);      // patrol x56.5..61.5
            // signpost the stealth play
            EchoBuildUtils.CreateWallNarrative(levelT, new Vector3(43f, 3.0f, 0f), "Don't be seen — hide, time it, or send a decoy");
            Arrow(45.5f, 1.0f, -90f);   // advance right, cover to cover

            // ---- Area 4: Memory Core ----
            SolidBlock(72f, -1f, 20f, 2f, "GroundD");         // x 62..82
            Checkpoint(64f);
            var plate4 = Inst("PressurePlate", new Vector3(66f, 0.13f, 0f));
            var d4 = Inst("Door", new Vector3(70f, 1f, 0f));
            DoorWall(70f);
            var door4 = d4.GetComponent<Door>();
            door4.requiredPlates = new[] { plate4.GetComponent<PressurePlate>() };
            door4.latch = true;
            Gate(72f, door4);
            Inst("EndArch", new Vector3(78f, 1f, 0f));
            Inst("Collectible", new Vector3(76f, 1f, 0f));    // ending fragment (endsLevel=true)
            SolidBlock(82.5f, 4f, 1f, 16f, "RightWall");

            // warm, bright memory-core lighting
            AmbientLight(75f, 2.5f, Warm, 0.95f, 7f);
            AmbientLight(78f, 3f, new Color(1f, 0.85f, 0.6f, 1f), 0.75f, 6f);

            // ambient mood + corridor narrative
            AmbientLight(4f, 4f, Cyan, 0.3f, 6f);
            AmbientLight(20f, 5f, Warm, 0.3f, 6f);
            AmbientLight(34f, 6f, Cyan, 0.3f, 7f);
            AmbientLight(54f, 0.8f, Warm, 0.3f, 6f);   // inside the tunnel now that the ceiling is solid up to y4.5
            EchoBuildUtils.CreateWallNarrative(levelT, new Vector3(28f, 6.5f, 0f), "Memory restoration chamber — 200m ahead");
            EchoBuildUtils.CreateWallNarrative(levelT, new Vector3(64f, 3.2f, 0f), "Welcome home, Echo.", new Color(1f, 0.8f, 0.5f, 1f));
        }

        // ---- platform config ----
        static void ConfigPlate(GameObject platform, GameObject plate, float rise, float speed)
        {
            var mp = platform.GetComponent<MovingPlatform>();
            mp.mode = MovingPlatform.PlatformMode.PlateActivated;
            mp.plate = plate.GetComponent<PressurePlate>();
            mp.riseHeight = rise;
            mp.moveSpeed = speed;
        }

        static void ConfigPingPong(GameObject platform, Vector2 offset, float speed)
        {
            var mp = platform.GetComponent<MovingPlatform>();
            mp.mode = MovingPlatform.PlatformMode.PingPong;
            mp.moveOffset = offset;
            mp.moveSpeed = speed;
        }

        static void InstFrag(float x, float y)
        {
            Inst("Collectible", new Vector3(x, y, 0f)).GetComponent<Collectible>().endsLevel = false;
        }

        // ---- shared helpers (mirror EchoLevel2) ----
        static GameObject BuildCamera()
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            go.transform.position = new Vector3(0f, 1f, -10f);
            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6.5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = EchoBuildUtils.ColBackground; // pure black for 1-Bit contrast
            go.AddComponent<AudioListener>();
            go.AddComponent<UniversalAdditionalCameraData>().renderPostProcessing = true;
            go.AddComponent<CameraShake>();
            go.AddComponent<CameraFollow>();
            return go;
        }

        static void BuildGameManager(EchoUI.GameplayUIRefs ui, string levelName, string nextScene,
            string narrative, string finalMessage, int totalFragments)
        {
            var gm = new GameObject("GameManager").AddComponent<GameManager>();
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

        static void BuildBackground(Transform cameraTransform)
        {
            Random.InitState(31337);
            var bg = new GameObject("Background");
            EchoBuildUtils.BuildAtmosphere(bg.transform, cameraTransform, 72f, unlit);
        }

        static GameObject SolidBlock(float cx, float cy, float w, float h, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(levelT, false);
            go.transform.localPosition = new Vector3(cx, cy, 0f);
            go.layer = groundLayer;
            // Visual: paint the kit's real rock tiles onto the shared terrain Tilemap
            // (collision unchanged — the BoxCollider below still drives physics).
            EchoTilemap.PaintSolid(cx, cy, w, h);
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(w, h);
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

        static void Checkpoint(float x)
        {
            var cp = Inst("Checkpoint", new Vector3(x, 1.2f, 0f));
            cp.GetComponent<Checkpoint>().respawnPoint = new Vector3(x, 0.6f, 0f);
        }

        static void Drone(float x, float y, float dist, float speed, bool destroysClone = true, float leash = 5f, bool detects = false)
        {
            var e = Inst("PatrolDrone", new Vector3(x, y, 0f));
            var d = e.GetComponent<PatrolDrone>();
            d.patrolDistance = dist;
            d.speed = speed;
            d.destroysClone = destroysClone;
            d.leash = leash;
            d.detects = detects;
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

        static void Arrow(float x, float y, float zRot)
        {
            var a = EchoBuildUtils.SpriteGO("Guide", EchoBuildUtils.LoadSprite("arrow"), "Foreground", 4, levelT, unlit,
                new Color(0.6f, 0.9f, 1f, 0.9f));
            a.transform.localPosition = new Vector3(x, y, 0f);
            a.transform.localRotation = Quaternion.Euler(0f, 0f, zRot);
            a.AddComponent<FloatingBob>();
            var pg = a.AddComponent<PulseGlow>();
            pg.target = a.GetComponent<SpriteRenderer>();
            pg.pulseScale = false;
            pg.minAlpha = 0.5f;
            pg.maxAlpha = 1f;
        }
    }
}
