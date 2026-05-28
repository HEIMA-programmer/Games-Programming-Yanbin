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

        Vector3 baseScale;
        SpriteRenderer sr;
        float bobTimer;
        float landStrength;
        float runDustTimer;

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
    }
}
