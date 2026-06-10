using EchoShift;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EchoShift.EditorTools
{
    public static class EchoNarrative
    {
        public static NarrativeTerminal BuildTerminal()
        {
            EchoBuildUtils.EnsureEventSystem();
            Canvas canvas = EchoBuildUtils.CreateOverlayCanvas("NarrativeCanvas", 70);
            Transform root = canvas.transform;

            var terminalGO = new GameObject("Terminal");
            terminalGO.transform.SetParent(root, false);
            var trt = terminalGO.AddComponent<RectTransform>();
            EchoBuildUtils.Place(trt, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 130f), new Vector2(1360f, 310f));
            var group = terminalGO.AddComponent<CanvasGroup>();
            group.alpha = 0f;

            var bg = EchoBuildUtils.CreateImage("Bg", terminalGO.transform, null, new Color(0f, 0f, 0f, 0.88f));
            EchoBuildUtils.FullStretch(bg.rectTransform);
            bg.raycastTarget = false;

            var accent = EchoBuildUtils.CreateImage("Accent", terminalGO.transform, EchoBuildUtils.LoadSprite("gui_meter"),
                new Color(1f, 1f, 1f, 0.7f), true);
            EchoBuildUtils.Place(accent.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(34f, -28f), new Vector2(230f, 32f));
            accent.raycastTarget = false;

            var frame = EchoBuildUtils.CreateImage("Frame", terminalGO.transform, EchoBuildUtils.LoadSprite("frame"), Color.white, true);
            EchoBuildUtils.FullStretch(frame.rectTransform);
            frame.raycastTarget = false;

            var title = EchoBuildUtils.CreateTitleText("Title", terminalGO.transform, "", 34f, Color.white, TextAlignmentOptions.Left);
            EchoBuildUtils.Place(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(42f, -38f), new Vector2(520f, 48f));
            title.fontStyle = FontStyles.Bold;

            var speaker = EchoBuildUtils.CreateText("Speaker", terminalGO.transform, "", 24f,
                new Color(1f, 1f, 1f, 0.72f), TextAlignmentOptions.Right);
            EchoBuildUtils.Place(speaker.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-42f, -42f), new Vector2(360f, 42f));

            var memGO = new GameObject("Memory");
            memGO.transform.SetParent(terminalGO.transform, false);
            var memImg = memGO.AddComponent<Image>();
            memImg.raycastTarget = false;
            memImg.preserveAspect = true;
            EchoBuildUtils.Place(memImg.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-44f, -12f), new Vector2(180f, 180f));
            var memGroup = memGO.AddComponent<CanvasGroup>();
            memGroup.alpha = 0f;
            memImg.enabled = false;

            var body = EchoBuildUtils.CreateTitleText("Body", terminalGO.transform, "", 38f, Color.white,
                TextAlignmentOptions.Left);
            EchoBuildUtils.Place(body.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(54f, -34f), new Vector2(1040f, 178f));
            body.enableWordWrapping = true;
            body.characterSpacing = 1.5f;

            var nt = canvas.gameObject.AddComponent<NarrativeTerminal>();
            nt.group = group;
            nt.titleText = title;
            nt.speakerText = speaker;
            nt.bodyText = body;
            nt.memoryImage = memImg;
            nt.memoryGroup = memGroup;
            nt.accentImage = accent;
            return nt;
        }
    }
}
