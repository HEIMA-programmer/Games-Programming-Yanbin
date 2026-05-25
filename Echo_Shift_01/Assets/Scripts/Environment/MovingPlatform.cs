using System.Collections.Generic;
using UnityEngine;

namespace EchoShift
{
    /// <summary>
    /// Kinematic platform that rises while its plate is pressed and lowers otherwise.
    /// Carries any standing player by applying its per-step delta to their Rigidbody2D.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class MovingPlatform : MonoBehaviour
    {
        public PressurePlate plate;
        public float riseHeight = 4.5f;
        public float moveSpeed = 3.5f;

        Rigidbody2D rb;
        Vector2 lowPos;
        Vector2 highPos;
        readonly List<Rigidbody2D> riders = new List<Rigidbody2D>();

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            lowPos = rb.position;
            highPos = lowPos + Vector2.up * riseHeight;
        }

        void FixedUpdate()
        {
            bool up = plate != null && plate.IsPressed;
            Vector2 target = up ? highPos : lowPos;
            Vector2 cur = rb.position;
            Vector2 next = Vector2.MoveTowards(cur, target, moveSpeed * Time.fixedDeltaTime);
            Vector2 delta = next - cur;
            rb.MovePosition(next);

            if (delta.sqrMagnitude <= 0f) return;
            for (int i = riders.Count - 1; i >= 0; i--)
            {
                if (riders[i] == null) { riders.RemoveAt(i); continue; }
                riders[i].MovePosition(riders[i].position + delta);
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
