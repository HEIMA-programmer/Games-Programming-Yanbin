using UnityEngine;
using UnityEngine.UI;

namespace EchoShift
{
    /// <summary>Animates a full-screen red border Image while the player is recording.</summary>
    public class RecordingVignette : MonoBehaviour
    {
        public Image image;
        public float fadeSpeed = 6f;
        public float baseAlpha = 0.34f;
        public float pulseAmp = 0.12f;
        public float pulseSpeed = 5f;

        bool recording;
        float current;

        void Awake()
        {
            if (image == null) image = GetComponent<Image>();
            SetAlpha(0f);
        }

        public void SetRecording(bool on) => recording = on;

        void Update()
        {
            float target = recording ? Mathf.Max(0f, baseAlpha + Mathf.Sin(Time.time * pulseSpeed) * pulseAmp) : 0f;
            current = Mathf.MoveTowards(current, target, fadeSpeed * Time.deltaTime);
            SetAlpha(current);
        }

        void SetAlpha(float a)
        {
            if (image == null) return;
            Color c = image.color;
            c.a = a;
            image.color = c;
        }
    }
}
