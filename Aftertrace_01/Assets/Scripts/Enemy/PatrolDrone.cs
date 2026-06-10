using System.Collections;
using UnityEngine;

namespace EchoShift
{
    /// <summary>
    /// Level 2/3 patrol enemy with a real line-of-sight sense. Sweeps its patrol span until
    /// it spots the player or an echo clone inside its detection cone (range + angle + an
    /// unobstructed ray on the Ground layer); then it briefly alerts, chases (slower than the
    /// player so you can always outrun it), searches the last spot it saw the target, and
    /// walks home if it loses the trail. A leash caps how far it may chase so it never wanders
    /// off its zone or corners the player. Clones are seen first, so a recorded echo reliably
    /// draws the cone away — the decoy. Touching the player respawns them; touching a clone
    /// destroys it (default) or only stuns, per destroysClone.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PatrolDrone : MonoBehaviour
    {
        enum State { Patrol, Alert, Chase, Search, Return, Stunned }

        [Header("Patrol")]
        public float patrolDistance = 4f;
        public float speed = 2.5f;
        public bool startMovingRight = true;

        [Header("Sight")]
        public float viewRange = 4.5f;
        public float viewHalfAngle = 26f;
        [Tooltip("Layers that block line of sight (Ground). Set by the build script.")]
        public LayerMask sightBlockMask;
        [Tooltip("If true, holding the player in sight fills GameManager's detection meter and eventually respawns them (a stealth rule). Off by default; enabled per-drone for stealth sections.")]
        public bool detects = false;

        [Header("Chase")]
        [Tooltip("Must stay below the player's moveSpeed (7.5) so the player can always escape.")]
        public float chaseSpeed = 6f;
        public float chaseAccel = 40f;
        public float alertTime = 0.3f;        // freeze + flash before committing to the chase
        public float searchTime = 1.5f;       // look around the last-known spot before giving up
        public float returnSpeedMult = 1.3f;
        [Tooltip("How far past the patrol span the drone may chase before giving up. Keeps it on its zone and the player un-cornered.")]
        public float leash = 5f;

        [Header("Decoy / contact")]
        public float stunTime = 2.5f;
        [Tooltip("True = touching a clone destroys it. False = clone survives and only stuns the drone, so one clone can draw several enemies in sequence.")]
        public bool destroysClone = true;

        [Header("Visuals")]
        [Tooltip("Parent holding the drone sprite + searchlight; flipped to face travel direction.")]
        public Transform facing;
        [Tooltip("Legacy hard cone sprite — superseded by the searchlight. Leave null on new prefabs.")]
        public SpriteRenderer coneRenderer;
        // 1-Bit: cone state reads via ALPHA (brightness), not hue. Brighter = more alert.
        public Color coneNormal = new Color(1f, 1f, 1f, 0.22f);   // faint white — passive sweep
        public Color coneAlert = new Color(1f, 1f, 1f, 0.6f);     // bright white — spotted you
        public Color coneStunned = new Color(1f, 1f, 1f, 0.08f);  // near-invisible — disabled
        [Tooltip("Soft Light2D beam replacing the hard cone — brightness = alertness; dies when stunned.")]
        public UnityEngine.Rendering.Universal.Light2D searchLight;
        public float lightPassive = 0.8f;
        public float lightAlert = 2.2f;
        public float lightStunned = 0.05f;
        [Tooltip("Body sprite, dimmed while stunned so 'safe to pass through' reads at a glance.")]
        public SpriteRenderer bodyRenderer;

        Rigidbody2D rb;
        Vector2 origin;
        int dir;
        State state;
        float currentSpeed;
        float lastKnownX;
        float stateTimer;
        Transform playerT;
        Collider2D playerCol;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            // Contact is handled in OnTriggerEnter2D: the collider must be a trigger (a solid
            // collider just shoves the player and never reports), and full kinematic contacts
            // are needed so the kinematic echo clone still raises trigger events against us.
            rb.useFullKinematicContacts = true;
            var contact = GetComponent<Collider2D>();
            if (contact != null) contact.isTrigger = true;
            origin = rb.position;
            dir = startMovingRight ? 1 : -1;
            state = State.Patrol;
            ApplyFacing();
            ApplyConeColour();
        }

