using UnityEngine;

namespace EchoShift
{
    /// <summary>Pulses scale and/or alpha to make glows breathe (collectible, record dot, prompt).</summary>
    public class PulseGlow : MonoBehaviour
    {
        public SpriteRenderer target;
        public bool pulseScale = true;
        public bool pulseAlpha = true;
        public float minScale = 0.9f;
        public float maxScale = 1.12f;
        public float minAlpha = 0.55f;
        public float maxAlpha = 1f;
        public float speed = 3f;

        Vector3 baseScale;
        Color baseColor;
        float phase;

        void Awake()
        {
            if (target == null) target = GetComponent<SpriteRenderer>();
            baseScale = transform.localScale;
            if (target != null) baseColor = target.color;
            phase = Random.value * 6.2831853f;
        }

        void Update()
        {
            float n = (Mathf.Sin(Time.time * speed + phase) + 1f) * 0.5f;
            if (pulseScale) transform.localScale = baseScale * Mathf.Lerp(minScale, maxScale, n);
            if (pulseAlpha && target != null)
            {
                Color c = baseColor;
                c.a = Mathf.Lerp(minAlpha, maxAlpha, n);
                target.color = c;
            }
        }
    }
}
