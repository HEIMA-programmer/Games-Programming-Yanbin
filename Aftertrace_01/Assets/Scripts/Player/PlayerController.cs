using System;
using UnityEngine;

namespace EchoShift
{
    /// <summary>
    /// Snappy 2D platformer movement: fast acceleration, variable jump height,
    /// coyote time + jump buffer, gravity shaping for a tight, non-floaty feel.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Move")]
        public float moveSpeed = 7.5f;
        public float groundAccel = 95f;
        public float groundDecel = 115f;
        public float airAccel = 45f;
        public float airDecel = 32f;

        [Header("Jump")]
        public float jumpForce = 15f;
        public float baseGravity = 4f;
        public float fallMultiplier = 1.8f;
        public float lowJumpMultiplier = 2.4f;
        public float coyoteTime = 0.1f;
        public float jumpBuffer = 0.1f;
        public float maxFallSpeed = 24f;

        [Header("Ground Check")]
        public Vector2 groundCheckOffset = new Vector2(0f, -0.52f);
        // Narrower than the 0.62 capsule on purpose: walls share the Ground layer, so a
        // box poking past the capsule's flanks reads a hugged wall as "ground" and refills
        // coyote time every frame — infinite wall-climb. 0.5 keeps a 0.05+ margin to a
        // touching wall while the box still reaches 0.09 below the feet for real floors.
        public Vector2 groundCheckSize = new Vector2(0.5f, 0.14f);
        public LayerMask groundMask;

        [Header("Landing")]
        public float hardLandingSpeed = 14f;

        [Header("Audio")]
        public AudioSource audioSource;
        public AudioClip jumpClip;
        public AudioClip landClip;
        public AudioClip footstepClip;
        public float footstepInterval = 0.3f;
        public float footstepMinSpeed = 1.5f;
        public float landClipMinSpeed = 3f;

        public Vector2 Velocity => rb ? rb.velocity : Vector2.zero;
        public bool IsGrounded { get; private set; }
        public bool FacingRight { get; private set; } = true;
        public float MoveInput { get; private set; }
        public bool ControlEnabled { get; set; } = true;

        /// <summary>Fired the frame the player touches ground; arg = downward speed at impact.</summary>
        public event Action<float> Landed;

        Rigidbody2D rb;
        float coyoteCounter;
        float bufferCounter;
        bool jumpHeld;
        bool wasGrounded;
        float footstepTimer;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = baseGravity;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        void Update()
        {
            if (!ControlEnabled || GameManager.Paused)
            {
                MoveInput = 0f;
                jumpHeld = false;
                return;
            }

            MoveInput = Input.GetAxisRaw("Horizontal");
            if (MoveInput > 0.01f) FacingRight = true;
            else if (MoveInput < -0.01f) FacingRight = false;

            bool jumpPressed = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
            jumpHeld = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
            if (jumpPressed) bufferCounter = jumpBuffer;

            if (IsGrounded && Mathf.Abs(Velocity.x) > footstepMinSpeed)
            {
                footstepTimer -= Time.deltaTime;
                if (footstepTimer <= 0f)
                {
                    if (audioSource != null && footstepClip != null) audioSource.PlayOneShot(footstepClip, 0.5f);
                    footstepTimer = footstepInterval;
                }
            }
            else footstepTimer = 0f;
        }

        void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            float fallSpeedBefore = rb.velocity.y;

            IsGrounded = Physics2D.OverlapBox(
                (Vector2)transform.position + groundCheckOffset, groundCheckSize, 0f, groundMask) != null;

            if (IsGrounded) coyoteCounter = coyoteTime;
            else coyoteCounter -= dt;
            if (bufferCounter > 0f) bufferCounter -= dt;

            // Horizontal acceleration toward target speed.
            float target = MoveInput * moveSpeed;
            float accel;
            if (Mathf.Abs(target) > 0.01f) accel = IsGrounded ? groundAccel : airAccel;
            else accel = IsGrounded ? groundDecel : airDecel;
            float newX = Mathf.MoveTowards(rb.velocity.x, target, accel * dt);
            rb.velocity = new Vector2(newX, rb.velocity.y);

            // Jump (consumes coyote + buffer).
            if (bufferCounter > 0f && coyoteCounter > 0f)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                bufferCounter = 0f;
                coyoteCounter = 0f;
                if (audioSource != null && jumpClip != null) audioSource.PlayOneShot(jumpClip);
            }

            // Gravity shaping: heavier on the way down, cut short jumps.
            if (rb.velocity.y < -0.01f) rb.gravityScale = baseGravity * fallMultiplier;
            else if (rb.velocity.y > 0.01f && !jumpHeld) rb.gravityScale = baseGravity * lowJumpMultiplier;
            else rb.gravityScale = baseGravity;

            if (rb.velocity.y < -maxFallSpeed)
                rb.velocity = new Vector2(rb.velocity.x, -maxFallSpeed);

            if (IsGrounded && !wasGrounded)
            {
                float impact = -fallSpeedBefore;
                Landed?.Invoke(impact);
                if (impact >= landClipMinSpeed && audioSource != null && landClip != null)
                    audioSource.PlayOneShot(landClip);
            }
            wasGrounded = IsGrounded;
        }

        public void Respawn(Vector3 position)
        {
            transform.position = position;
            if (rb != null) rb.velocity = Vector2.zero;
            coyoteCounter = 0f;
            bufferCounter = 0f;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube((Vector2)transform.position + groundCheckOffset, groundCheckSize);
        }
    }
}
