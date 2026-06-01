using UnityEngine;

namespace EchoShift
{
    /// <summary>
    /// Procedural, code-driven character animation: idle float, run tilt + bob,
    /// jump stretch / fall squash, landing squash, and foot dust. No sprite sheets.
    /// Operates on a visual child so the physics collider is never deformed.
    /// </summary>
    public class PlayerAnimator : MonoBehaviour
    {
        public Transform visual;
        public PlayerController controller;
        public ParticleSystem footDust;

        [Header("Idle")]
        public float idleBobAmp = 0.06f;
        public float idleBobSpeed = 3.5f;

        [Header("Run")]
        public float runTilt = 8f;
        public float runBobAmp = 0.05f;
        public float runBobSpeed = 14f;
        public float runDustInterval = 0.14f;

        [Header("Air")]
        public float stretchAmount = 0.16f;

        [Header("Landing")]
        public float landSquash = 0.34f;
        public float landRecoverRate = 4.0f;
        public float landDustThreshold = 9f;

        [Header("Sprite Frames (optional — leave empty to stay fully procedural)")]
        public Sprite defaultSprite;          // shown for a state whose frame list is empty
        public Sprite[] idleFrames;
        public Sprite[] runFrames;
        public Sprite[] jumpFrames;           // rising  (velocity.y up)
        public Sprite[] fallFrames;           // falling (velocity.y down)
        public float frameIdleFps = 6f;
        public float frameRunFps = 12f;
        public float frameJumpFps = 12f;
        public float frameFallFps = 10f;

        Vector3 baseScale;
        SpriteRenderer sr;
        float bobTimer;
        float landStrength;
        float runDustTimer;
        Sprite[] curFrames;
        int curFrame;
        float frameTimer;

        void Reset() { visual = transform; }

        void Awake()
        {
            if (!controller) controller = GetComponentInParent<PlayerController>();
            if (!visual) visual = transform;
            baseScale = visual.localScale;
            sr = visual.GetComponent<SpriteRenderer>();
        }

        void OnEnable()
        {
            if (controller != null) controller.Landed += OnLanded;
        }

        void OnDisable()
        {
            if (controller != null) controller.Landed -= OnLanded;
        }

        void OnLanded(float impact)
        {
            landStrength = Mathf.Clamp01((impact - 3f) / 16f);
            if (footDust != null && impact >= landDustThreshold)
                footDust.Emit(Mathf.RoundToInt(Mathf.Lerp(4f, 12f, landStrength)));
        }

        void Update()
        {
            if (controller == null || visual == null) return;
            float dt = Time.deltaTime;
            Vector2 v = controller.Velocity;
            bool grounded = controller.IsGrounded;
            float move = Mathf.Abs(controller.MoveInput);
            float dir = controller.FacingRight ? 1f : -1f;

            if (sr != null) sr.flipX = !controller.FacingRight;
            bool framed = UpdateFrames(grounded, move, v.y, dt);

            // When sprite frames drive the look, keep the visual neutral — procedural
            // stretch/tilt/bob would distort the frame and rotate pixels (blurry on 1-bit art).
            if (framed)
            {
                float kf = 1f - Mathf.Exp(-25f * dt);
                visual.localScale = Vector3.Lerp(visual.localScale, baseScale, kf);
                visual.localRotation = Quaternion.Lerp(visual.localRotation, Quaternion.identity, kf);
                Vector3 lpf = visual.localPosition;
                lpf.y = Mathf.Lerp(lpf.y, 0f, kf);
                visual.localPosition = lpf;
                return;
            }

            Vector3 scale = baseScale;
            float tilt = 0f;
            float bobY = 0f;

            if (!grounded)
            {
                float vy = Mathf.Clamp(v.y / 12f, -1f, 1f);
                scale.y = baseScale.y * (1f + stretchAmount * vy);
                scale.x = baseScale.x * (1f - stretchAmount * vy * 0.6f);
            }
            else if (move > 0.1f)
            {
                bobTimer += dt * runBobSpeed;
                bobY = Mathf.Abs(Mathf.Sin(bobTimer)) * runBobAmp;
                tilt = -dir * runTilt;

                runDustTimer -= dt;
                if (footDust != null && runDustTimer <= 0f)
                {
                    footDust.Emit(1);
                    runDustTimer = runDustInterval;
                }
            }
            else
            {
                bobTimer += dt * idleBobSpeed;
                bobY = Mathf.Sin(bobTimer) * idleBobAmp;
            }

            if (landStrength > 0f)
            {
                float s = landStrength * landSquash;
                scale.y = baseScale.y * (1f - s);
                scale.x = baseScale.x * (1f + s * 0.7f);
                landStrength = Mathf.MoveTowards(landStrength, 0f, landRecoverRate * dt);
            }

            float k = 1f - Mathf.Exp(-25f * dt);
            visual.localScale = Vector3.Lerp(visual.localScale, scale, k);
            visual.localRotation = Quaternion.Lerp(visual.localRotation, Quaternion.Euler(0f, 0f, tilt), 1f - Mathf.Exp(-15f * dt));
            Vector3 lp = visual.localPosition;
            lp.y = Mathf.Lerp(lp.y, bobY, 1f - Mathf.Exp(-20f * dt));
            visual.localPosition = lp;
        }

        // Optional sprite-frame animation. Returns TRUE if it drove a frame this update
        // (so Update can suppress the procedural squash/tilt that would distort the frame).
        // Ground states (idle/run) LOOP; air states (jump/fall) play once and HOLD the last
        // frame — looping a jump is what makes the pose look "scrambled".
        bool UpdateFrames(bool grounded, float move, float vy, float dt)
        {
            if (sr == null) return false;
            Sprite[] want = !grounded ? (vy > 0.1f ? jumpFrames : fallFrames)
                          : (move > 0.1f ? runFrames : idleFrames);
            float fps = !grounded ? (vy > 0.1f ? frameJumpFps : frameFallFps)
                      : (move > 0.1f ? frameRunFps : frameIdleFps);
            bool loop = grounded; // jump/fall do not loop

            if (want == null || want.Length == 0)
            {
                if (defaultSprite != null) { sr.sprite = defaultSprite; curFrames = null; return true; }
                curFrames = null;
                return false; // nothing to drive -> stay fully procedural
            }
            if (want != curFrames) { curFrames = want; curFrame = 0; frameTimer = 0f; sr.sprite = want[0]; return true; }

            frameTimer += dt;
            if (frameTimer >= 1f / Mathf.Max(1f, fps))
            {
                frameTimer -= 1f / Mathf.Max(1f, fps);
                if (loop) curFrame = (curFrame + 1) % want.Length;
                else curFrame = Mathf.Min(curFrame + 1, want.Length - 1); // hold last frame
                sr.sprite = want[curFrame];
            }
            return true;
        }
    }
}
