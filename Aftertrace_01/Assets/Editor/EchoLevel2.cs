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
    /// Builds Level_02 "Deep Labs": four areas introducing the patrol drone enemy, with
    /// checkpoints, a clone-decoy puzzle, a timed run, and a two-enemy gauntlet with a
    /// plate-controlled bridge. Darker/cooler than Level 1.
    /// </summary>
    public static class EchoLevel2
    {
        static Transform levelT;
        static int groundLayer;
        static Material lit;
        static Material unlit;
        static readonly Color Cold = new Color(0.45f, 0.65f, 1f, 1f);
        static readonly Color Amber = new Color(1f, 0.78f, 0.45f, 1f);

        public static void Build()
        {
            groundLayer = EchoBuildUtils.EnsurePhysicsLayer(EchoBuildUtils.GroundLayer);
            lit = EchoBuildUtils.LoadMaterial(EchoMaterials.LitName);
            unlit = EchoBuildUtils.LoadMaterial(EchoMaterials.UnlitName);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject cam = BuildCamera();
            EchoBuildUtils.AddGlobalLight(new GameObject("Global Light 2D"), Color.white, 0.08f);
            var vol = new GameObject("Bloom Volume").AddComponent<Volume>();
            vol.isGlobal = true;
            vol.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(EchoBuildUtils.BloomProfilePath);

            EchoUI.GameplayUIRefs ui = EchoUI.BuildGameplayCanvas();
            BuildGameManager(ui, "Sector 02 — Deep Labs", "Level_03",
                "I remember... I wasn't always alone.", "", 3);

            var levelGO = new GameObject("Level");
            levelT = levelGO.transform;

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{EchoBuildUtils.PrefabDir}/Player.prefab");
            var player = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            player.transform.position = new Vector3(-2f, 0.6f, 0f);
            cam.GetComponent<CameraFollow>().target = player.transform;

            BuildGeometry();

            EchoBuildUtils.EnsureFolder(EchoBuildUtils.SceneDir);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, EchoBuildUtils.Level2ScenePath);
        }

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

        static void BuildGeometry()
        {
            // --- floors / walls ---
            SolidBlock(-4.6f, 3f, 1f, 14f, "LeftWall");
            SolidBlock(39f, -1f, 86f, 2f, "GroundMain");        // x -4..82 (areas 1-3)
            SolidBlock(91f, -1f, 18f, 2f, "Ground4Lower");      // x 82..100
            SolidBlock(111.5f, -1f, 9f, 2f, "Ground4Right");    // x 107..116 (gap 100..107 = bridge)
            SolidBlock(84f, 0.5f, 2f, 3f, "Step1");             // top y 2
            SolidBlock(86.5f, 1.75f, 2f, 4.5f, "Step2");        // helps reach the upper ledge
            SolidBlock(92.5f, 3.75f, 14f, 1.5f, "UpperLedge");  // top y 4.5 (x 85.5..99.5)
            SolidBlock(116.5f, 4f, 1f, 16f, "RightWall");

            // --- Area 1: enemy introduction ---
            Checkpoint(-2f);
            Drone(12f, 0.9f, 6f, 2.6f);
            SolidBlock(21f, 0.7f, 1f, 1.4f, "CoverPillarA");   // low cover just past the patrol: duck behind it to break the drone's sight

            // --- Area 2: echo decoy ---
            Checkpoint(29f);
            Drone(37f, 0.9f, 4f, 2.8f);   // tighter: harder to slip past without a decoy clone
            var plate2 = Inst("PressurePlate", new Vector3(44f, 0.13f, 0f));
            var door2 = Inst("Door", new Vector3(50f, 1f, 0f));
            DoorWall(50f);
            door2.GetComponent<Door>().requiredPlates = new[] { plate2.GetComponent<PressurePlate>() };
            Gate(51.5f, door2.GetComponent<Door>());

            // --- Area 3: timed plate + enemy ---
            Checkpoint(53f);
            var plate3 = Inst("PressurePlate", new Vector3(56f, 0.13f, 0f));
            Drone(69f, 0.9f, 6f, 3.0f);
            var door3 = Inst("Door", new Vector3(80f, 1f, 0f));
            DoorWall(80f);
            door3.GetComponent<Door>().requiredPlates = new[] { plate3.GetComponent<PressurePlate>() };
            Gate(81.5f, door3.GetComponent<Door>());

            // --- Area 4: the gauntlet (upper plate -> lower bridge, two enemies) ---
            Checkpoint(82f);
            var plateA = Inst("PressurePlate", new Vector3(88f, 4.63f, 0f));   // on the upper ledge
            Drone(95f, 5.4f, 3f, 2.4f, 4f);                                    // upper enemy (short leash so it can't chase off the ledge edge at x99.5)
            Drone(94f, 0.9f, 4f, 2.8f);                                        // lower enemy

            var bridge = Inst("MovingPlatform", new Vector3(103.5f, -2.5f, 0f));
            var mp = bridge.GetComponent<MovingPlatform>();
            mp.plate = plateA.GetComponent<PressurePlate>();
            mp.riseHeight = 3f;
            mp.moveSpeed = 3f;
            var vis = bridge.transform.Find("Visual");
            if (vis != null) vis.localScale = new Vector3(6f, 0.6f, 1f);
            var bcol = bridge.GetComponent<BoxCollider2D>();
            if (bcol != null) bcol.size = new Vector2(6f, 0.6f);

            // --- ending ---
            Inst("EndArch", new Vector3(114f, 1f, 0f));
            Inst("Collectible", new Vector3(112f, 1f, 0f)); // level-ending fragment

            // --- optional fragments (off the critical path) ---
            SolidBlock(12f, 2f, 2f, 0.4f, "FragLedge1");     // above enemy e1's patrol
            Inst("Collectible", new Vector3(12f, 2.8f, 0f)).GetComponent<Collectible>().endsLevel = false;
            SolidBlock(66f, 2.1f, 2f, 0.4f, "FragLedge3");   // alcove on the timed sprint
            Inst("Collectible", new Vector3(66f, 2.8f, 0f)).GetComponent<Collectible>().endsLevel = false;

            // --- ambient mood lights (cool, sparse) ---
            AmbientLight(8f, 4f, Cold, 0.3f, 6f);
            AmbientLight(40f, 4.5f, Cold, 0.25f, 6f);
            AmbientLight(64f, 4.5f, Amber, 0.25f, 5f);
            AmbientLight(90f, 6f, Cold, 0.3f, 7f);
            AmbientLight(112f, 4f, new Color(0.27f, 1f, 0.67f, 1f), 0.35f, 6f);
        }

        // ---- helpers ----
        // Blockout: collider + a flat grey GREYBOX fill so the level STRUCTURE is visible while
        // you hand-author terrain art on tilemap layers over it. The "BlockoutFill" children are
        // a temporary placeholder (sorting order -5, so painted art on top hides them; or delete
        // them per region). Gameplay/physics unchanged.
        static GameObject SolidBlock(float cx, float cy, float w, float h, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(levelT, false);
            go.transform.localPosition = new Vector3(cx, cy, 0f);
            go.layer = groundLayer;
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(w, h);

            var fill = EchoBuildUtils.SpriteGO("BlockoutFill", EchoBuildUtils.LoadSprite("white"),
                "Environment", -5, go.transform, unlit, new Color(0.36f, 0.39f, 0.45f, 1f));
            fill.transform.localScale = new Vector3(w, h, 1f);
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

        static void Drone(float x, float y, float dist, float speed, float leash = 5f)
        {
            var e = Inst("PatrolDrone", new Vector3(x, y, 0f));
            var d = e.GetComponent<PatrolDrone>();
            d.patrolDistance = dist;
            d.speed = speed;
            d.leash = leash;
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
    }
}
