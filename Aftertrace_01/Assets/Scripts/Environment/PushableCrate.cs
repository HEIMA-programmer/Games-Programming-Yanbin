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
        float chainDx;   // horizontal shove handed over by a neighbour this tick; consume-or-discard

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

            // --- horizontal: player push + chain shove from a neighbouring crate, one move.
            // A crate pushed into another hands its full intent over via ChainPush; the
            // receiver consumes it in its OWN tick (kinematic MovePosition is deferred to
            // the physics step, so both update orders see identical geometry — the train
            // just ramps one frame per link and then runs at full pushSpeed).
            float maxStep = pushSpeed * dt;
            float playerDx = supported ? PushDirection(pos) * maxStep : 0f;
            // Opposite shoves cancel (Sokoban squeeze); same-direction player+ghost can't
            // exceed pushSpeed. Discard unconditionally — a shove received while falling
            // or blocked must not fire on some later frame.
            float want = Mathf.Clamp(playerDx + chainDx, -maxStep, maxStep);
            chainDx = 0f;

            if (supported && Mathf.Abs(want) > 1e-6f)
            {
                float d = Mathf.Sign(want);
                // detect 2*Skin past the move: at chain steady state the leading crate sits
                // exactly one step ahead — a tighter cast misses it every other frame (stutter)
                RaycastHit2D wall = Cast(pos, new Vector2(d, 0f), Mathf.Abs(want) + Skin * 2f);
                float dx = want;
                if (wall.collider != null)
                {
                    float gap = Mathf.Max(0f, wall.distance - Skin);
                    dx = d * Mathf.Min(Mathf.Abs(want), gap);   // explicit cap — the extended cast can return gap > |want|
                    var next = wall.collider.GetComponent<PushableCrate>();
                    if (next != null && !next.IsReplaying) next.ChainPush(want);
                    // terrain or a replaying ghost: plain wall — the stall propagates back
                    // through the train one link per frame, which is correct Sokoban feel.
                }
                pos.x += dx;
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

        /// <summary>
        /// Horizontal shove handed over by a neighbouring crate (or a replaying ghost).
        /// Accumulated, then consumed-or-discarded in our own next FixedUpdate.
        /// </summary>
        public void ChainPush(float wantDx)
        {
            if (IsReplaying) return;   // a ghost-bound crate is a wall to live pushers
            chainDx += wantDx;
        }

        // ---- echo replay control (driven by EchoRecorder / EchoClone) ----

        /// <summary>Lock the crate and teleport it back to where the recording began.</summary>
        public void BeginReplay(Vector2 rewindTo)
        {
            IsReplaying = true;
            vy = 0f;
            chainDx = 0f;   // drop shoves queued before the rewind teleport
            rb.position = rewindTo;
            transform.position = rewindTo;   // don't let interpolation smear the rewind
            Physics2D.SyncTransforms();
        }

        /// <summary>Advance one recorded tick (called from EchoClone.FixedUpdate with the player's frame index).</summary>
        public void ApplyReplayFrame(Vector2 framePos)
        {
            // The ghost re-enacts the recording verbatim — it never deviates — but it
            // displaces any LIVE crate standing in its horizontal path (the live world
            // yields to recorded reality). Per-tick delta <= pushSpeed*dt, so the shoved
            // crate keeps pace; squeezed-against-a-wall overlap resolves in EndReplay.
            float deltaX = framePos.x - rb.position.x;
            if (Mathf.Abs(deltaX) > 1e-6f)
            {
                float d = Mathf.Sign(deltaX);
                RaycastHit2D hit = Cast(rb.position, new Vector2(d, 0f), Mathf.Abs(deltaX) + Skin * 2f);
                var live = hit.collider != null ? hit.collider.GetComponent<PushableCrate>() : null;
                if (live != null && !live.IsReplaying) live.ChainPush(deltaX);
            }
            rb.MovePosition(framePos);
        }

        /// <summary>Unlock at the current pose; gravity resumes, the puzzle result stays.</summary>
        public void EndReplay()
        {
            IsReplaying = false;
            vy = 0f;
            chainDx = 0f;
            DepenetrateLiveCrates();
        }

        // A ghost squeezed a live crate against a wall during replay (the live crate
        // clamps to 0 while the verbatim ghost keeps advancing — transient overlap is
        // unavoidable). On unlock, nudge the intruder out along the shorter horizontal
        // exit, mirroring EchoRecorder.NudgePlayerOutOfCrates.
        void DepenetrateLiveCrates()
        {
            Physics2D.SyncTransforms();
            Bounds mine = box.bounds;
            foreach (var c in All)
            {
                if (c == this || c.IsReplaying) continue;
                Bounds theirs = c.box.bounds;
                if (!mine.Intersects(theirs)) continue;
                float side = theirs.center.x >= mine.center.x ? 1f : -1f;
                float centerX = mine.center.x + side * (mine.extents.x + theirs.extents.x + Skin);
                Vector2 p = new Vector2(centerX - c.box.offset.x, c.rb.position.y);
                c.rb.position = p;
                c.transform.position = p;
                Physics2D.SyncTransforms();
            }
        }
    }
}
