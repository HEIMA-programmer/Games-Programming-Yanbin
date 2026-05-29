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
    /// Builds Level_00 "Awakening": a tiny dim room + corridor with one jump gap and an
    /// auto-opening exit door, plus the opening text sequence. No echo / HUD / enemies.
    /// </summary>
    public static class EchoLevel0
    {
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

            var levelGO = new GameObject("Level");
            levelT = levelGO.transform;
            EchoTilemap.BeginTerrain(levelT, lit);

            // room + corridor (top y = 0); gap x[6.5..8.2] forces a jump
            SolidBlock(0f, -1f, 7f, 2f, "GroundRoom");        // x -3.5..3.5
            SolidBlock(5f, -1f, 3f, 2f, "CorridorA");         // x 3.5..6.5
            SolidBlock(11.35f, -1f, 6.3f, 2f, "CorridorB");   // x 8.2..14.5
            SolidBlock(7.35f, -3f, 3f, 1f, "SafetyFloor");    // catch under the gap (top y -2.5)
            SolidBlock(-3f, 2.5f, 1f, 9f, "LeftWall");
            SolidBlock(0f, 5f, 8f, 1f, "Ceiling");

            // dim flickering ceiling lamp (only light source)
            var lampGO = new GameObject("CeilingLamp");
            lampGO.transform.SetParent(levelT, false);
            lampGO.transform.localPosition = new Vector3(0f, 4f, 0f);
            var lamp = EchoBuildUtils.AddPointLight(lampGO, new Color(0.6f, 0.8f, 1f, 1f), 0.9f, 5.5f);
            var fl = lampGO.AddComponent<Flicker>();
            fl.lightTarget = lamp; fl.min = 0.4f; fl.max = 1f; fl.speed = 7f;

            // player (recording disabled; control granted by Level0Intro)
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{EchoBuildUtils.PrefabDir}/Player.prefab");
            var player = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            player.transform.position = new Vector3(-1f, 0.6f, 0f);
            var rec = player.GetComponent<EchoRecorder>();
            if (rec != null) rec.enabled = false;
            cam.GetComponent<CameraFollow>().target = player.transform;

            BuildExitDoor(13f);
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
            cam.orthographicSize = 6f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = EchoBuildUtils.ColBackground;
            go.AddComponent<AudioListener>();
            go.AddComponent<UniversalAdditionalCameraData>().renderPostProcessing = true;
            go.AddComponent<CameraShake>();
            go.AddComponent<CameraFollow>();
            return go;
        }

        static void BuildExitDoor(float x)
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

            var panel = EchoBuildUtils.SpriteGO("Panel", EchoBuildUtils.LoadSprite("glow"), "Environment", 1, root.transform, unlit,
                new Color(1f, 1f, 1f, 0.8f));  // 1-Bit: white door glow
            panel.transform.localPosition = new Vector3(0.6f, 1.2f, 0f);
            panel.transform.localScale = Vector3.one * 0.6f;

            var lightGO = new GameObject("DoorLight");
            lightGO.transform.SetParent(root.transform, false);
            lightGO.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            EchoBuildUtils.AddPointLight(lightGO, new Color(0.27f, 1f, 0.67f, 1f), 0.8f, 4f);

            var ad = root.AddComponent<AutoDoorExit>();
            ad.doorBody = body.transform;
            ad.audioSource = audio;
            ad.slideClip = EchoBuildUtils.LoadAudio("doorslide");
            ad.targetScene = "Level_01";
        }

        static void BuildIntro(GameObject player)
        {
            Canvas canvas = EchoBuildUtils.CreateOverlayCanvas("IntroCanvas", 60);
            var black = EchoBuildUtils.CreateImage("Black", canvas.transform, null, Color.black);
            EchoBuildUtils.FullStretch(black.rectTransform);
            black.raycastTarget = false;
            var line = EchoBuildUtils.CreateText("Line", canvas.transform, "", 60f, Color.white, TextAlignmentOptions.Center);
            EchoBuildUtils.Place(line.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1400f, 240f));

            var introGO = new GameObject("Level0Intro");
            var intro = introGO.AddComponent<Level0Intro>();
            intro.player = player.GetComponent<PlayerController>();
            intro.blackImage = black;
            intro.lineText = line;
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
    }
}
