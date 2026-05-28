using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EchoShift
{
    /// <summary>
    /// Replays recorded player frames via a kinematic Rigidbody2D so its trigger
    /// collider still activates pressure plates. Expands a ripple on spawn and
    /// dissolves into rising particles when playback ends.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class EchoClone : MonoBehaviour
    {
        public Transform ripple;                 // child ring scaled up on materialize
        public ParticleSystem dissolveParticles; // upward burst on dissolve
        public float dissolveTime = 0.5f;
        public float materializeTime = 0.32f;
        public float floatUpSpeed = 1.2f;

        [Header("Audio")]
        public AudioSource audioSource;
        public AudioClip dissolveClip;

        [Header("Ride")]
        /// <summary>One-way solid top surface (BoxCollider2D + PlatformEffector2D) the player can stand on.
        /// Builder assigns; disabled on dissolve so the rising echo cannot drag the player up.</summary>
        public Collider2D standpointCollider;

        /// <summary>The one clone currently in play, or null. Enemies use this to target the decoy.</summary>
        public static EchoClone Active { get; private set; }

        Rigidbody2D rb;
        SpriteRenderer sr;
        List<RecordedFrame> frames;
        int index;
        bool playing;
        bool dissolving;
        bool registered;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            sr = GetComponent<SpriteRenderer>();
        }

        public void Play(List<RecordedFrame> recorded)
        {
            Active = this;
            frames = recorded;
            index = 0;
            playing = frames != null && frames.Count > 0;
            if (playing)
            {
                transform.position = frames[0].position;
                rb.position = frames[0].position;
            }

            if (GameManager.Instance != null) { GameManager.Instance.RegisterClone(); registered = true; }
            StartCoroutine(Materialize());
        }

        void FixedUpdate()
        {
            if (!playing) return;

            if (index >= frames.Count)
            {
                BeginDissolve();
                return;
            }

            RecordedFrame f = frames[index];
            Vector2 cur = rb.position;
            Vector2 next = f.position;
            Vector2 delta = next - cur;

            rb.MovePosition(next);
            if (sr != null) sr.flipX = !f.facingRight;
            index++;

            if (delta.sqrMagnitude > 0f) CarryRiders(delta);
        }

        // Carry whoever overlaps a thin strip above the standpoint (effector makes Enter/Exit unreliable).
        void CarryRiders(Vector2 delta)
        {
            if (standpointCollider == null || !standpointCollider.enabled) return;

            Bounds b = standpointCollider.bounds;
            float topY = b.max.y;
            Vector2 checkCenter = new Vector2(b.center.x, topY + 0.04f);
            Vector2 checkSize = new Vector2(b.size.x - 0.04f, 0.12f);

            var hits = Physics2D.OverlapBoxAll(checkCenter, checkSize, 0f);
            for (int i = 0; i < hits.Length; i++)
            {
                var p = hits[i].GetComponentInParent<PlayerController>();
                if (p == null) continue;
                // Ignore jumps passing through from below (feet below standpoint top).
                if (hits[i].bounds.min.y < topY - 0.02f) continue;
                var body = p.GetComponent<Rigidbody2D>();
                if (body != null) body.MovePosition(body.position + delta);
            }
        }

        public void BeginDissolve()
        {
            if (dissolving) return;
            dissolving = true;
            playing = false;
            if (standpointCollider != null) standpointCollider.enabled = false;
            if (dissolveParticles != null) dissolveParticles.Play();
            if (audioSource != null && dissolveClip != null) audioSource.PlayOneShot(dissolveClip);
            StartCoroutine(DissolveRoutine());
        }

        IEnumerator Materialize()
        {
            if (ripple == null) yield break;
            ripple.gameObject.SetActive(true);
            SpriteRenderer rs = ripple.GetComponent<SpriteRenderer>();
            float t = 0f;
            Vector3 from = Vector3.one * 0.2f;
            Vector3 to = Vector3.one * 2.4f;
            while (t < materializeTime)
            {
                t += Time.deltaTime;
                float n = t / materializeTime;
                ripple.localScale = Vector3.Lerp(from, to, n);
                if (rs != null)
                {
                    Color c = rs.color;
                    c.a = Mathf.Lerp(0.9f, 0f, n);
                    rs.color = c;
                }
                yield return null;
            }
            ripple.gameObject.SetActive(false);
        }

        IEnumerator DissolveRoutine()
        {
            float t = 0f;
            Color start = sr != null ? sr.color : Color.white;
            while (t < dissolveTime)
            {
                t += Time.deltaTime;
                float n = t / dissolveTime;
                if (sr != null)
                {
                    Color c = start;
                    c.a = Mathf.Lerp(start.a, 0f, n);
                    sr.color = c;
                }
                transform.position += Vector3.up * floatUpSpeed * Time.deltaTime;
                yield return null;
            }

            Unregister();
            Destroy(gameObject);
        }

        void Unregister()
        {
            if (Active == this) Active = null;
            if (registered && GameManager.Instance != null)
            {
                GameManager.Instance.UnregisterClone();
                registered = false;
            }
        }

        void OnDestroy()
        {
            Unregister();
        }
    }
}
