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

            // Menu background removed — to be hand-designed later. menuPs = null is null-safe
            // in MainMenuController (ambientParticles is null-checked before use).
            ParticleSystem menuPs = null;
            var warmGlow = new GameObject("WarmGlow");
            warmGlow.transform.position = new Vector3(0f, 1f, 0f);
            EchoBuildUtils.AddPointLight(warmGlow, new Color(1f, 0.6f, 0.3f, 1f), 0.8f, 12f);
            warmGlow.SetActive(false);
            BuildUI(menuPs, warmGlow);

            EchoBuildUtils.EnsureFolder(EchoBuildUtils.SceneDir);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, EchoBuildUtils.MenuScenePath);
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

            // Step 110 = button height 92 + 18 px gap (85 used to OVERLAP the 92-tall buttons by 7 px).
            Button startBtn = MakeButton("Start", root, "Start Game", buttonBg, uiSource, hover, click, -30f);
            Button howBtn = MakeButton("HowTo", root, "How to Play", buttonBg, uiSource, hover, click, -140f);
            Button selectBtn = MakeButton("Select", root, "Level Select", buttonBg, uiSource, hover, click, -250f);
            Button quitBtn = MakeButton("Quit", root, "Quit", buttonBg, uiSource, hover, click, -360f);

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
            string[] scenes = { "Level_00", "Level_01", "Level_02" };
            string[] labels = { "0  ·  Awakening", "1  ·  Playroom", "2  ·  Hide and Seek" };
            for (int i = 0; i < scenes.Length; i++)
            {
                Button lb = MakeButton("Lv" + i, selPanel.transform, labels[i], buttonBg, uiSource, hover, click, 145f - i * 110f);
                lb.gameObject.AddComponent<LevelButton>().sceneName = scenes[i];
            }
            Button selBack = MakeButton("Back", selPanel.transform, "Back", buttonBg, uiSource, hover, click, -205f);
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
