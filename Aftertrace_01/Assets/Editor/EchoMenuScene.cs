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
    /// <summary>Builds the MainMenu scene: atmospheric lab backdrop + title + buttons + How-to-Play.</summary>
    public static class EchoMenuScene
    {
        static readonly Color Cyan = new Color(0f, 0.83f, 1f, 1f);          // kept for AmbientLight
        static readonly Color Soft = new Color(0.667f, 0.867f, 1f, 1f);
        // 1-Bit menu text — pure white pop with a subtle cool tint.
        static readonly Color MenuText  = new Color(0.95f, 0.98f, 1f, 1f);
        static readonly Color MenuTextDim = new Color(0.78f, 0.84f, 0.92f, 0.85f);

        public static void Build()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGO = new GameObject("Main Camera") { tag = "MainCamera" };
            camGO.transform.position = new Vector3(0f, 0f, -10f);
            var cam = camGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6.5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = EchoBuildUtils.ColBackground;
            camGO.AddComponent<AudioListener>();
            camGO.AddComponent<UniversalAdditionalCameraData>().renderPostProcessing = true;

            EchoBuildUtils.AddGlobalLight(new GameObject("Global Light 2D"), Color.white, 0.22f);

            var vol = new GameObject("Bloom Volume").AddComponent<Volume>();
            vol.isGlobal = true;
            vol.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(EchoBuildUtils.BloomProfilePath);

            ParticleSystem menuPs = BuildBackground();
            var warmGlow = new GameObject("WarmGlow");
            warmGlow.transform.position = new Vector3(0f, 1f, 0f);
            EchoBuildUtils.AddPointLight(warmGlow, new Color(1f, 0.6f, 0.3f, 1f), 0.8f, 12f);
            warmGlow.SetActive(false);
            BuildUI(menuPs, warmGlow);

            EchoBuildUtils.EnsureFolder(EchoBuildUtils.SceneDir);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, EchoBuildUtils.MenuScenePath);
        }

        static ParticleSystem BuildBackground()
        {
            // Dense diorama — many elements arranged in two side "scenes" + top accents.
            // Button column = world x ±2.5, y ~+0.5 down to ~-3. Decorations live OUTSIDE.
            var unlitMat = EchoBuildUtils.LoadMaterial(EchoMaterials.UnlitName);

            // FAR — single huge planet hero behind everything.
            var hero = EchoBuildUtils.SpriteGO("BG_Hero", EchoBuildUtils.GetBackgroundVariant(4),
                "Default", -30, null, unlitMat, new Color(1f, 1f, 1f, 0.12f));  // faint ghost — full white read as a noisy blob behind the title
            hero.transform.position = new Vector3(0f, 0.8f, 8f);
            hero.transform.localScale = Vector3.one * 3.2f;  // PPU 80→32 (×0.4) keeps size

            // Additional far backdrops in the TOP corners (out of button area).
            var topLeftBg = EchoBuildUtils.SpriteGO("BG_TL", EchoBuildUtils.GetBackgroundVariant(2),
                "Default", -28, null, unlitMat, EchoBuildUtils.TintBgFar);
            topLeftBg.transform.position = new Vector3(-7f, 3.5f, 7f);
            topLeftBg.transform.localScale = Vector3.one * 0.72f;  // PPU 80→32 (×0.4)

            var topRightBg = EchoBuildUtils.SpriteGO("BG_TR", EchoBuildUtils.GetBackgroundVariant(6),
                "Default", -28, null, unlitMat, EchoBuildUtils.TintBgFar);
            topRightBg.transform.position = new Vector3(7f, 3.5f, 7f);
            topRightBg.transform.localScale = Vector3.one * 0.72f;  // PPU 80→32 (×0.4)

            // LEFT SCENE — NPC on box, with tileset detail above (mini diorama).
            var npcLeft = EchoBuildUtils.SpriteGO("NPC_Left", EchoBuildUtils.GetNpcBoy(0),
                "Default", -18, null, unlitMat, EchoBuildUtils.TintNpc);
            npcLeft.transform.position = new Vector3(-7f, -1.3f, 4f);
            npcLeft.transform.localScale = new Vector3(1.73f, 1.73f, 1f);  // PPU 48→32 (×0.667)

            var leftBox = EchoBuildUtils.SpriteGO("LeftBox", EchoBuildUtils.GetBox(4),
                "Default", -16, null, unlitMat, EchoBuildUtils.TintBgNear);
            leftBox.transform.position = new Vector3(-7f, -3.5f, 3f);
            leftBox.transform.localScale = Vector3.one * 1.6f;

            var leftDetail = EchoBuildUtils.SpriteGO("LeftDet", EchoBuildUtils.GetTilesetDetail(40),
                "Default", -22, null, unlitMat, EchoBuildUtils.TintBgMid);
            leftDetail.transform.position = new Vector3(-9f, 1.2f, 5f);
            leftDetail.transform.localScale = Vector3.one * 3.2f;  // PPU 16→32 (×2)

            // RIGHT SCENE — NPC + machine + cable detail (mirrored composition).
            var npcRight = EchoBuildUtils.SpriteGO("NPC_Right", EchoBuildUtils.GetNpcGirl(0),
                "Default", -18, null, unlitMat, EchoBuildUtils.TintNpc);
            npcRight.transform.position = new Vector3(7f, -1.3f, 4f);
            npcRight.transform.localScale = new Vector3(-1.73f, 1.73f, 1f);  // PPU 48→32 (×0.667)

            var rightMachine = EchoBuildUtils.SpriteGO("RightMachine", EchoBuildUtils.GetMachine(0),
                "Default", -16, null, unlitMat, EchoBuildUtils.TintBgNear);
            rightMachine.transform.position = new Vector3(7f, -3.5f, 3f);
            rightMachine.transform.localScale = Vector3.one * 1.2f;  // PPU 48→32 (×0.667)

            var rightDetail = EchoBuildUtils.SpriteGO("RightDet", EchoBuildUtils.GetTilesetDetail(80),
                "Default", -22, null, unlitMat, EchoBuildUtils.TintBgMid);
            rightDetail.transform.position = new Vector3(9f, 1.2f, 5f);
            rightDetail.transform.localScale = Vector3.one * 3.2f;  // PPU 16→32 (×2)

            // BOTTOM STRIP — small ground props well below buttons (y < -3.8 is safe).
            for (int i = 0; i < 5; i++)
            {
                var sp = EchoBuildUtils.GetTilesetDetail(i * 23 + 5);
                if (sp == null) continue;
                var d = EchoBuildUtils.SpriteGO("BotDet_" + i, sp,
                    "Default", -20, null, unlitMat, EchoBuildUtils.TintBgMid);
                d.transform.position = new Vector3(-4.5f + i * 2.25f, -4.5f, 4f);
                d.transform.localScale = Vector3.one * 1.8f;  // PPU 16→32 (×2)
            }

            // TOP ACCENT — two small Icons floating above the title row.
            var iconLeft = EchoBuildUtils.SpriteGO("IconL", EchoBuildUtils.LoadImportedSprite(
                "Assets/Art/Imported/CraftPix1Bit/GUI/Icons.png", "Icons_5"),
                "Default", -19, null, unlitMat, EchoBuildUtils.TintBgNear);
            iconLeft.transform.position = new Vector3(-5f, 4f, 4f);
            iconLeft.transform.localScale = Vector3.one * 1.8f;  // PPU 16→32 (×2)

            var iconRight = EchoBuildUtils.SpriteGO("IconR", EchoBuildUtils.LoadImportedSprite(
                "Assets/Art/Imported/CraftPix1Bit/GUI/Icons.png", "Icons_11"),
                "Default", -19, null, unlitMat, EchoBuildUtils.TintBgNear);
            iconRight.transform.position = new Vector3(5f, 4f, 4f);
            iconRight.transform.localScale = Vector3.one * 1.8f;  // PPU 16→32 (×2)

            // Scene border frame is added on the MenuCanvas (screen-space) in BuildUI.

            EchoBuildUtils.AddPointLight(NewAt("AmbientA", new Vector3(-6f, 3f, 0f)), Cyan, 0.5f, 7f);
            EchoBuildUtils.AddPointLight(NewAt("AmbientB", new Vector3(6f, -2f, 0f)), new Color(0.4f, 0.6f, 1f, 1f), 0.4f, 6f);
            EchoBuildUtils.AddPointLight(NewAt("AmbientC", new Vector3(0f, 4f, 0f)), Cyan, 0.3f, 8f);

            var go = new GameObject("MenuParticles");
            go.transform.position = new Vector3(0f, 0f, 2f);
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true; main.duration = 10f; main.startLifetime = 9f; main.startSpeed = 0.3f;
            main.startSize = 0.12f; main.startColor = new Color(1f, 1f, 1f, 0.5f);  // 1-Bit: white motes
            main.maxParticles = 140; main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = -0.02f; main.playOnAwake = true;
            var em = ps.emission; em.rateOverTime = 12f;
            var sh = ps.shape; sh.enabled = true; sh.shapeType = ParticleSystemShapeType.Box; sh.scale = new Vector3(30f, 16f, 1f);
            var vel = ps.velocityOverLifetime; vel.enabled = true; vel.space = ParticleSystemSimulationSpace.World;
            vel.y = new ParticleSystem.MinMaxCurve(0.22f);
            var col = ps.colorOverLifetime; col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.5f, 0.4f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;
            var rend = go.GetComponent<ParticleSystemRenderer>();
            rend.sharedMaterial = EchoBuildUtils.LoadMaterial(EchoMaterials.ParticleName);
            rend.sortingOrder = -10;
            return ps;
        }

        static GameObject NewAt(string name, Vector3 pos)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            return go;
        }

        static void BuildUI(ParticleSystem menuPs, GameObject warmGlow)
        {
            EchoBuildUtils.EnsureEventSystem();
            Canvas canvas = EchoBuildUtils.CreateOverlayCanvas("MenuCanvas", 10);
            Transform root = canvas.transform;
            var uiSource = canvas.gameObject.AddComponent<AudioSource>();
            uiSource.playOnAwake = false; uiSource.spatialBlend = 0f;
            AudioClip hover = EchoBuildUtils.LoadAudio("uihover");
            AudioClip click = EchoBuildUtils.LoadAudio("uiclick");
            Sprite buttonBg = EchoBuildUtils.LoadSprite("button");

            // Scene border — single closed rounded-rectangle frame.
            EchoBuildUtils.AddSceneFrame(root, new Color(0.92f, 0.96f, 1f, 0.95f), 10f);

            // title glow + text (upper third)
            // 1-Bit: a faint WHITE halo behind the title (no cyan accent).
            var glow = EchoBuildUtils.CreateImage("TitleGlow", root, EchoBuildUtils.LoadSprite("glow"), new Color(1f, 1f, 1f, 0.14f));
            EchoBuildUtils.Place(glow.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 250f), new Vector2(1100f, 360f));
            glow.raycastTarget = false;

            var title = EchoBuildUtils.CreateTitleText("Title", root, "AFTERTRACE", 130f, MenuText, TextAlignmentOptions.Center);
            EchoBuildUtils.Place(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 255f), new Vector2(1500f, 260f));
            title.fontStyle = FontStyles.Bold;
            title.characterSpacing = 14f;
            EchoBuildUtils.ApplyOutline(title, new Color(0f, 0f, 0f, 1f), 0.22f); // crisp black outline (1-Bit)

            var subtitle = EchoBuildUtils.CreateText("Subtitle", root, "a memory in two bodies", 32f, MenuTextDim, TextAlignmentOptions.Center);
            EchoBuildUtils.Place(subtitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 150f), new Vector2(1000f, 60f));

            Button startBtn = MakeButton("Start", root, "Start Game", buttonBg, uiSource, hover, click, -30f);
            Button howBtn = MakeButton("HowTo", root, "How to Play", buttonBg, uiSource, hover, click, -115f);
            Button selectBtn = MakeButton("Select", root, "Level Select", buttonBg, uiSource, hover, click, -200f);
            Button quitBtn = MakeButton("Quit", root, "Quit", buttonBg, uiSource, hover, click, -285f);

            // How to Play overlay
            var howPanel = new GameObject("HowToPanel");
            howPanel.transform.SetParent(root, false);
            EchoBuildUtils.FullStretch(howPanel.AddComponent<RectTransform>());
            var dim = EchoBuildUtils.CreateImage("Dim", howPanel.transform, null, new Color(0.01f, 0.03f, 0.07f, 1f));
            EchoBuildUtils.FullStretch(dim.rectTransform);
            var howTitle = EchoBuildUtils.CreateTitleText("Title", howPanel.transform, "HOW TO PLAY", 64f, MenuText, TextAlignmentOptions.Center);
            EchoBuildUtils.Place(howTitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 250f), new Vector2(900f, 90f));
            EchoBuildUtils.ApplyOutline(howTitle, new Color(0f, 0f, 0f, 1f), 0.20f);
            var body = EchoBuildUtils.CreateText("Body", howPanel.transform,
                "A / D   or   ← / →      Move\n\nSpace / W      Jump\n\nR  (hold)      Record an echo, release to replay\n\nESC      Pause",
                38f, new Color(0.85f, 0.95f, 1f, 1f), TextAlignmentOptions.Center);
            EchoBuildUtils.Place(body.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(1100f, 380f));
            Button backBtn = MakeButton("Back", howPanel.transform, "Back", buttonBg, uiSource, hover, click, -250f);
            howPanel.SetActive(false);

            // Level Select overlay
            var selPanel = new GameObject("LevelSelectPanel");
            selPanel.transform.SetParent(root, false);
            EchoBuildUtils.FullStretch(selPanel.AddComponent<RectTransform>());
            var selDim = EchoBuildUtils.CreateImage("Dim", selPanel.transform, null, new Color(0.01f, 0.03f, 0.07f, 1f));
            EchoBuildUtils.FullStretch(selDim.rectTransform);
            var selTitle = EchoBuildUtils.CreateTitleText("Title", selPanel.transform, "SELECT LEVEL", 64f, MenuText, TextAlignmentOptions.Center);
            EchoBuildUtils.Place(selTitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 250f), new Vector2(900f, 90f));
            EchoBuildUtils.ApplyOutline(selTitle, new Color(0f, 0f, 0f, 1f), 0.20f);
            string[] scenes = { "Level_00", "Level_01", "Level_02", "Level_03" };
            string[] labels = { "0  ·  Awakening", "1  ·  Sector 01", "2  ·  Deep Labs", "3  ·  The Core" };
            for (int i = 0; i < scenes.Length; i++)
            {
                Button lb = MakeButton("Lv" + i, selPanel.transform, labels[i], buttonBg, uiSource, hover, click, 120f - i * 90f);
                lb.gameObject.AddComponent<LevelButton>().sceneName = scenes[i];
            }
            Button selBack = MakeButton("Back", selPanel.transform, "Back", buttonBg, uiSource, hover, click, -260f);
            selPanel.SetActive(false);

            var ctrl = canvas.gameObject.AddComponent<MainMenuController>();
            ctrl.startButton = startBtn;
            ctrl.howToButton = howBtn;
            ctrl.quitButton = quitBtn;
            ctrl.backButton = backBtn;
            ctrl.howToPanel = howPanel;
            ctrl.firstLevelScene = "Level_00";
            ctrl.ambientParticles = menuPs;
            ctrl.warmGlow = warmGlow;
            ctrl.selectButton = selectBtn;
            ctrl.levelSelectPanel = selPanel;
            ctrl.selectBackButton = selBack;
        }

        static Button MakeButton(string name, Transform parent, string label, Sprite bg,
            AudioSource src, AudioClip hover, AudioClip click, float y)
        {
            Button b = EchoBuildUtils.CreateButton(name, parent, label, bg, src, hover, click);
            EchoBuildUtils.Place((RectTransform)b.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(460f, 92f));
            return b;
        }
    }
}
