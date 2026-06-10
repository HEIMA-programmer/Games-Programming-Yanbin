using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace EchoShift.EditorTools
{
    /// <summary>
    /// Builds the four story-cutscene scenes (Cut_00..Cut_03) from the AI-generated
    /// slides in Assets/Art/images. CURRENT tooling (not legacy): re-running it simply
    /// regenerates the cutscene scenes from the table below — captions and pacing live
    /// here, the playback logic lives in CutscenePlayer.
    /// Flow: MainMenu -> Cut_00 -> L0 -> Cut_01 -> L1 -> Cut_02 -> L2 -> Cut_03 (marks
    /// the game completed) -> MainMenu. Level Select still jumps straight to levels.
    /// </summary>
    public static class EchoCutscenes
    {
        const string ImgDir = "Assets/Art/images/";
        const string BlipPath = "Assets/Audio/typeblip.wav";
        const string FontPath = "Assets/Fonts/VT323-Regular SDF.asset";

        class Act
        {
            public string scenePath;
            public string nextScene;
            public bool markCompleted;
            public string[] images;
            public string[][] lines;
            public string music;        // Assets path; Cut_00 shares the menu clip so it carries over seamlessly
            public float musicScale = 0.8f;
        }

        // Caption grammar matches the in-game terminal: '>' = SYSTEM (white),
        // '"' = the child's PLAYBACK recordings (cyan via CutscenePlayer).
        static readonly Act[] Acts =
        {
            new Act
            {
                scenePath = "Assets/_Scenes/Cut_00.unity", nextScene = "Level_00",
                // the cycle's OPENING piece — and it puts ALL four cutscenes in the
                // music-box family: chip = the machine's present, music box = the
                // child's recordings. The menu chip stays a "UI sound" apart.
                music = "Assets/Audio/Music/cut0_distant_lands.mp3", musicScale = 0.75f,
                images = new[] { "1-1.png", "1-2.png", "1-3.png" },
                lines = new[]
                {
                    new[] { "> ARCHIVE PLAYBACK — day 001", "\"hold still... there. your heart goes here.\"" },
                    new[] { "\"watch! I press the button — and there's TWO of me!\"" },
                    new[] { "> last entry: 1,460 days ago", "> reboot reason: unknown" },
                },
            },
            new Act
            {
                scenePath = "Assets/_Scenes/Cut_01.unity", nextScene = "Level_01",
                music = "Assets/Audio/Music/cut1_pleading_child.mp3", musicScale = 0.75f,
                images = new[] { "2-1.png", "2-2.png" },
                lines = new[]
                {
                    new[] { "> trace detected: small. barefoot. familiar.", "> following." },
                    new[] { "\"race you to the playroom!\"" },
                },
            },
            new Act
            {
                scenePath = "Assets/_Scenes/Cut_02.unity", nextScene = "Level_02",
                music = "Assets/Audio/Music/cut2_poet_speaks.mp3", musicScale = 0.75f,
                images = new[] { "3-1.png", "3-2.png" },
                lines = new[]
                {
                    new[] { "\"shh! it NEVER finds me behind the toy box. never ever.\"" },
                    new[] { "> the game went on without us." },
                },
            },
            new Act
            {
                scenePath = "Assets/_Scenes/Cut_03.unity", nextScene = "MainMenu", markCompleted = true,
                music = "Assets/Audio/Music/cut3_reverie.mp3", musicScale = 0.85f,
                images = new[] { "4-1.png", "4-2.png", "4-3.png" },
                lines = new[]
                {
                    new[] { "> final trace recovered.", "\"found you.\"" },
                    new[] { "> archive complete — a memory in two bodies." },
                    new[] { "> playback never ends.", "thank you for playing." },
                },
            },
        };

        [MenuItem("Aftertrace/Build Cutscene Scenes")]
        public static void Build()
        {
            EnsureTypeBlip();
            foreach (var act in Acts) BuildOne(act);
            RegisterScenes();
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(Acts[0].scenePath);
            Debug.Log("[EchoCutscenes] Built Cut_00..Cut_03 and registered them in Build Settings.");
        }

        static void BuildOne(Act act)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGO = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = camGO.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.orthographic = true;
            camGO.AddComponent<AudioListener>();
            camGO.transform.position = new Vector3(0f, 0f, -10f);

            // the riveted screen frame — same framing language as gameplay
            var framePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ScreenFrameCanvas.prefab");
            if (framePrefab != null) PrefabUtility.InstantiatePrefab(framePrefab);

            var canvasGO = new GameObject("CutsceneCanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 40;   // under the frame (50): rivets stay on top
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            var sfx = canvasGO.AddComponent<AudioSource>();
            sfx.playOnAwake = false;
            sfx.spatialBlend = 0f;

            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);

            var rootGO = new GameObject("SlideRoot");
            rootGO.transform.SetParent(canvasGO.transform, false);
            FullStretch(rootGO.AddComponent<RectTransform>());
            var group = rootGO.AddComponent<CanvasGroup>();

            var imgGO = new GameObject("Slide");
            imgGO.transform.SetParent(rootGO.transform, false);
            var img = imgGO.AddComponent<Image>();
            img.preserveAspect = true;
            img.raycastTarget = false;
            var imgRT = img.rectTransform;
            imgRT.anchorMin = imgRT.anchorMax = new Vector2(0.5f, 0.5f);
            imgRT.sizeDelta = new Vector2(1920f, 1080f);

            // black band behind the caption — white type stays readable over bright art
            // (chalk grids, the open-door light); black IS this art style's canvas colour
            var plateGO = new GameObject("CaptionPlate");
            plateGO.transform.SetParent(rootGO.transform, false);
            var plate = plateGO.AddComponent<Image>();
            plate.color = new Color(0f, 0f, 0f, 0.78f);
            plate.raycastTarget = false;
            var plateRT = plate.rectTransform;
            plateRT.anchorMin = plateRT.anchorMax = new Vector2(0.5f, 0f);
            plateRT.pivot = new Vector2(0.5f, 0f);
            plateRT.anchoredPosition = new Vector2(0f, 84f);
            plateRT.sizeDelta = new Vector2(1640f, 190f);

            var capGO = new GameObject("Caption");
            capGO.transform.SetParent(rootGO.transform, false);
            var cap = capGO.AddComponent<TextMeshProUGUI>();
            if (font != null) cap.font = font;
            cap.fontSize = 46f;
            cap.color = new Color(0.92f, 0.96f, 1f, 1f);
            cap.alignment = TextAlignmentOptions.Center;
            cap.outlineWidth = 0.22f;       // keeps captions readable over bright slide areas
            cap.outlineColor = Color.black;
            cap.raycastTarget = false;
            var capRT = cap.rectTransform;
            capRT.anchorMin = capRT.anchorMax = new Vector2(0.5f, 0f);
            capRT.pivot = new Vector2(0.5f, 0f);
            capRT.anchoredPosition = new Vector2(0f, 96f);
            capRT.sizeDelta = new Vector2(1560f, 170f);

            var hintGO = new GameObject("Hint");
            hintGO.transform.SetParent(canvasGO.transform, false);
            var hint = hintGO.AddComponent<TextMeshProUGUI>();
            if (font != null) hint.font = font;
            hint.fontSize = 30f;
            hint.color = new Color(1f, 1f, 1f, 0.3f);
            hint.alignment = TextAlignmentOptions.Right;
            hint.text = "[ SPACE ]";        // same key-prompt style as the L0 tutorial
            hint.raycastTarget = false;
            var hintRT = hint.rectTransform;
            hintRT.anchorMin = hintRT.anchorMax = new Vector2(1f, 0f);
            hintRT.pivot = new Vector2(1f, 0f);
            hintRT.anchoredPosition = new Vector2(-44f, 26f);
            hintRT.sizeDelta = new Vector2(280f, 40f);

            if (!string.IsNullOrEmpty(act.music))
            {
                var music = new GameObject("SceneMusic").AddComponent<SceneMusic>();
                music.track = AssetDatabase.LoadAssetAtPath<AudioClip>(act.music);
                music.volumeScale = act.musicScale;
            }

            var player = new GameObject("Cutscene").AddComponent<CutscenePlayer>();
            player.slideImage = img;
            player.slideGroup = group;
            player.caption = cap;
            player.captionPlate = plate;
            player.audioSource = sfx;
            player.typeClip = AssetDatabase.LoadAssetAtPath<AudioClip>(BlipPath);
            player.nextScene = act.nextScene;
            player.markCompleted = act.markCompleted;
            player.slides = new CutsceneSlide[act.images.Length];
            for (int i = 0; i < act.images.Length; i++)
            {
                player.slides[i] = new CutsceneSlide
                {
                    image = ImportSlide(ImgDir + act.images[i]),
                    lines = act.lines[i],
                };
            }

            EditorSceneManager.SaveScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene(), act.scenePath);
        }

        static void FullStretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        // Crisp pixel import for the AI slides (already 1-bit + cyan; just keep them sharp).
        static Sprite ImportSlide(string path)
        {
            var ti = (TextureImporter)AssetImporter.GetAtPath(path);
            bool dirty = false;
            if (ti.textureType != TextureImporterType.Sprite) { ti.textureType = TextureImporterType.Sprite; dirty = true; }
            if (ti.filterMode != FilterMode.Point) { ti.filterMode = FilterMode.Point; dirty = true; }
            if (ti.textureCompression != TextureImporterCompression.Uncompressed) { ti.textureCompression = TextureImporterCompression.Uncompressed; dirty = true; }
            if (ti.mipmapEnabled) { ti.mipmapEnabled = false; dirty = true; }
            if (ti.maxTextureSize < 2048) { ti.maxTextureSize = 2048; dirty = true; }
            if (dirty) ti.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        // Tiny terminal key blip, synthesized like the rest of the EchoAudio pipeline:
        // 50 ms square-wave chirp 1300->850 Hz with a fast exponential decay.
        static void EnsureTypeBlip()
        {
            if (File.Exists(BlipPath)) return;
            const int rate = 22050;
            const float dur = 0.05f;
            int n = (int)(rate * dur);
            var samples = new short[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / rate;
                float freq = Mathf.Lerp(1300f, 850f, t / dur);
                float square = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * freq * t));
                float env = Mathf.Exp(-t * 70f);
                samples[i] = (short)(square * env * 0.28f * short.MaxValue);
            }
            using (var bw = new BinaryWriter(new FileStream(BlipPath, FileMode.Create)))
            {
                int dataLen = n * 2;
                bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); bw.Write(36 + dataLen);
                bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); bw.Write(16);
                bw.Write((short)1); bw.Write((short)1); bw.Write(rate); bw.Write(rate * 2);
                bw.Write((short)2); bw.Write((short)16);
                bw.Write(System.Text.Encoding.ASCII.GetBytes("data")); bw.Write(dataLen);
                foreach (var s in samples) bw.Write(s);
            }
            AssetDatabase.ImportAsset(BlipPath);
        }

        static void RegisterScenes()
        {
            var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach (var act in Acts)
                if (list.FindIndex(s => s.path == act.scenePath) < 0)
                    list.Add(new EditorBuildSettingsScene(act.scenePath, true));
            EditorBuildSettings.scenes = list.ToArray();
        }
    }
}
