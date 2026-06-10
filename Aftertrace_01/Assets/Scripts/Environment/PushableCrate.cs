using System.Collections.Generic;
using UnityEngine;

namespace EchoShift
{
    /// <summary>
    /// Pushable supply crate, "record the world you touched" style:
    /// while the player records, EchoRecorder snapshots every crate each tick;
    /// on release the disturbed crates rewind to where the recording began and
    /// replay alongside the echo clone. The body stays kinematic — gravity and
    /// pushing are integrated by hand, and a replay just plays back recorded
    /// positions, so it can never diverge from what actually happened.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class PushableCrate : MonoBehaviour
    {
        /// <summary>Every enabled crate in the scene (EchoRecorder snapshots these).</summary>
        public static readonly List<PushableCrate> All = new List<PushableCrate>();

        public float pushSpeed = 2.0f;
        public float gravity = 32f;
        public float maxFallSpeed = 16f;
        public LayerMask solidMask;          // what blocks/supports the crate (terrain + other crates)
        [Tooltip("How far from the side faces the player is detected as pushing.")]
        public float pushProbe = 0.12f;

        public bool IsReplaying { get; private set; }
        public Vector2 Position => rb.position;

        const float Skin = 0.02f;

        Rigidbody2D rb;
        BoxCollider2D box;
        float vy;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            box = GetComponent<BoxCollider2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.useFullKinematicContacts = true;   // pressure plates + player contacts still fire
        }

        void OnEnable() { All.Add(this); }
        void OnDisable() { All.Remove(this); }

        void FixedUpdate()
        {
            if (IsReplaying) return;

            float dt = Time.fixedDeltaTime;
            Vector2 pos = rb.position;

            // --- vertical: hand-rolled gravity with a downward box cast
            vy = Mathf.Max(vy - gravity * dt, -maxFallSpeed);
            float dy = vy * dt;   // <= 0
            RaycastHit2D ground = Cast(pos, Vector2.down, -dy + Skin);
            bool supported;
            if (ground.collider != null)
            {
                pos.y -= Mathf.Max(0f, ground.distance - Skin);
                vy = 0f;
                supported = true;
            }
            else
            {
                pos.y += dy;
                supported = false;
            }

            // --- horizontal: the player walking into a side face pushes the crate
            if (supported)
            {
                float dir = PushDirection(pos);
                if (dir != 0f)
                {
                    float dx = dir * pushSpeed * dt;
                    RaycastHit2D wall = Cast(pos, new Vector2(dir, 0f), Mathf.Abs(dx) + Skin);
                    if (wall.collider != null) dx = dir * Mathf.Max(0f, wall.distance - Skin);
                    pos.x += dx;
                }
            }

            rb.MovePosition(pos);
        }

        // -1 push left, +1 push right, 0 none. The player must overlap a probe strip
        // beside the crate AND hold movement toward it while grounded.
        float PushDirection(Vector2 pos)
        {
            Vector2 c = pos + box.offset;
            Vector2 probeSize = new Vector2(pushProbe, box.size.y * 0.8f);
            for (int side = -1; side <= 1; side += 2)
            {
                Vector2 probeCenter = c + new Vector2(side * (box.size.x * 0.5f + pushProbe * 0.5f), 0f);
                var hits = Physics2D.OverlapBoxAll(probeCenter, probeSize, 0f);
                foreach (var h in hits)
                {
                    var pc = h.GetComponentInParent<PlayerController>();
                    if (pc == null || !pc.ControlEnabled || !pc.IsGrounded) continue;
                    if (pc.MoveInput * side < -0.25f) return -side;
                }
            }
            return 0f;
        }

        RaycastHit2D Cast(Vector2 pos, Vector2 dir, float dist)
        {
            Vector2 origin = pos + box.offset;
            Vector2 size = box.size - new Vector2(Skin * 2f, Skin * 2f);
            var hits = Physics2D.BoxCastAll(origin, size, 0f, dir, dist, solidMask);
            RaycastHit2D best = default;
            foreach (var h in hits)
            {
                if (h.collider == box) continue;   // own collider is on the solid layer too
                if (best.collider == null || h.distance < best.distance) best = h;
            }
            return best;
        }

        // ---- echo replay control (driven by EchoRecorder / EchoClone) ----

        /// <summary>Lock the crate and teleport it back to where the recording began.</summary>
        public void BeginReplay(Vector2 rewindTo)
        {
            IsReplaying = true;
            vy = 0f;
            rb.position = rewindTo;
            transform.position = rewindTo;   // don't let interpolation smear the rewind
            Physics2D.SyncTransforms();
        }

        /// <summary>Advance one recorded tick (called from EchoClone.FixedUpdate with the player's frame index).</summary>
        public void ApplyReplayFrame(Vector2 framePos) => rb.MovePosition(framePos);

        /// <summary>Unlock at the current pose; gravity resumes, the puzzle result stays.</summary>
        public void EndReplay()
        {
            IsReplaying = false;
            vy = 0f;
        }
    }
}
