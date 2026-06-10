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
    /// Builds Level_00 "Awakening" — the ECHO TUTORIAL. The machine boots (terminal log),
    /// learns to move and jump (fall = respawn at a checkpoint), then DISCOVERS the echo at a
    /// one-plate gate: record yourself onto the plate so your echo holds the door open while you
    /// slip through. It recovers its first memory and steps into Level_01.
    ///
    /// In-level narrative + hints run through the shared NarrativeTerminal + StoryTrigger zones.
    /// A bare GameManager (no HUD / victory) is present only to own the checkpoint + fall-respawn.
    /// </summary>
    public static class EchoLevel0
    {
        const string DoorPath = "Assets/Art/Imported/CraftPix1Bit/Objects/Door.png";

        static Transform levelT;
        static int groundLayer;
        static Material lit;
        static Material unlit;

        public static void Build()
        {
            groundLayer = EchoBuildUtils.EnsurePhysicsLayer(EchoBuildUtils.GroundLayer);
            lit = EchoBuildUtils.LoadMaterial(EchoMaterials.LitName);
            unlit = EchoBuildUtils.LoadMaterial(EchoMaterials.UnlitName);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject cam = BuildCamera();
            EchoBuildUtils.AddGlobalLight(new GameObject("Global Light 2D"), Color.white, 0.1f);
            var vol = new GameObject("Bloom Volume").AddComponent<Volume>();
            vol.isGlobal = true;
            vol.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(EchoBuildUtils.BloomProfilePath);

            // Bare GameManager (no UI) — owns the checkpoint + fall-respawn only.
            BuildGameManager();

            // Shared narrative terminal — drives the in-level hints + memory beats.
            EchoNarrative.BuildTerminal();

            var levelGO = new GameObject("Level");
            levelT = levelGO.transform;

            // --- geometry (top surface y = 0); one jump gap at x[6..8] (fall = respawn) ---
            SolidBlock(1f, -1f, 10f, 2f, "GroundA");         // x -4..6  (wake room + corridor)
            SolidBlock(16f, -1f, 16f, 2f, "GroundB");        // x 8..24  (landing + echo gate + exit)

            // Invisible bounds (air walls — collider only, no greybox): keep the player in without
            // visual clutter. The DoorWall above the gate stops the player jumping over the puzzle.
            AirWall(-4.5f, 2.5f, 1f, 12f, "LeftBound");      // x -5
            AirWall(24.5f, 2.5f, 1f, 12f, "RightBound");     // x 24

            // dim flickering lamps (the only mood light; no ceiling)
            BuildLamp(2f, 4f);
            BuildLamp(16f, 4f);

            // fall-out plane: drop below the floor → respawn at the checkpoint
            BuildKillZone();
            Inst("Checkpoint", new Vector3(1f, 1f, 0f));

            // player — recording ENABLED (this is the echo tutorial); control granted by intro
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{EchoBuildUtils.PrefabDir}/Player.prefab");
            var player = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            player.transform.position = new Vector3(-2f, 0.6f, 0f);
            cam.GetComponent<CameraFollow>().target = player.transform;

            // --- echo gate: record yourself onto the plate so the echo holds the door open ---
            Sprite[] doorFrames = LoadDoorFrames();
            var plate = Inst("PressurePlate", new Vector3(12f, 0.13f, 0f));
            var doorGO = Inst("Door", new Vector3(15.5f, 1f, 0f));
            AirWall(15.5f, 4.5f, 0.7f, 5f, "DoorWall");      // invisible wall above the door (no jump-over)
            var gate = doorGO.GetComponent<Door>();
            gate.requiredPlates = new[] { plate.GetComponent<PressurePlate>() };
            gate.latch = false;                               // open ONLY while the plate is held → the echo is required (no self-bypass)
            gate.openFrames = doorFrames;                     // proper open animation (not the legacy slide-up)

            // --- in-level hints + first memory (all through the terminal) ---
            StoryZone(-0.5f, 1.8f, false, "[ ←  → ]   move");
            StoryZone(4.5f, 2.2f, false, "[ SPACE ]   jump");
            StoryZone(10.5f, 2.4f, false,
                "> anomaly: a second signal, moving like me.",
                "[ HOLD R ]   record — release to spawn your echo. it repeats what you did.");
            StoryZone(19f, 2.4f, true,
                "> fragment recovered",
                "\"I have done this before. Many times.\"");

            // --- exit portal → Level_01 ---
            BuildExitDoor(22f, doorFrames);

            // --- opening boot sequence (black reveal + terminal-style log) ---
            BuildIntro(player);

            EchoBuildUtils.EnsureFolder(EchoBuildUtils.SceneDir);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, EchoBuildUtils.Level0ScenePath);
        }

        static GameObject BuildCamera()
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            go.transform.position = new Vector3(0f, 1f, -10f);
            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 4.3f;   // intimate opening (smaller = closer; was 6 → 5 → 4.3)
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = EchoBuildUtils.ColBackground;
            go.AddComponent<AudioListener>();
            go.AddComponent<UniversalAdditionalCameraData>().renderPostProcessing = true;
            go.AddComponent<CameraShake>();
            go.AddComponent<CameraFollow>();
            return go;
        }

        static void BuildGameManager()
        {
            var go = new GameObject("GameManager");
            var gm = go.AddComponent<GameManager>();
            gm.levelName = "Awakening";
            gm.nextSceneName = "Level_01";
            gm.totalFragments = 0;
            // No HUD / victory / vignette refs — Level 0 stays deliberately bare. GameManager is
            // here only to own SetCheckpoint + RespawnPlayer (every UI reference null-guards).
        }

        static void BuildLamp(float x, float y)
        {
            var lampGO = new GameObject("Lamp");
            lampGO.transform.SetParent(levelT, false);
            lampGO.transform.localPosition = new Vector3(x, y, 0f);
            var lamp = EchoBuildUtils.AddPointLight(lampGO, new Color(0.6f, 0.8f, 1f, 1f), 0.9f, 5.5f);
            var fl = lampGO.AddComponent<Flicker>();
            fl.lightTarget = lamp; fl.min = 0.4f; fl.max = 1f; fl.speed = 7f;
        }

        static void BuildKillZone()
        {
            var go = new GameObject("KillZone");
            go.transform.SetParent(levelT, false);
            go.transform.localPosition = new Vector3(10f, -7f, 0f);
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(44f, 2f);   // wide plane under the whole level
            go.AddComponent<KillZone>();
        }

        static Sprite[] LoadDoorFrames() => new[]
        {
            EchoBuildUtils.LoadImportedSprite(DoorPath, "Door_0"),
            EchoBuildUtils.LoadImportedSprite(DoorPath, "Door_1"),
            EchoBuildUtils.LoadImportedSprite(DoorPath, "Door_2"),
            EchoBuildUtils.LoadImportedSprite(DoorPath, "Door_3"),
        };

        static void BuildExitDoor(float x, Sprite[] doorFrames)
        {
            var root = new GameObject("ExitDoor");
            root.transform.SetParent(levelT, false);
            root.transform.localPosition = new Vector3(x, 0f, 0f);

            var trig = root.AddComponent<BoxCollider2D>();
            trig.isTrigger = true;
            trig.size = new Vector2(2.6f, 3.6f);
            trig.offset = new Vector2(0f, 1f);

            var audio = root.AddComponent<AudioSource>();
            audio.playOnAwake = false; audio.spatialBlend = 0f;

            var body = new GameObject("Body");
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, 1f, 0f);
            var bsr = body.AddComponent<SpriteRenderer>();
            bsr.sprite = EchoBuildUtils.LoadSprite("door");
            bsr.sharedMaterial = lit;
            bsr.sortingLayerName = "Environment";
            bsr.sortingOrder = 2;

            var lightGO = new GameObject("DoorLight");
            lightGO.transform.SetParent(root.transform, false);
            lightGO.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            EchoBuildUtils.AddPointLight(lightGO, new Color(0.27f, 1f, 0.67f, 1f), 0.8f, 4f);

            var ad = root.AddComponent<AutoDoorExit>();
            ad.doorBody = body.transform;
            ad.openFrames = doorFrames;                       // animate open (was missing → no animation)
            ad.audioSource = audio;
            ad.openClip = EchoBuildUtils.LoadAudio("doorslide");
            ad.closeClip = EchoBuildUtils.LoadAudio("doorclose");
            ad.targetScene = "Level_01";
        }

        static void BuildIntro(GameObject player)
        {
            Canvas canvas = EchoBuildUtils.CreateOverlayCanvas("IntroCanvas", 60);
            var black = EchoBuildUtils.CreateImage("Black", canvas.transform, null, Color.black);
            EchoBuildUtils.FullStretch(black.rectTransform);
            black.raycastTarget = false;
            var line = EchoBuildUtils.CreateTitleText("Line", canvas.transform, "", 58f, Color.white, TextAlignmentOptions.Center);
            EchoBuildUtils.Place(line.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1500f, 240f));

            var introGO = new GameObject("Level0Intro");
            var intro = introGO.AddComponent<Level0Intro>();
            intro.player = player.GetComponent<PlayerController>();
            intro.blackImage = black;
            intro.lineText = line;
            intro.lines = new[]
            {
                "> SYSTEM REBOOT …",
                "> memory core: 94% CORRUPTED",
                "> motor functions: ONLINE",
                "> … who switched me back on?"
            };
        }

        // ---- gameplay-trigger helper ----
        // A one-shot zone that plays a narrative/hint beat through the terminal the first time the
        // player walks into it. blocking=false → quiet hint (keeps control); true → story beat
        // (freezes control until dismissed).
        static void StoryZone(float x, float width, bool blocking, params string[] lines)
        {
            var go = new GameObject("StoryZone");
            go.transform.SetParent(levelT, false);
            go.transform.localPosition = new Vector3(x, 1.2f, 0f);
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(width, 3.5f);
            var st = go.AddComponent<StoryTrigger>();
            st.lines = lines;
            st.blocking = blocking;
            st.oneShot = true;
        }

        static GameObject Inst(string prefabName, Vector3 pos)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{EchoBuildUtils.PrefabDir}/{prefabName}.prefab");
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.transform.SetParent(levelT, false);
            go.transform.localPosition = pos;
            return go;
        }

        // Invisible bound: collider only on the Ground layer, no greybox fill.
        static GameObject AirWall(float cx, float cy, float w, float h, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(levelT, false);
            go.transform.localPosition = new Vector3(cx, cy, 0f);
            go.layer = groundLayer;
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(w, h);
            return go;
        }

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
    }
}
