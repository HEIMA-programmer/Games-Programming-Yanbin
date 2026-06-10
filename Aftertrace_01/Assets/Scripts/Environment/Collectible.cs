using UnityEngine;

namespace EchoShift
{
    /// <summary>
    /// Memory fragment. Optional fragments count immediately; ending fragments can play
    /// a memory beat before completing the level.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Collectible : MonoBehaviour
    {
        public AudioSource audioSource;
        public AudioClip chimeClip;
        public ParticleSystem burstParticles;
        [Tooltip("True = the level-ending fragment. False = optional fragment.")]
        public bool endsLevel = true;

        [Header("Memory beat")]
        public string memoryTitle = "MEMORY FRAGMENT";
        public string memorySpeaker = "TRACE";
        [TextArea] public string[] storyLines;
        public Sprite memoryImage;
        public bool completeAfterStory = true;
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

            bool hasStory = storyLines != null && storyLines.Length > 0;
            if (hasStory && NarrativeTerminal.Instance != null)
            {
                NarrativeTerminal.Instance.Play(storyLines, storyMode, memoryTitle, memorySpeaker, memoryImage,
                    () => Finish(gm));
            }
            else
            {
                Finish(gm);
            }
        }

        void Finish(GameManager gm)
        {
            if (!endsLevel) return;
            if (!completeAfterStory && storyLines != null && storyLines.Length > 0) return;

            if (gm != null)
            {
                gm.Flash();
                gm.CompleteLevel();
            }
        }
    }
}
