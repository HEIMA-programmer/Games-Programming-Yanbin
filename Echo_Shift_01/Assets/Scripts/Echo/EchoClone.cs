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
            rb.MovePosition(f.position);
            if (sr != null) sr.flipX = !f.facingRight;
            index++;
        }

        public void BeginDissolve()
        {
            if (dissolving) return;
            dissolving = true;
            playing = false;
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
