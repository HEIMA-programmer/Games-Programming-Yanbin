using UnityEngine;

namespace EchoShift
{
    /// <summary>
    /// The memory fragment at the level's end. When the player touches it, triggers the
    /// level-complete sequence. Only the player (not an echo clone) can collect it.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Collectible : MonoBehaviour
    {
        public AudioSource audioSource;
        public AudioClip chimeClip;
        public ParticleSystem burstParticles;
        [Tooltip("True = the level-ending fragment (flash + victory). False = an optional fragment that only counts.")]
        public bool endsLevel = true;

        bool taken;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (taken) return;
            if (other.GetComponentInParent<PlayerController>() == null) return;

            taken = true;
            if (audioSource != null && chimeClip != null) audioSource.PlayOneShot(chimeClip);
            if (burstParticles != null) burstParticles.Play();

            GameManager gm = GameManager.Instance;
            if (gm != null)
            {
                gm.CollectFragment();
                if (endsLevel) { gm.Flash(); gm.CompleteLevel(); }
            }

            foreach (SpriteRenderer r in GetComponentsInChildren<SpriteRenderer>())
                r.enabled = false;
        }
    }
}
