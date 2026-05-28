using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EchoShift
{
    /// <summary>
    /// Victory sequence: narrative line fades in, holds ~3s, fades out, then a results
    /// panel slides in (level name / time / fragments) with Next Level + Main Menu.
    /// </summary>
    public class VictoryScreen : MonoBehaviour
    {
        public GameObject panelRoot;
        public TMP_Text narrativeText;
        public CanvasGroup resultsGroup;
        public RectTransform resultsRect;
        public TMP_Text levelNameText;
        public TMP_Text timeText;
        public TMP_Text fragmentsText;
        public TMP_Text finalMessageText;
        public Button nextButton;
        public Button menuButton;
        public float narrativeHold = 3f;

        string nextScene = "MainMenu";
        string pendingNarrative = "";

        void Awake()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            if (nextButton != null) nextButton.onClick.AddListener(Next);
            if (menuButton != null) menuButton.onClick.AddListener(ToMenu);
        }

        public void Show(string levelName, float timeTaken, int collected, int total,
            string narrative, string next, string finalMessage,
            int totalCollected, int maxFragments, float totalTime, bool isFinal)
        {
            nextScene = string.IsNullOrEmpty(next) ? "MainMenu" : next;
            pendingNarrative = narrative ?? "";
            if (panelRoot != null) panelRoot.SetActive(true);
            if (levelNameText != null) levelNameText.text = levelName;
            if (timeText != null)
                timeText.text = isFinal ? "Total Time   " + FormatTime(totalTime) : "Time   " + FormatTime(timeTaken);
            if (fragmentsText != null)
                fragmentsText.text = "Fragments   " + collected + " / " + total + "        Total   " + totalCollected + " / " + maxFragments;
            if (finalMessageText != null)
            {
                finalMessageText.text = finalMessage ?? "";
                finalMessageText.gameObject.SetActive(!string.IsNullOrEmpty(finalMessage));
            }
            if (nextButton != null) nextButton.gameObject.SetActive(!isFinal);
            StartCoroutine(Sequence());
        }

        IEnumerator Sequence()
        {
            if (resultsGroup != null) { resultsGroup.alpha = 0f; resultsGroup.gameObject.SetActive(false); }

            if (narrativeText != null)
            {
                string[] lines = pendingNarrative.Split('\n');
                foreach (string line in lines)
                {
                    narrativeText.text = line;
                    SetTextAlpha(narrativeText, 0f);
                    yield return Fade(narrativeText, 0f, 1f, 0.9f);
                    yield return WaitUnscaled(1.4f);
                }
                yield return Fade(narrativeText, 1f, 0f, 0.7f);
            }

            if (resultsGroup != null)
            {
                resultsGroup.gameObject.SetActive(true);
                float t = 0f, dur = 0.6f;
                Vector2 from = new Vector2(0f, -40f), to = Vector2.zero;
                while (t < dur)
                {
                    t += Time.unscaledDeltaTime;
                    float n = t / dur;
                    resultsGroup.alpha = n;
                    if (resultsRect != null) resultsRect.anchoredPosition = Vector2.Lerp(from, to, Mathf.SmoothStep(0f, 1f, n));
                    yield return null;
                }
                resultsGroup.alpha = 1f;
                if (resultsRect != null) resultsRect.anchoredPosition = to;
            }
        }

        void Next()
        {
            if (SceneFader.Instance != null) SceneFader.Instance.FadeToScene(nextScene);
            else SceneManager.LoadScene(nextScene);
        }

        void ToMenu()
        {
            if (SceneFader.Instance != null) SceneFader.Instance.FadeToScene("MainMenu");
            else SceneManager.LoadScene("MainMenu");
        }

        static string FormatTime(float s)
        {
            int m = (int)(s / 60f);
            int sec = (int)(s % 60f);
            return string.Format("{0:0}:{1:00}", m, sec);
        }

        static IEnumerator Fade(TMP_Text t, float a, float b, float dur)
        {
            float e = 0f;
            while (e < dur)
            {
                e += Time.unscaledDeltaTime;
                SetTextAlpha(t, Mathf.Lerp(a, b, e / dur));
                yield return null;
            }
            SetTextAlpha(t, b);
        }

        static void SetTextAlpha(TMP_Text t, float a)
        {
            Color c = t.color; c.a = a; t.color = c;
        }

        static IEnumerator WaitUnscaled(float seconds)
        {
            float t = 0f;
            while (t < seconds) { t += Time.unscaledDeltaTime; yield return null; }
        }
    }
}
