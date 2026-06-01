using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoShift
{
    /// <summary>
    /// Proximity exit door. Sequence when the player enters the doorway:
    ///   open frames -> player walks in & fades out -> close frames -> screen fade to next scene.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class AutoDoorExit : MonoBehaviour
    {
        [Tooltip("Door body whose sprite gets swapped. Its SpriteRenderer is found automatically.")]
        public Transform doorBody;
        [Tooltip("Open animation, in order closed -> fully open (e.g. Door_0 .. Door_3).")]
        public Sprite[] openFrames;
        [Tooltip("Close animation, open -> closed. Leave empty to reuse openFrames reversed.")]
        public Sprite[] closeFrames;
        public AudioSource audioSource;
        public AudioClip openClip;
        public AudioClip closeClip;
        public string targetScene = "Level_01";

        [Header("Timing")]
        public float openTime = 0.35f;
        public float closeTime = 0.3f;
        public float playerFadeTime = 0.4f;   // player dissolves while "entering" the door
        public float pauseAfterClose = 0.25f;  // beat on the closed door before the screen fade

        SpriteRenderer doorRenderer;
        bool fired;

        void Awake()
        {
            if (doorBody != null) doorRenderer = doorBody.GetComponent<SpriteRenderer>();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (fired) return;
            if (other.GetComponentInParent<PlayerController>() == null) return;
            fired = true;
            StartCoroutine(Sequence(other.GetComponentInParent<PlayerController>()));
        }

        IEnumerator Sequence(PlayerController player)
        {
            // freeze player control so they can't walk back out mid-sequence
            if (player != null) player.ControlEnabled = false;

            // 1) open
            if (audioSource != null && openClip != null) audioSource.PlayOneShot(openClip);
            yield return PlayFrames(openFrames, openTime, false);

            // 2) player fades out (walks into the doorway)
            if (player != null) yield return FadePlayer(player, playerFadeTime);

            // 3) close
            if (audioSource != null && closeClip != null) audioSource.PlayOneShot(closeClip);
            Sprite[] closing = (closeFrames != null && closeFrames.Length > 0) ? closeFrames : Reversed(openFrames);
            yield return PlayFrames(closing, closeTime, true);

            yield return new WaitForSeconds(pauseAfterClose);

            // 4) screen fade to next scene
            if (SceneFader.Instance != null) SceneFader.Instance.FadeToScene(targetScene);
            else SceneManager.LoadScene(targetScene);
        }

        IEnumerator PlayFrames(Sprite[] frames, float dur, bool holdLast)
        {
            if (doorRenderer == null || frames == null || frames.Length == 0) yield break;
            float t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                int i = Mathf.Clamp(Mathf.FloorToInt(t / dur * frames.Length), 0, frames.Length - 1);
                doorRenderer.sprite = frames[i];
                yield return null;
            }
            doorRenderer.sprite = frames[frames.Length - 1];
        }

        IEnumerator FadePlayer(PlayerController player, float dur)
        {
            var renderers = player.GetComponentsInChildren<SpriteRenderer>();
            float t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float a = 1f - Mathf.Clamp01(t / dur);
                foreach (var r in renderers)
                {
                    if (r == null) continue;
                    Color c = r.color; c.a = a; r.color = c;
                }
                yield return null;
            }
            foreach (var r in renderers) if (r != null) r.gameObject.SetActive(false);
        }

        static Sprite[] Reversed(Sprite[] src)
        {
            if (src == null) return null;
            var r = (Sprite[])src.Clone();
            System.Array.Reverse(r);
            return r;
        }
    }
}
