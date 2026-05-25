using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace EchoShift
{
    /// <summary>
    /// Flickering effect for dim lights and "broken screen" wall displays. Drives alpha
    /// of a SpriteRenderer / UI Graphic and/or the intensity of a Light2D, using Perlin
    /// noise with occasional sharp dips.
    /// </summary>
    public class Flicker : MonoBehaviour
    {
        public SpriteRenderer spriteTarget;
        public Graphic uiTarget;
        public Light2D lightTarget;
        public float min = 0.45f;
        public float max = 1f;
        public float speed = 9f;
        public float dipChance = 0.03f;

        float seed;
        float baseIntensity = 1f;

        void Awake()
        {
            seed = Random.value * 100f;
            if (lightTarget != null) baseIntensity = lightTarget.intensity;
        }

        void Update()
        {
            float n = Mathf.PerlinNoise(seed, Time.time * speed);
            float v = Mathf.Lerp(min, max, n);
            if (Random.value < dipChance) v *= 0.4f;

            if (spriteTarget != null) { Color c = spriteTarget.color; c.a = v; spriteTarget.color = c; }
            if (uiTarget != null) { Color c = uiTarget.color; c.a = v; uiTarget.color = c; }
            if (lightTarget != null) lightTarget.intensity = baseIntensity * v;
        }
    }
}
