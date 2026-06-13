using UnityEngine;

namespace EchoShift
{
    /// <summary>
    /// Memory fragment pickup: counts toward the level's recovered traces and can play
    /// a memory beat. Fragments are optional — levels end at the exit door, not here.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Collectible : MonoBehaviour
    {
        public AudioSource audioSource;
        public AudioClip chimeClip;
        public ParticleSystem burstParticles;

        [Header("Memory beat")]
        public string memoryTitle = "MEMORY FRAGMENT";
        public string memorySpeaker = "TRACE";
        [TextArea] public string[] storyLines;
        public Sprite memoryImage;
        public NarrativeBlockingMode storyMode = NarrativeBlockingMode.Blocking;

        bool taken;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (taken) return;
            if (other.GetComponentInParent<PlayerController>() == null) return;

            taken = true;
            if (audioSource != null && chimeClip != null) audioSource.PlayOneShot(chimeClip);
            if (burstParticles != null) burstParticles.Play();

            foreach (SpriteRenderer r in GetComponentsInChildren<SpriteRenderer>())
                r.enabled = false;

            GameManager gm = GameManager.Instance;
            if (gm != null) gm.CollectFragment();

            if (storyLines != null && storyLines.Length > 0 && NarrativeTerminal.Instance != null)
                NarrativeTerminal.Instance.Play(storyLines, storyMode, memoryTitle, memorySpeaker, memoryImage);
        }
    }
}