        void Start()
        {
            var pc = FindObjectOfType<PlayerController>();
            if (pc != null) { playerT = pc.transform; playerCol = pc.GetComponent<Collider2D>(); }
        }

        void FixedUpdate()
        {
            if (state == State.Stunned || GameManager.Paused) return;

            float dt = Time.fixedDeltaTime;
            bool seesPlayer = CanSee(playerT, playerCol);
            if (detects && seesPlayer && GameManager.Instance != null) GameManager.Instance.ReportSeen();
            Transform seen = AcquireTarget(seesPlayer);

            switch (state)
            {
                case State.Patrol:
                    PatrolStep(dt);
                    if (seen != null) EnterAlert(seen.position.x);
                    break;

                case State.Alert:
                    if (seen != null) lastKnownX = seen.position.x;
                    FaceToward(lastKnownX);
                    stateTimer -= dt;
                    if (stateTimer <= 0f) state = State.Chase;
                    break;

                case State.Chase:
                    if (seen != null) lastKnownX = seen.position.x;
                    FaceToward(lastKnownX);
                    currentSpeed = Mathf.MoveTowards(currentSpeed, chaseSpeed, chaseAccel * dt);
                    MoveToward(lastKnownX, currentSpeed, dt, clampToLeash: true);
                    if (seen == null) { state = State.Search; stateTimer = searchTime; }
                    // leash 0 = stationary gaze sentinel: pinned at its zone edge it keeps
                    // STARING (and reporting) instead of breaking off to walk home — walking
                    // home faces away and would reset the detection meter every sweep.
                    else if (leash > 0f && BeyondLeash()) EnterReturn();
                    break;

                case State.Search:
                    FaceToward(lastKnownX);
                    MoveToward(lastKnownX, speed, dt, clampToLeash: true);
                    stateTimer -= dt;
                    if (seen != null) EnterAlert(seen.position.x);
                    else if (stateTimer <= 0f || BeyondLeash()) EnterReturn();
                    break;

                case State.Return:
                    // Walks home and ignores the target on the way — gives the player a clean
                    // window and prevents oscillating at the leash boundary. Re-detects once
                    // it is back on patrol.
                    FaceToward(origin.x);
                    MoveToward(origin.x, speed * returnSpeedMult, dt);
                    if (Mathf.Abs(rb.position.x - origin.x) < 0.05f) ResumePatrol();
                    break;
            }
        }

        // ---- sensing ----
        Transform AcquireTarget(bool seesPlayer)
        {
            EchoClone clone = EchoClone.Active;
            if (clone != null && CanSee(clone.transform, clone.GetComponent<Collider2D>()))
                return clone.transform;
            return seesPlayer ? playerT : null;
        }

        bool CanSee(Transform t, Collider2D col)
        {
            if (t == null) return false;
            Vector2 eye = rb.position;
            Vector2 target = col != null ? (Vector2)col.bounds.center : (Vector2)t.position;
            Vector2 to = target - eye;
            float dist = to.magnitude;
            if (dist > viewRange) return false;
            if (dist > 0.01f)
            {
                if (Vector2.Angle(new Vector2(dir, 0f), to) > viewHalfAngle) return false;
                if (Physics2D.Raycast(eye, to / dist, dist, sightBlockMask).collider != null) return false;
            }
            return true;
        }

        // ---- movement ----
        void PatrolStep(float dt)
        {
            Vector2 pos = rb.position;
            pos.x += dir * speed * dt;
            if (pos.x > origin.x + patrolDistance) { pos.x = origin.x + patrolDistance; dir = -1; ApplyFacing(); }
            else if (pos.x < origin.x - patrolDistance) { pos.x = origin.x - patrolDistance; dir = 1; ApplyFacing(); }
            rb.MovePosition(pos);
        }

