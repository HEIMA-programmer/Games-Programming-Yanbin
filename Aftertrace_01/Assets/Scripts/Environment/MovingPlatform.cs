using System.Collections.Generic;
using UnityEngine;

namespace EchoShift
{
    /// <summary>
    /// Kinematic platform with two modes:
    /// - PlateActivated: moves to point B while its plate is held, returns to A when released.
    /// - PingPong: continuously travels A↔B (with an optional pause at each end).
    /// Point B = A + moveOffset (or (0, riseHeight) if moveOffset is zero). Carries any
    /// standing player by applying its per-step delta to their Rigidbody2D.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class MovingPlatform : MonoBehaviour
    {
        public enum PlatformMode { PlateActivated, PingPong }

        public PlatformMode mode = PlatformMode.PlateActivated;
        public PressurePlate plate;
        public Vector2 moveOffset = Vector2.zero;   // B relative to A; zero => (0, riseHeight)
        public float riseHeight = 4.5f;
        public float moveSpeed = 3.5f;
        public float waitTime = 0.4f;               // ping-pong pause at each endpoint

        Rigidbody2D rb;
        Vector2 pointA;
        Vector2 pointB;
        int pingDir = 1;
        float waitTimer;
        readonly List<Rigidbody2D> riders = new List<Rigidbody2D>();

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            pointA = rb.position;
            Vector2 off = (moveOffset == Vector2.zero) ? new Vector2(0f, riseHeight) : moveOffset;
            pointB = pointA + off;
        }

        void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            Vector2 cur = rb.position;
            Vector2 target;

            if (mode == PlatformMode.PlateActivated)
            {
                target = (plate != null && plate.IsPressed) ? pointB : pointA;
            }
            else // PingPong
            {
                if (waitTimer > 0f) { waitTimer -= dt; target = cur; }
                else target = (pingDir > 0) ? pointB : pointA;
            }

            Vector2 next = Vector2.MoveTowards(cur, target, moveSpeed * dt);
            Vector2 delta = next - cur;
            rb.MovePosition(next);

            if (delta.sqrMagnitude > 0f)
            {
                for (int i = riders.Count - 1; i >= 0; i--)
                {
                    if (riders[i] == null) { riders.RemoveAt(i); continue; }
                    riders[i].MovePosition(riders[i].position + delta);
                }
            }

            if (mode == PlatformMode.PingPong && waitTimer <= 0f && (next - target).sqrMagnitude < 0.0001f)
            {
                pingDir = -pingDir;
                waitTimer = waitTime;
            }
        }

        void OnCollisionEnter2D(Collision2D c)
        {
            PlayerController p = c.collider.GetComponentInParent<PlayerController>();
            if (p == null) return;
            Rigidbody2D body = p.GetComponent<Rigidbody2D>();
            if (body != null && !riders.Contains(body)) riders.Add(body);
        }

        void OnCollisionExit2D(Collision2D c)
        {
            PlayerController p = c.collider.GetComponentInParent<PlayerController>();
            if (p == null) return;
            Rigidbody2D body = p.GetComponent<Rigidbody2D>();
            if (body != null) riders.Remove(body);
        }
    }
}
