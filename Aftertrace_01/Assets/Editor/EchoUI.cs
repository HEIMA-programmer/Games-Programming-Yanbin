using EchoShift;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EchoShift.EditorTools
{
    /// <summary>
    /// Builds the shared in-game UI canvas (HUD + pause + victory) and returns wired
    /// component references for the GameManager. Uses EchoBuildUtils UI helpers.
    /// </summary>
    public static class EchoUI
    {
        public class GameplayUIRefs
        {
            public HUDController hud;
            public PauseMenu pause;
            public VictoryScreen victory;
            public RecordingVignette vignette;
            public Image flash;
            public Image hitFlash;
        }

        // 1-Bit: all UI text is white. (Names kept to avoid churn at the call sites.)
        static readonly Color Cyan = Color.white;
        static readonly Color Soft = Color.white;
        static readonly Color Gold = Color.white;

        public static GameplayUIRefs BuildGameplayCanvas()
        {
            var refs = new GameplayUIRefs();
            EchoBuildUtils.EnsureEventSystem();
            Canvas canvas = EchoBuildUtils.CreateOverlayCanvas("GameplayCanvas", 50);
            Transform root = canvas.transform;

            var uiSource = canvas.gameObject.AddComponent<AudioSource>();
            uiSource.playOnAwake = false;
            uiSource.spatialBlend = 0f;
            AudioClip hover = EchoBuildUtils.LoadAudio("uihover");
            AudioClip click = EchoBuildUtils.LoadAudio("uiclick");
            AudioClip pauseClip = EchoBuildUtils.LoadAudio("pauseopen");
            Sprite buttonBg = EchoBuildUtils.LoadSprite("button");

            // ---- recording vignette + white/red flashes ----
            var vigImg = EchoBuildUtils.CreateImage("RecordVignette", root, EchoBuildUtils.LoadSprite("vignette"), new Color(1f, 1f, 1f, 0f));
            EchoBuildUtils.FullStretch(vigImg.rectTransform);
            vigImg.raycastTarget = false;
            refs.vignette = vigImg.gameObject.AddComponent<RecordingVignette>();
            refs.vignette.image = vigImg;

            refs.flash = EchoBuildUtils.CreateImage("Flash", root, null, new Color(1f, 1f, 1f, 0f));
            EchoBuildUtils.FullStretch(refs.flash.rectTransform);
            refs.flash.raycastTarget = false;

            refs.hitFlash = EchoBuildUtils.CreateImage("HitFlash", root, null, new Color(1f, 1f, 1f, 0f));  // 1-Bit: white damage flash
            EchoBuildUtils.FullStretch(refs.hitFlash.rectTransform);
            refs.hitFlash.raycastTarget = false;

            // ---- Scene border — single closed rounded-rectangle frame (1-Bit demo art look) ----
            EchoBuildUtils.AddSceneFrame(root, new Color(0.92f, 0.96f, 1f, 0.92f), 10f);

            // ---- HUD (gem icon + "X / Y" counter top-left) ----
            var hud = canvas.gameObject.AddComponent<HUDController>();
            Sprite gem = EchoBuildUtils.LoadSprite("fragment");

            var icon = EchoBuildUtils.CreateImage("FragIcon", root, gem, Color.white);
            EchoBuildUtils.Place(icon.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(48f, -52f), new Vector2(54f, 54f));
            icon.raycastTarget = false;
            hud.iconImage = icon;

            var counter = EchoBuildUtils.CreateTitleText("FragCounter", root, "0 / 3", 44f, Color.white, TextAlignmentOptions.Left);
            EchoBuildUtils.Place(counter.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(95f, -52f), new Vector2(160f, 56f));
            counter.fontStyle = FontStyles.Bold;
            counter.characterSpacing = 4f;
            EchoBuildUtils.ApplyOutline(counter, new Color(0f, 0.1f, 0.18f, 0.95f), 0.18f);
            hud.counterText = counter;

            var rText = EchoBuildUtils.CreateText("RecordR", root, "R", 40f, new Color(1f, 1f, 1f, 0.22f), TextAlignmentOptions.Center);
            EchoBuildUtils.Place(rText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-44f, -44f), new Vector2(64f, 64f));
            hud.recordIndicator = rText;
            refs.hud = hud;

            // ---- Pause panel ----
            var pausePanel = NewPanel("PausePanel", root, new Color(0.02f, 0.04f, 0.09f, 0.72f));
            var pausedTitle = EchoBuildUtils.CreateTitleText("PausedTitle", pausePanel.transform, "PAUSED", 70f, Cyan, TextAlignmentOptions.Center);
            EchoBuildUtils.Place(pausedTitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 200f), new Vector2(720f, 100f));
            EchoBuildUtils.ApplyOutline(pausedTitle, new Color(0f, 0.05f, 0.12f, 0.95f), 0.16f);
            Button resumeBtn = MakeMenuButton("Resume", pausePanel.transform, "Resume", buttonBg, uiSource, hover, click, 70f);
            Button restartBtn = MakeMenuButton("Restart", pausePanel.transform, "Restart Level", buttonBg, uiSource, hover, click, -20f);
            Button pmenuBtn = MakeMenuButton("Menu", pausePanel.transform, "Main Menu", buttonBg, uiSource, hover, click, -110f);
            var pause = canvas.gameObject.AddComponent<PauseMenu>();
            pause.panelRoot = pausePanel;
            pause.resumeButton = resumeBtn;
            pause.restartButton = restartBtn;
            pause.menuButton = pmenuBtn;
            pause.sfxSource = uiSource;
            pause.openClip = pauseClip;
            pausePanel.SetActive(false);
            refs.pause = pause;

            // ---- Victory panel ----
            var vicPanel = NewPanel("VictoryPanel", root, new Color(0.01f, 0.03f, 0.07f, 0.8f));
            var narrative = EchoBuildUtils.CreateText("Narrative", vicPanel.transform, "", 50f, Soft, TextAlignmentOptions.Center);
            EchoBuildUtils.Place(narrative.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 120f), new Vector2(1200f, 300f));

            var resultsGO = new GameObject("Results");
            resultsGO.transform.SetParent(vicPanel.transform, false);
            var resultsRT = resultsGO.AddComponent<RectTransform>();
            EchoBuildUtils.Place(resultsRT, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760f, 500f));
            var resultsCG = resultsGO.AddComponent<CanvasGroup>();

            var lvlName = EchoBuildUtils.CreateTitleText("LevelName", resultsGO.transform, "", 46f, Cyan, TextAlignmentOptions.Center);
            EchoBuildUtils.Place(lvlName.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 175f), new Vector2(720f, 70f));
            EchoBuildUtils.ApplyOutline(lvlName, new Color(0f, 0.05f, 0.12f, 0.95f), 0.14f);
            var timeT = EchoBuildUtils.CreateText("TimeText", resultsGO.transform, "", 34f, Color.white, TextAlignmentOptions.Center);
            EchoBuildUtils.Place(timeT.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 105f), new Vector2(720f, 50f));
            var fragT = EchoBuildUtils.CreateText("FragText", resultsGO.transform, "", 34f, Color.white, TextAlignmentOptions.Center);
            EchoBuildUtils.Place(fragT.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 55f), new Vector2(720f, 50f));
            var finalT = EchoBuildUtils.CreateText("FinalText", resultsGO.transform, "", 30f, Gold, TextAlignmentOptions.Center);
            EchoBuildUtils.Place(finalT.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -10f), new Vector2(700f, 110f));

            Button nextBtn = MakeMenuButton("Next", resultsGO.transform, "Next Level", buttonBg, uiSource, hover, click, -135f);
            Button vmenuBtn = MakeMenuButton("Menu", resultsGO.transform, "Main Menu", buttonBg, uiSource, hover, click, -220f);

            var victory = canvas.gameObject.AddComponent<VictoryScreen>();
            victory.panelRoot = vicPanel;
            victory.narrativeText = narrative;
            victory.resultsGroup = resultsCG;
            victory.resultsRect = resultsRT;
            victory.levelNameText = lvlName;
            victory.timeText = timeT;
            victory.fragmentsText = fragT;
            victory.finalMessageText = finalT;
            victory.nextButton = nextBtn;
            victory.menuButton = vmenuBtn;
            vicPanel.SetActive(false);
            refs.victory = victory;

            return refs;
        }

        static GameObject NewPanel(string name, Transform parent, Color dimColor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            EchoBuildUtils.FullStretch(rt);
            var dim = EchoBuildUtils.CreateImage("Dim", go.transform, null, dimColor);
            EchoBuildUtils.FullStretch(dim.rectTransform);
            dim.raycastTarget = true;
            return go;
        }

        static Button MakeMenuButton(string name, Transform parent, string label, Sprite bg,
            AudioSource src, AudioClip hover, AudioClip click, float y)
        {
            Button b = EchoBuildUtils.CreateButton(name, parent, label, bg, src, hover, click);
            EchoBuildUtils.Place((RectTransform)b.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(360f, 74f));
            return b;
        }
    }
}