        void MoveToward(float targetX, float spd, float dt, bool clampToLeash = false)
        {
            Vector2 pos = rb.position;
            pos.x = Mathf.MoveTowards(pos.x, targetX, spd * dt);
            if (clampToLeash)
            {
                float bound = patrolDistance + leash;
                pos.x = Mathf.Clamp(pos.x, origin.x - bound, origin.x + bound);
            }
            rb.MovePosition(pos);
        }

        bool BeyondLeash() => Mathf.Abs(rb.position.x - origin.x) >= patrolDistance + leash - 0.02f;

        // ---- transitions ----
        void EnterAlert(float targetX)
        {
            state = State.Alert;
            stateTimer = alertTime;
            lastKnownX = targetX;
            currentSpeed = 0f;
            FaceToward(targetX);
            ApplyConeColour();
        }

        void EnterReturn()
        {
            state = State.Return;
            currentSpeed = 0f;
            ApplyConeColour();
        }

        void ResumePatrol()
        {
            state = State.Patrol;
            currentSpeed = 0f;
            ApplyConeColour();
        }

        void FaceToward(float targetX)
        {
            int want = targetX >= rb.position.x ? 1 : -1;
            if (want != dir) { dir = want; ApplyFacing(); }
        }

        void ApplyFacing()
        {
            if (facing == null) return;
            Vector3 s = facing.localScale;
            s.x = Mathf.Abs(s.x) * (dir >= 0 ? 1f : -1f);
            facing.localScale = s;
        }

        void ApplyConeColour()
        {
            Color c;
            float intensity;
            switch (state)
            {
                case State.Alert:
                case State.Chase:
                case State.Search: c = coneAlert; intensity = lightAlert; break;
                case State.Stunned: c = coneStunned; intensity = lightStunned; break;
                default: c = coneNormal; intensity = lightPassive; break;   // Patrol, Return
            }
            if (coneRenderer != null) coneRenderer.color = c;
            if (searchLight != null) searchLight.intensity = intensity;
            if (bodyRenderer != null)
            {
                Color bc = bodyRenderer.color;
                bc.a = state == State.Stunned ? 0.45f : 1f;
                bodyRenderer.color = bc;
            }
        }

        // ---- decoy / contact ----
        void OnTriggerEnter2D(Collider2D other)
        {
            // Powered down: harmless until it reboots. This is what makes the decoy loop
            // work on flat ground — the player escapes THROUGH the sleeping body.
            if (state == State.Stunned) return;

            EchoClone clone = other.GetComponentInParent<EchoClone>();
            if (clone != null)
            {
                if (destroysClone) clone.BeginDissolve();
                Stun();
                return;
            }

            PlayerController player = other.GetComponentInParent<PlayerController>();
            if (player != null && GameManager.Instance != null)
                GameManager.Instance.RespawnPlayer();
        }

        void Stun()
        {
            StopAllCoroutines();
            StartCoroutine(StunRoutine());
        }

        IEnumerator StunRoutine()
        {
            state = State.Stunned;
            currentSpeed = 0f;
            ApplyConeColour();
            float t = 0f;
            while (t < stunTime) { t += Time.deltaTime; yield return null; }
            EnterReturn();   // shake it off and head home; re-aggros once back on patrol
        }

        void OnDrawGizmosSelected()
        {
            int d = Application.isPlaying ? dir : (startMovingRight ? 1 : -1);
            Vector3 o = transform.position;
            Vector3 fwd = new Vector3(d, 0f, 0f);
            Gizmos.color = new Color(1f, 0.5f, 0.2f, 0.9f);
            Gizmos.DrawLine(o, o + Quaternion.Euler(0, 0, viewHalfAngle) * fwd * viewRange);
            Gizmos.DrawLine(o, o + Quaternion.Euler(0, 0, -viewHalfAngle) * fwd * viewRange);
        }
    }
}
