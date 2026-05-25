using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EchoShift
{
    /// <summary>
    /// Persistent fade-to-black scene transition (DontDestroyOnLoad). Builds its own
    /// top-most overlay canvas in code. Use FadeToScene to switch scenes with a
    /// 0.5s out / load / 0.5s in transition. Uses unscaled time so it works while paused.
    /// </summary>
    public class SceneFader : MonoBehaviour
    {
        public static SceneFader Instance { get; private set; }
        public float fadeTime = 0.5f;

        Image image;
        bool isFading;

        public bool IsFading => isFading;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildOverlay();
            SetAlpha(0f);
        }

        void BuildOverlay()
        {
            var canvasGO = new GameObject("FaderCanvas");
            canvasGO.transform.SetParent(transform, false);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32760; // above every gameplay/UI canvas
            canvasGO.AddComponent<GraphicRaycaster>();

            var imgGO = new GameObject("Black");
            imgGO.transform.SetParent(canvasGO.transform, false);
            image = imgGO.AddComponent<Image>();
            image.color = Color.black;
            RectTransform rt = image.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        public void FadeToScene(string sceneName)
        {
            if (isFading) return;
            StartCoroutine(LoadRoutine(sceneName));
        }

        public void FadeInNow()
        {
            StopAllCoroutines();
            SetAlpha(1f);
            StartCoroutine(FadeRoutine(1f, 0f, fadeTime));
        }

        IEnumerator LoadRoutine(string sceneName)
        {
            isFading = true;
            yield return FadeRoutine(image.color.a, 1f, fadeTime);

            Time.timeScale = 1f; // make sure gameplay resumes after a paused transition
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            while (op != null && !op.isDone) yield return null;

            yield return FadeRoutine(1f, 0f, fadeTime);
            isFading = false;
        }

        IEnumerator FadeRoutine(float from, float to, float dur)
        {
            float t = 0f;
            SetAlpha(from);
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                SetAlpha(Mathf.Lerp(from, to, dur <= 0f ? 1f : t / dur));
                yield return null;
            }
            SetAlpha(to);
        }

        void SetAlpha(float alpha)
        {
            if (image == null) return;
            image.raycastTarget = alpha > 0.001f;
            Color c = image.color;
            c.a = alpha;
            image.color = c;
        }
    }
}
