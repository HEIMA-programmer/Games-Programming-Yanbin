using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EchoShift
{
    /// <summary>
    /// Level 0 opening: screen starts black, three lines fade in one at a time, then the
    /// black overlay fades out to reveal the (initially uncontrollable) player; after a
    /// beat, control is granted. No HUD / echo / enemies — pure mood + movement.
    /// </summary>
    public class Level0Intro : MonoBehaviour
    {
        public PlayerController player;
        public Image blackImage;
        public TMP_Text lineText;
        public string[] lines =
        {
            "System rebooting...",
            "Memory module... corrupted.",
            "Motor functions... online."
        };
        public float lineFadeIn = 1.2f;
        public float lineHold = 1.4f;
        public float lineFadeOut = 0.8f;
        public float revealFade = 1.5f;
        public float standBeat = 0.8f;

        void Start()
        {
            if (player != null) player.ControlEnabled = false;
            if (blackImage != null) SetImgAlpha(blackImage, 1f);
            if (lineText != null) SetTextAlpha(lineText, 0f);
            StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            if (lineText != null)
            {
                foreach (string line in lines)
                {
                    lineText.text = line;
                    yield return FadeText(lineText, 0f, 1f, lineFadeIn);
                    yield return Wait(lineHold);
                    yield return FadeText(lineText, 1f, 0f, lineFadeOut);
                }
            }
            if (blackImage != null) yield return FadeImg(blackImage, 1f, 0f, revealFade);
            yield return Wait(standBeat);
            if (player != null) player.ControlEnabled = true;
        }

        IEnumerator FadeText(TMP_Text t, float a, float b, float dur)
        {
            float e = 0f;
            while (e < dur) { e += Time.deltaTime; SetTextAlpha(t, Mathf.Lerp(a, b, e / dur)); yield return null; }
            SetTextAlpha(t, b);
        }

        IEnumerator FadeImg(Image img, float a, float b, float dur)
        {
            float e = 0f;
            while (e < dur) { e += Time.deltaTime; SetImgAlpha(img, Mathf.Lerp(a, b, e / dur)); yield return null; }
            SetImgAlpha(img, b);
        }

        static IEnumerator Wait(float s) { float e = 0f; while (e < s) { e += Time.deltaTime; yield return null; } }
        static void SetTextAlpha(TMP_Text t, float a) { Color c = t.color; c.a = a; t.color = c; }
        static void SetImgAlpha(Image img, float a) { Color c = img.color; c.a = a; img.color = c; }
    }
}
