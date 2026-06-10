using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EchoShift
{
    [System.Serializable]
    public class CutsceneSlide
    {
        public Sprite image;
        // Speaker styling is inferred per line: lines starting with '"' are the child's
        // PLAYBACK recordings (cyan, the echo colour); everything else is SYSTEM (white).
        [TextArea] public string[] lines;
    }

    /// <summary>
    /// Full-screen story interlude: a sequence of slides, each fading in from black,
    /// typing its caption with a terminal blip, holding, and fading back to black.
    /// Fade-to-black (not crossfade) on purpose — crossfading 1-bit art produces grey
    /// in-between frames that break the two-colour look, and black is the same
    /// transition language SceneFader/AutoDoorExit already speak.
    /// SPACE/click: first press reveals the full caption, second skips to the next
    /// slide. ESC skips the whole cutscene. Uses unscaled time throughout.
    /// </summary>
    public class CutscenePlayer : MonoBehaviour
    {
        public CutsceneSlide[] slides;
        public string nextScene = "MainMenu";
        [Tooltip("Set on the ending cutscene only — marks the game completed for the menu.")]
        public bool markCompleted = false;

        [Header("Wiring")]
        public Image slideImage;
        public CanvasGroup slideGroup;     // image + caption fade together
        public TMP_Text caption;
        [Tooltip("Black band behind the caption so white type stays readable over bright art. Shown only while there is text.")]
        public Image captionPlate;
        public AudioSource audioSource;
        public AudioClip typeClip;

        [Header("Pacing")]
        public float fadeInTime = 0.6f;
        public float fadeOutTime = 0.4f;
        public float blackBeat = 0.15f;    // breath of pure black between slides
        public float holdAfterText = 1.6f;
        public float charsPerSecond = 28f;
        public float linePause = 0.35f;

        const string CyanHex = "#5CF2F2";

        bool advanceQueued;
        bool skipAllQueued;
        int blipToggle;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
                Advance();
            if (Input.GetKeyDown(KeyCode.Escape))
                SkipAll();
            if (captionPlate != null)
                captionPlate.enabled = !string.IsNullOrEmpty(caption.text);
        }

        public void Advance() => advanceQueued = true;
        public void SkipAll() { skipAllQueued = true; advanceQueued = true; }

        IEnumerator Start()
        {
            slideGroup.alpha = 0f;
            caption.text = "";

            foreach (var slide in slides)
            {
                if (skipAllQueued) break;
                advanceQueued = false;

                slideImage.sprite = slide.image;
                caption.text = "";
                yield return Fade(0f, 1f, fadeInTime);

                // typewriter, line by line; one press hard-cuts to the full caption
                string shown = "";
                foreach (var line in slide.lines)
                {
                    bool playback = line.StartsWith("\"");
                    string open = playback ? "<color=" + CyanHex + ">" : "";
                    string close = playback ? "</color>" : "";
                    if (shown.Length > 0) shown += "\n";

                    int i = 0;
                    while (i < line.Length && !advanceQueued && !skipAllQueued)
                    {
                        i++;
                        caption.text = shown + open + line.Substring(0, i) + close;
                        Blip(line[i - 1]);
                        yield return new WaitForSecondsRealtime(1f / Mathf.Max(1f, charsPerSecond));
                    }
                    caption.text = shown + open + line + close;
                    shown += open + line + close;
                    if (!advanceQueued && !skipAllQueued)
                        yield return new WaitForSecondsRealtime(linePause);
                }
                advanceQueued = false;   // the reveal press is spent; next press = next slide

                yield return WaitOrAdvance(holdAfterText);
                yield return Fade(1f, 0f, fadeOutTime);
                yield return new WaitForSecondsRealtime(blackBeat);
            }

            if (markCompleted) GameProgress.MarkCompleted();
            if (SceneFader.Instance != null) SceneFader.Instance.FadeToScene(nextScene);
            else SceneManager.LoadScene(nextScene);
        }

        IEnumerator WaitOrAdvance(float duration)
        {
            float t = 0f;
            while (t < duration && !advanceQueued && !skipAllQueued)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }
            advanceQueued = false;
        }

        IEnumerator Fade(float from, float to, float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                slideGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
                yield return null;
            }
            slideGroup.alpha = to;
        }

        void Blip(char c)
        {
            if (audioSource == null || typeClip == null) return;
            if (char.IsWhiteSpace(c)) return;
            if ((blipToggle++ & 1) == 1) return;          // every 2nd glyph
            audioSource.pitch = Random.Range(0.92f, 1.12f);
            audioSource.PlayOneShot(typeClip, 0.55f);
        }
    }
}
