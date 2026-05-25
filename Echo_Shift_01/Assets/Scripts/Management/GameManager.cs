using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EchoShift
{
    /// <summary>
    /// Central flow + UI controller: recording vignette toggle, active-clone tracking
    /// (for camera pull-back), and the level-complete sequence (white flash → memory
    /// line → "Level Complete" → pause).
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public RecordingVignette vignette;
        public Image flashImage;
        public TMP_Text endText;

        public string line1 = "I remember... this was my home.";
        public string line2 = "Level Complete";
        public float flashTime = 0.45f;
        public float textFadeTime = 1.2f;
        public float betweenDelay = 1.7f;

        int activeClones;
        bool completed;

        public bool IsCloneActive => activeClones > 0;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            Time.timeScale = 1f;
            if (flashImage != null) SetImageAlpha(flashImage, 0f);
            if (endText != null) SetTextAlpha(endText, 0f);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void RegisterClone() => activeClones++;
        public void UnregisterClone() => activeClones = Mathf.Max(0, activeClones - 1);
        public void SetRecording(bool on) { if (vignette != null) vignette.SetRecording(on); }

        public void CompleteLevel()
        {
            if (completed) return;
            completed = true;
            StartCoroutine(CompleteRoutine());
        }

        IEnumerator CompleteRoutine()
        {
            if (flashImage != null)
            {
                SetImageAlpha(flashImage, 0.92f);
                float t = 0f;
                while (t < flashTime)
                {
                    t += Time.unscaledDeltaTime;
                    SetImageAlpha(flashImage, Mathf.Lerp(0.92f, 0f, t / flashTime));
                    yield return null;
                }
                SetImageAlpha(flashImage, 0f);
            }

            if (endText != null)
            {
                endText.text = line1;
                yield return FadeText(endText, 0f, 1f, textFadeTime);
                yield return WaitUnscaled(betweenDelay);
                endText.text = line1 + "\n\n" + line2;
            }

            yield return WaitUnscaled(0.4f);
            Time.timeScale = 0f;
        }

        static IEnumerator FadeText(TMP_Text txt, float from, float to, float dur)
        {
            float t = 0f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                SetTextAlpha(txt, Mathf.Lerp(from, to, t / dur));
                yield return null;
            }
            SetTextAlpha(txt, to);
        }

        static IEnumerator WaitUnscaled(float seconds)
        {
            float t = 0f;
            while (t < seconds) { t += Time.unscaledDeltaTime; yield return null; }
        }

        static void SetImageAlpha(Image img, float a)
        {
            Color c = img.color; c.a = a; img.color = c;
        }

        static void SetTextAlpha(TMP_Text txt, float a)
        {
            Color c = txt.color; c.a = a; txt.color = c;
        }
    }
}
