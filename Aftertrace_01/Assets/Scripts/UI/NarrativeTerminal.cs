using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EchoShift
{
    public enum NarrativeBlockingMode
    {
        Passive,
        Blocking,
        ForceBlocking
    }

    /// <summary>
    /// Shared diegetic terminal for story beats, contextual hints, and memory fragments.
    /// Beats are queued so nearby triggers never erase one another.
    /// </summary>
    public class NarrativeTerminal : MonoBehaviour
    {
        public static NarrativeTerminal Instance { get; private set; }

        [Header("Wired by scene/UI builders")]
        public CanvasGroup group;
        public TMP_Text titleText;
        public TMP_Text speakerText;
        public TMP_Text bodyText;
        public Image memoryImage;
        public CanvasGroup memoryGroup;
        public Image accentImage;
        public Image portraitImage;      // speaker head (classic bottom-left dialogue style)
        public Sprite defaultPortrait;   // shown when a beat doesn't specify its own

        [Header("Feel")]
        public float charDelay = 0.032f;
        public float holdPerLine = 1.15f;
        public float fadeTime = 0.28f;
        public float skipArmTime = 0.15f;

        readonly Queue<Beat> queue = new Queue<Beat>();
        bool busy;
        Coroutine runner;

        public bool IsBusy => busy;
        public int QueuedCount => queue.Count;

        void Awake()
        {
            Instance = this;
            if (group != null)
            {
                group.alpha = 0f;
                group.blocksRaycasts = false;
            }
            if (memoryGroup != null) memoryGroup.alpha = 0f;
            if (memoryImage != null) memoryImage.enabled = false;
            if (portraitImage != null) portraitImage.enabled = false;
            SetText(titleText, "");
            SetText(speakerText, "");
            SetText(bodyText, "");
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            // scene change mid-beat must not leave the world frozen
            StoryFreeze = false;
        }

        public void Play(string[] lines, bool blocking, Sprite memory = null, Action onComplete = null)
        {
            Enqueue(lines, blocking ? NarrativeBlockingMode.Blocking : NarrativeBlockingMode.Passive,
                "", "", memory, onComplete);
        }

        public void Play(string[] lines, NarrativeBlockingMode mode, string title = "",
            string speaker = "", Sprite memory = null, Action onComplete = null, Sprite portrait = null)
        {
            Enqueue(lines, mode, title, speaker, memory, onComplete, portrait);
        }

        public void Enqueue(string[] lines, NarrativeBlockingMode mode, string title = "",
            string speaker = "", Sprite memory = null, Action onComplete = null, Sprite portrait = null)
        {
            if (lines == null || lines.Length == 0)
            {
                onComplete?.Invoke();
                return;
            }

            queue.Enqueue(new Beat
            {
                lines = lines,
                mode = mode,
                title = title,
                speaker = speaker,
                memory = memory,
                portrait = portrait,
                onComplete = onComplete
            });

            if (runner == null) runner = StartCoroutine(ProcessQueue());
        }

        IEnumerator ProcessQueue()
        {
            while (queue.Count > 0)
            {
                Beat beat = queue.Dequeue();
                yield return Run(beat);
            }
            runner = null;
        }

        /// <summary>
        /// True while a BLOCKING beat is on screen. The player is frozen then, so the
        /// world's threats freeze with them — drones, mines, traps and echo playback all
        /// gate on this. Without it, a story beat next to a hazard is a death sentence
        /// (frozen player, live mine).
        /// </summary>
        public static bool StoryFreeze { get; private set; }

        IEnumerator Run(Beat beat)
        {
            busy = true;
            bool blocking = beat.mode != NarrativeBlockingMode.Passive;

            PlayerController player = blocking ? FindObjectOfType<PlayerController>() : null;
            bool prevControl = true;
            if (player != null)
            {
                prevControl = player.ControlEnabled;
                player.ControlEnabled = false;
            }
            if (blocking) StoryFreeze = true;

            SetText(titleText, beat.title);
            SetText(speakerText, beat.speaker);
            // clear the PREVIOUS beat's last line — it stays in the TMP after fade-out,
            // and fading the panel back in would flash it before TypeLine resets the text
            SetText(bodyText, "");

            if (portraitImage != null)
            {
                Sprite face = beat.portrait != null ? beat.portrait : defaultPortrait;
                portraitImage.sprite = face;
                portraitImage.enabled = face != null;
            }

            if (accentImage != null)
            {
                Color c = accentImage.color;
                c.a = blocking ? 1f : 0.55f;
                accentImage.color = c;
            }

            if (group != null)
            {
                group.blocksRaycasts = blocking;
                yield return Fade(group, 0f, 1f, fadeTime);
            }

            bool showImage = beat.memory != null && memoryImage != null;
            if (showImage)
            {
                memoryImage.sprite = beat.memory;
                memoryImage.enabled = true;
                if (memoryGroup != null) yield return Fade(memoryGroup, 0f, 1f, fadeTime);
            }

            foreach (string raw in beat.lines)
                yield return TypeLine(raw ?? "");

            if (showImage && memoryGroup != null) yield return Fade(memoryGroup, 1f, 0f, fadeTime);
            if (memoryImage != null) memoryImage.enabled = false;

            if (group != null)
            {
                yield return Fade(group, 1f, 0f, fadeTime);
                group.blocksRaycasts = false;
            }

            StoryFreeze = false;
            if (player != null) player.ControlEnabled = prevControl;
            busy = false;
            beat.onComplete?.Invoke();
        }

        IEnumerator TypeLine(string line)
        {
            float lineTime = 0f;
            int total = 0;

            if (bodyText != null)
            {
                bodyText.gameObject.SetActive(true);
                bodyText.text = line;
                bodyText.maxVisibleCharacters = 0;
                bodyText.ForceMeshUpdate();
                total = bodyText.textInfo.characterCount;
            }

            int shown = 0;
            float acc = 0f;
            while (bodyText != null && shown < total)
            {
                lineTime += Time.unscaledDeltaTime;
                if (lineTime > skipArmTime && SkipPressed())
                {
                    shown = total;
                    break;
                }

                acc += Time.unscaledDeltaTime;
                while (acc >= charDelay && shown < total)
                {
                    acc -= charDelay;
                    shown++;
                }
                bodyText.maxVisibleCharacters = shown;
                yield return null;
            }

            if (bodyText != null) bodyText.maxVisibleCharacters = total;

            float held = 0f;
            while (held < holdPerLine)
            {
                lineTime += Time.unscaledDeltaTime;
                held += Time.unscaledDeltaTime;
                if (lineTime > skipArmTime && SkipPressed()) break;
                yield return null;
            }
        }

        static bool SkipPressed() => Input.anyKeyDown || Input.GetMouseButtonDown(0);

        static void SetText(TMP_Text text, string value)
        {
            if (text == null) return;
            text.text = value ?? "";
            text.gameObject.SetActive(!string.IsNullOrEmpty(text.text));
        }

        IEnumerator Fade(CanvasGroup g, float a, float b, float dur)
        {
            float e = 0f;
            while (e < dur)
            {
                e += Time.unscaledDeltaTime;
                g.alpha = Mathf.Lerp(a, b, e / dur);
                yield return null;
            }
            g.alpha = b;
        }

        struct Beat
        {
            public string[] lines;
            public NarrativeBlockingMode mode;
            public string title;
            public string speaker;
            public Sprite memory;
            public Sprite portrait;
            public Action onComplete;
        }
    }
}
