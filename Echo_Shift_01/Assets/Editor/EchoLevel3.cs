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

            // ---- Area 2: Mirror Room (2 plates + latch, vertical) ----
            SolidBlock(34f, -1f, 24f, 2f, "GroundB");         // x 22..46
            Checkpoint(24f);
            var plateA = Inst("PressurePlate", new Vector3(27f, 0.13f, 0f));
            ConfigPlate(Inst("MovingPlatform", new Vector3(30f, 0.4f, 0f)), plateA, 3.3f, 3f); // top ~4 = MidLedgeB
            SolidBlock(36f, 3.5f, 10f, 1f, "MidLedgeB");      // mid ledge top y4 (x31..41), clear above P2
            var plateB = Inst("PressurePlate", new Vector3(35f, 4.13f, 0f));
            var d2 = Inst("Door", new Vector3(40f, 5f, 0f));  // door at mid level
            SolidBlock(40f, 7.5f, 0.7f, 4f, "DoorWallB");
            var door2 = d2.GetComponent<Door>();
            door2.requiredPlates = new[] { plateA.GetComponent<PressurePlate>(), plateB.GetComponent<PressurePlate>() };
            door2.requireAll = true;
            door2.latch = true;
            Gate(43f, door2);
            ConfigPingPong(Inst("MovingPlatform", new Vector3(37.5f, 4f, 0f)), new Vector2(0f, 4f), 2.4f);
            InstFrag(37.5f, 8.5f);     // fragment 2 sits at P3's top point (no ledge above the platform)
            Arrow(27f, 2f, -45f);      // points from plateA toward the rising platform P2
            Arrow(37.5f, 6.4f, 0f);    // points up: ride the ping-pong platform to fragment 2

            // ---- Area 3: Decoy Corridor (two enemies) ----
            SolidBlock(54f, -1f, 24f, 2f, "GroundC");         // x 42..66
            SolidBlock(54f, 2f, 18f, 1f, "CeilingC");         // low ceiling (y1.5..2.5) forces a ground run past enemies
            Checkpoint(44f);
            // decoy enemies (clone survives, draws both); x44..48 left as a safe record buffer
            Drone(52f, 0.9f, 4f, 3.0f, false);
            Drone(57f, 0.9f, 3f, 3.4f, false);

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
            AmbientLight(54f, 4f, Warm, 0.3f, 6f);
            EchoBuildUtils.CreateWallNarrative(levelT, new Vector3(28f, 6.5f, 0f), "Memory restoration chamber — 200m ahead");
            EchoBuildUtils.CreateWallNarrative(levelT, new Vector3(63f, 3.2f, 0f), "Welcome home, Echo.", new Color(1f, 0.8f, 0.5f, 1f));
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
            cam.backgroundColor = new Color(0.07f, 0.06f, 0.08f, 1f);
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

            var far = new GameObject("BG_Far");
            far.transform.SetParent(bg.transform, false);
            var fp = far.AddComponent<Parallax>(); fp.factor = 0.2f; fp.cameraTransform = cameraTransform;
            for (int i = 0; i < 44; i++)
            {
                bool warm = Random.value < 0.5f;
                Color c = warm ? new Color(1f, 0.7f, 0.4f, Random.Range(0.15f, 0.45f))
                               : new Color(0.4f, 0.6f, 1f, Random.Range(0.15f, 0.4f));
                var d = EchoBuildUtils.SpriteGO("Dot", EchoBuildUtils.LoadSprite("bgdot"), "Background", 0, far.transform, unlit, c);
                d.transform.localPosition = new Vector3(Random.Range(-6f, 86f), Random.Range(2f, 12f), 5f);
                d.transform.localScale = Vector3.one * Random.Range(0.3f, 1f);
            }

            var mid = new GameObject("BG_Mid");
            mid.transform.SetParent(bg.transform, false);
            var mp = mid.AddComponent<Parallax>(); mp.factor = 0.5f; mp.cameraTransform = cameraTransform;
            for (int i = 0; i < 14; i++)
            {
                var e = EchoBuildUtils.SpriteGO("Equip", EchoBuildUtils.LoadSprite("bgequip"), "Midground", 0, mid.transform, lit,
                    new Color(0.5f, 0.45f, 0.5f, 1f));
                e.transform.localPosition = new Vector3(Random.Range(0f, 84f), Random.Range(-0.5f, 3f), 3f);
                e.transform.localScale = Vector3.one * Random.Range(0.9f, 1.9f);
            }
        }

        static GameObject SolidBlock(float cx, float cy, float w, float h, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(levelT, false);
            go.transform.localPosition = new Vector3(cx, cy, 0f);
            go.layer = groundLayer;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = EchoBuildUtils.LoadSprite("platform");
            sr.sharedMaterial = lit;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(w, h);
            sr.sortingLayerName = "Environment";
            sr.sortingOrder = 0;
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(w, h);
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

        static void Drone(float x, float y, float dist, float speed, bool destroysClone = true)
        {
            var e = Inst("PatrolDrone", new Vector3(x, y, 0f));
            var d = e.GetComponent<PatrolDrone>();
            d.patrolDistance = dist;
            d.speed = speed;
            d.destroysClone = destroysClone;
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
