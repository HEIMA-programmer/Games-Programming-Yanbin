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
        static readonly Color Cyan = new Color(0f, 0.83f, 1f, 1f);
        static readonly Color Soft = new Color(0.667f, 0.867f, 1f, 1f);

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
            // a couple of soft equipment silhouettes + distant lights
            var mid = EchoBuildUtils.SpriteGO("Equip", EchoBuildUtils.LoadSprite("bgequip"),
                "Default", -20, null, EchoBuildUtils.LoadMaterial(EchoMaterials.LitName), new Color(0.4f, 0.5f, 0.7f, 1f));
            mid.transform.position = new Vector3(-6f, -2.5f, 3f);
            mid.transform.localScale = Vector3.one * 1.6f;

            EchoBuildUtils.AddPointLight(NewAt("AmbientA", new Vector3(-6f, 3f, 0f)), Cyan, 0.5f, 7f);
            EchoBuildUtils.AddPointLight(NewAt("AmbientB", new Vector3(6f, -2f, 0f)), new Color(0.4f, 0.6f, 1f, 1f), 0.4f, 6f);
            EchoBuildUtils.AddPointLight(NewAt("AmbientC", new Vector3(0f, 4f, 0f)), Cyan, 0.3f, 8f);

            var go = new GameObject("MenuParticles");
            go.transform.position = new Vector3(0f, 0f, 2f);
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true; main.duration = 10f; main.startLifetime = 9f; main.startSpeed = 0.3f;
            main.startSize = 0.12f; main.startColor = new Color(0.4f, 0.6f, 1f, 0.5f);
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

            // title glow + text (upper third)
            var glow = EchoBuildUtils.CreateImage("TitleGlow", root, EchoBuildUtils.LoadSprite("glow"), new Color(0f, 0.83f, 1f, 0.32f));
            EchoBuildUtils.Place(glow.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 250f), new Vector2(1100f, 360f));
            glow.raycastTarget = false;

            var title = EchoBuildUtils.CreateText("Title", root, "AFTERTRACE", 130f, Cyan, TextAlignmentOptions.Center);
            EchoBuildUtils.Place(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 255f), new Vector2(1500f, 260f));
            title.fontStyle = FontStyles.Bold;
            title.characterSpacing = 14f;

            var subtitle = EchoBuildUtils.CreateText("Subtitle", root, "a memory in two bodies", 32f, new Color(Soft.r, Soft.g, Soft.b, 0.7f), TextAlignmentOptions.Center);
            EchoBuildUtils.Place(subtitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 150f), new Vector2(1000f, 60f));

            Button startBtn = MakeButton("Start", root, "Start Game", buttonBg, uiSource, hover, click, -30f);
            Button howBtn = MakeButton("HowTo", root, "How to Play", buttonBg, uiSource, hover, click, -115f);
            Button selectBtn = MakeButton("Select", root, "Level Select", buttonBg, uiSource, hover, click, -200f);
            Button quitBtn = MakeButton("Quit", root, "Quit", buttonBg, uiSource, hover, click, -285f);

            // How to Play overlay
            var howPanel = new GameObject("HowToPanel");
            howPanel.transform.SetParent(root, false);
            EchoBuildUtils.FullStretch(howPanel.AddComponent<RectTransform>());
            var dim = EchoBuildUtils.CreateImage("Dim", howPanel.transform, null, new Color(0.01f, 0.03f, 0.07f, 0.9f));
            EchoBuildUtils.FullStretch(dim.rectTransform);
            var howTitle = EchoBuildUtils.CreateText("Title", howPanel.transform, "HOW TO PLAY", 64f, Cyan, TextAlignmentOptions.Center);
            EchoBuildUtils.Place(howTitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 250f), new Vector2(900f, 90f));
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
            var selDim = EchoBuildUtils.CreateImage("Dim", selPanel.transform, null, new Color(0.01f, 0.03f, 0.07f, 0.9f));
            EchoBuildUtils.FullStretch(selDim.rectTransform);
            var selTitle = EchoBuildUtils.CreateText("Title", selPanel.transform, "SELECT LEVEL", 64f, Cyan, TextAlignmentOptions.Center);
            EchoBuildUtils.Place(selTitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 250f), new Vector2(900f, 90f));
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
            EchoBuildUtils.Place((RectTransform)b.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(360f, 74f));
            return b;
        }
    }
}
