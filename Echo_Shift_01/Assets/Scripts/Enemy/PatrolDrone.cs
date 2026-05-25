using System.Collections;
using UnityEngine;

namespace EchoShift
{
    /// <summary>
    /// Level 2 patrol enemy: a kinematic drone moving back and forth at constant speed.
    /// Touching the player → respawn at last checkpoint. Touching an echo clone →
    /// destroy the clone and stun (the clone acts as a decoy that draws its attention).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PatrolDrone : MonoBehaviour
    {
        public float patrolDistance = 4f;
        public float speed = 2.5f;
        public bool startMovingRight = true;
        public float stunTime = 2.5f;
        [Tooltip("True = touching a clone destroys it (Level 2). False = clone survives and only stuns the drone, so one clone can draw several enemies (Level 3 decoy corridor).")]
        public bool destroysClone = true;

        [Tooltip("Parent holding the drone sprite + detection cone; flipped to face travel direction.")]
        public Transform facing;
        public SpriteRenderer coneRenderer;
        public Color coneNormal = new Color(1f, 0.45f, 0.2f, 0.28f);
        public Color coneAlert = new Color(1f, 0.12f, 0.1f, 0.55f);

        Rigidbody2D rb;
        Vector2 origin;
        int dir;
        bool stunned;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            origin = rb.position;
            dir = startMovingRight ? 1 : -1;
            ApplyFacing();
            if (coneRenderer != null) coneRenderer.color = coneNormal;
        }

        void FixedUpdate()
        {
            if (stunned || GameManager.Paused) return;

            Vector2 pos = rb.position;
            pos.x += dir * speed * Time.fixedDeltaTime;
            if (pos.x > origin.x + patrolDistance) { pos.x = origin.x + patrolDistance; dir = -1; ApplyFacing(); }
            else if (pos.x < origin.x - patrolDistance) { pos.x = origin.x - patrolDistance; dir = 1; ApplyFacing(); }
            rb.MovePosition(pos);
        }

        void ApplyFacing()
        {
            if (facing == null) return;
            Vector3 s = facing.localScale;
            s.x = Mathf.Abs(s.x) * (dir >= 0 ? 1f : -1f);
            facing.localScale = s;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
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
            stunned = true;
            if (coneRenderer != null) coneRenderer.color = coneAlert;
            float t = 0f;
            while (t < stunTime) { t += Time.deltaTime; yield return null; }
            stunned = false;
            if (coneRenderer != null) coneRenderer.color = coneNormal;
        }
    }
}
