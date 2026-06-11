using UnityEngine;

namespace EchoShift
{
    /// <summary>
    /// Proximity mine (Trap1): arms itself when a player or echo clone gets close,
    /// burns a short visible fuse, then detonates — the player respawns, an echo
    /// dissolves. Re-arms after a cooldown so the corridor stays dangerous on
    /// re-traversal. The intended play: bait it and back off before the blast, or
    /// spend an echo as a decoy. Crates never trigger it.
    /// </summary>
    public class ProximityMine : MonoBehaviour
    {
        public SpriteRenderer visual;
        public Sprite[] idleFrames;      // armed loop (slow blink)
        public Sprite[] explodeFrames;   // one-shot burst
        public float idleFps = 3f;
        public float fuseFps = 12f;      // idle frames flashed fast while the fuse burns
        public float explodeFps = 14f;

        [Header("Ranges / timing")]
        public float armRadius = 1.3f;
        public float blastRadius = 1.1f;
        public float fuseTime = 0.8f;
        public float cooldownTime = 3f;

        [Header("Audio")]
        public AudioSource audioSource;
        public AudioClip armClip;
        public AudioClip explodeClip;

        enum State { Idle, Fuse, Exploding, Cooldown }
        State state;
        float timer;
        int frame;
        float frameTimer;

        void Awake()
        {
            if (visual == null) visual = GetComponent<SpriteRenderer>();
            SetState(State.Idle);
        }

        void Update()
        {
            if (visual == null) return;
            float fps; Sprite[] frames; bool loop;
            switch (state)
            {
                case State.Fuse:      frames = idleFrames;    fps = fuseFps;    loop = true;  break;
                case State.Exploding: frames = explodeFrames; fps = explodeFps; loop = false; break;
                case State.Cooldown:  return;   // hidden
                default:              frames = idleFrames;    fps = idleFps;    loop = true;  break;
            }
            if (frames == null || frames.Length == 0) return;

            frameTimer += Time.deltaTime;
            float step = 1f / Mathf.Max(1f, fps);
            while (frameTimer >= step)
            {
                frameTimer -= step;
                if (loop) frame = (frame + 1) % frames.Length;
                else frame = Mathf.Min(frame + 1, frames.Length - 1);
                visual.sprite = frames[frame];
            }
        }

        void FixedUpdate()
        {
            // story beats freeze the player — the fuse must not keep burning at them
            if (NarrativeTerminal.StoryFreeze) return;
            float dt = Time.fixedDeltaTime;
            switch (state)
            {
                case State.Idle:
                    if (TargetInRange()) SetState(State.Fuse);
                    break;
                case State.Fuse:
                    timer -= dt;
                    if (timer <= 0f) Detonate();
                    break;
                case State.Exploding:
                    timer -= dt;
                    if (timer <= 0f) SetState(State.Cooldown);
                    break;
                case State.Cooldown:
                    timer -= dt;
                    if (timer <= 0f) SetState(State.Idle);
                    break;
            }
        }

        bool TargetInRange()
        {
            foreach (var h in Physics2D.OverlapCircleAll(transform.position, armRadius))
            {
                if (h.GetComponentInParent<PlayerController>() != null) return true;
                if (h.GetComponentInParent<EchoClone>() != null) return true;
            }
            return false;
        }

        void Detonate()
        {
            SetState(State.Exploding);
            if (audioSource != null && explodeClip != null) audioSource.PlayOneShot(explodeClip);

            foreach (var h in Physics2D.OverlapCircleAll(transform.position, blastRadius))
            {
                EchoClone clone = h.GetComponentInParent<EchoClone>();
                if (clone != null) { clone.BeginDissolve(); continue; }
                if (h.GetComponentInParent<PlayerController>() != null && GameManager.Instance != null)
                    GameManager.Instance.RespawnPlayer();
            }
        }

        void SetState(State s)
        {
            state = s;
            frame = 0;
            frameTimer = 0f;
            switch (s)
            {
                case State.Fuse:
                    timer = fuseTime;
                    if (audioSource != null && armClip != null) audioSource.PlayOneShot(armClip);
                    break;
                case State.Exploding:
                    timer = (explodeFrames != null && explodeFrames.Length > 0)
                        ? explodeFrames.Length / Mathf.Max(1f, explodeFps) : 0.5f;
                    if (visual != null && explodeFrames != null && explodeFrames.Length > 0)
                        visual.sprite = explodeFrames[0];
                    break;
                case State.Cooldown:
                    timer = cooldownTime;
                    if (visual != null) visual.enabled = false;
                    break;
                case State.Idle:
                    if (visual != null)
                    {
                        visual.enabled = true;
                        if (idleFrames != null && idleFrames.Length > 0) visual.sprite = idleFrames[0];
                    }
                    break;
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, armRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, blastRadius);
        }
    }
}
