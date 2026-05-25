using UnityEngine;

namespace EchoShift
{
    /// <summary>
    /// Trauma-based screen shake using Perlin noise. Exposes a positional Offset that
    /// CameraFollow adds. Auto-subscribes to the player's landing to shake on hard falls.
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        public float traumaDecay = 1.7f;
        public float maxOffset = 0.32f;
        public float frequency = 26f;
        public float hardLandSpeed = 12f;
        public float maxLandSpeed = 26f;

        public Vector3 Offset { get; private set; }

        float trauma;
        float seedX;
        float seedY;
        PlayerController player;

        void Awake()
        {
            seedX = Random.value * 100f;
            seedY = Random.value * 100f;
        }

        void Start()
        {
            player = FindObjectOfType<PlayerController>();
            if (player != null) player.Landed += OnLanded;
        }

        void OnDestroy()
        {
            if (player != null) player.Landed -= OnLanded;
        }

        void OnLanded(float impact)
        {
            if (impact < hardLandSpeed) return;
            Shake(Mathf.InverseLerp(hardLandSpeed, maxLandSpeed, impact) * 0.7f);
        }

        public void Shake(float amount)
        {
            trauma = Mathf.Clamp01(trauma + amount);
        }

        void Update()
        {
            if (trauma <= 0f) { Offset = Vector3.zero; return; }

            float s = trauma * trauma;
            float t = Time.unscaledTime * frequency;
            float x = (Mathf.PerlinNoise(seedX, t) * 2f - 1f);
            float y = (Mathf.PerlinNoise(seedY, t) * 2f - 1f);
            Offset = new Vector3(x, y, 0f) * maxOffset * s;

            trauma = Mathf.Max(0f, trauma - traumaDecay * Time.unscaledDeltaTime);
        }
    }
}
