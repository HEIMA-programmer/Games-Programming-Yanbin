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

        // (FadeInNow was removed: dead code whose StopAllCoroutines could kill a
        //  mid-flight LoadRoutine and leave isFading stuck true = every later
        //  FadeToScene silently no-ops. Nothing ever called it.)

        IEnumerator LoadRoutine(string sceneName)
        {
            isFading = true;
            // the old music starts drifting away WITH the black — if the next scene
            // wants a different track, it should already be receding when we get there
            if (AudioManager.Instance != null) AudioManager.Instance.DriftOut();
            yield return FadeRoutine(image.color.a, 1f, fadeTime);

            Time.timeScale = 1f; // make sure gameplay resumes after a paused transition
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            while (op != null && !op.isDone) yield return null;

            // Hold full black for two rendered frames so first-frame initialization
            // (camera snap, player ground-settle, pixel-perfect RT creation, Light2D
            // warm-up) all happens under cover instead of flashing on screen.
            SetAlpha(1f);
            yield return null;
            yield return null;

            yield return FadeRoutine(1f, 0f, fadeTime);
            isFading = false;
        }

        IEnumerator FadeRoutine(float from, float to, float dur)
        {
            float t = 0f;
            SetAlpha(from);
            while (t < dur)
            {
                // Clamp the step: the frame right after a scene load carries the whole
                // load hitch in its delta (hundreds of ms) and would swallow the entire
                // fade in one jump — the "flash" the fade exists to prevent.
                t += Mathf.Min(Time.unscaledDeltaTime, 1f / 30f);
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
