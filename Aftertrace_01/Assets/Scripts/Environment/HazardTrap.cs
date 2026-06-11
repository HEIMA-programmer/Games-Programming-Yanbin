using System.Collections;
using UnityEngine;

namespace EchoShift
{
    /// <summary>
    /// Cyclic hazard for energy leaks, flame vents, and unstable memory arcs.
    /// Active hazards respawn the player and dissolve echoes.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class HazardTrap : MonoBehaviour
    {
        public SpriteRenderer visual;
        public Sprite idleSprite;   // shown while inactive (e.g. bare emitter plate)
        public Sprite[] frames;     // active-cycle animation (e.g. the arc itself)
        public ParticleSystem particles;
        public AudioSource audioSource;
        public AudioClip activateClip;

        [Header("Cycle")]
        public bool cycle = true;
        public bool startsActive = false;
        public float startDelay = 0f;
        public float activeTime = 1.2f;
        public float inactiveTime = 1.8f;
        public float warningTime = 0.35f;
        public float frameRate = 12f;

        [Header("Effect")]
        public bool respawnPlayer = true;
        public bool dissolveEcho = true;

        Collider2D hitbox;
        bool active;
        int frameIndex;
        float frameTimer;

        void Awake()
        {
            hitbox = GetComponent<Collider2D>();
            hitbox.isTrigger = true;
            SetActiveState(startsActive);
        }

        void OnEnable()
        {
            if (cycle) StartCoroutine(CycleRoutine());
        }

        void OnDisable()
        {
            StopAllCoroutines();
        }

        void Update()
        {
            if (!active || visual == null || frames == null || frames.Length == 0) return;

            frameTimer += Time.deltaTime;
            float step = 1f / Mathf.Max(1f, frameRate);
            if (frameTimer >= step)
            {
                frameTimer -= step;
                frameIndex = (frameIndex + 1) % frames.Length;
                visual.sprite = frames[frameIndex];
            }
        }

        IEnumerator CycleRoutine()
        {
            if (startDelay > 0f) yield return new WaitForSeconds(startDelay);

            while (enabled)
            {
                if (!startsActive)
                {
                    SetWarning(true);
                    yield return new WaitForSeconds(warningTime);
                }

                SetWarning(false);
                SetActiveState(true);
                yield return new WaitForSeconds(activeTime);

                SetActiveState(false);
                yield return new WaitForSeconds(inactiveTime);
            }
        }

        void SetWarning(bool on)
        {
            if (visual == null || active) return;
            Color c = visual.color;
            c.a = on ? 0.7f : 0.25f;
            visual.color = c;
        }

        void SetActiveState(bool on)
        {
            active = on;
            if (hitbox != null) hitbox.enabled = on;

            if (visual != null)
            {
                Color c = visual.color;
                c.a = on ? 1f : (idleSprite != null ? 1f : 0.25f);
                visual.color = c;
                if (on && frames != null && frames.Length > 0)
                {
                    frameIndex = 0;
                    visual.sprite = frames[0];
                }
                else if (!on && idleSprite != null)
                {
                    visual.sprite = idleSprite;
                }
            }

            if (particles != null)
            {
                if (on) particles.Play();
                else particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            if (on && audioSource != null && activateClip != null)
                audioSource.PlayOneShot(activateClip);
        }

        void OnTriggerEnter2D(Collider2D other) => Affect(other);
        void OnTriggerStay2D(Collider2D other) => Affect(other);

        void Affect(Collider2D other)
        {
            if (!active) return;
            // flames keep animating during a story beat but can't hurt a frozen player
            if (NarrativeTerminal.StoryFreeze) return;

            if (dissolveEcho)
            {
                EchoClone clone = other.GetComponentInParent<EchoClone>();
                if (clone != null)
                {
                    clone.BeginDissolve();
                    return;
                }
            }

            if (respawnPlayer && other.GetComponentInParent<PlayerController>() != null)
            {
                if (GameManager.Instance != null) GameManager.Instance.RespawnPlayer();
            }
        }
    }
}
