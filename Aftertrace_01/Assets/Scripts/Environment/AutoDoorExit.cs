using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoShift
{
    /// <summary>
    /// Proximity door that opens automatically when the player approaches, then fades to
    /// the target scene. Used at the end of Level 0's corridor.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class AutoDoorExit : MonoBehaviour
    {
        public Transform doorBody;
        public AudioSource audioSource;
        public AudioClip slideClip;
        public string targetScene = "Level_01";
        public float openDistance = 3f;
        public float openTime = 0.4f;
        public float delayBeforeLoad = 0.9f;

        bool fired;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (fired) return;
            if (other.GetComponentInParent<PlayerController>() == null) return;
            fired = true;
            StartCoroutine(OpenAndLoad());
        }

        IEnumerator OpenAndLoad()
        {
            if (audioSource != null && slideClip != null) audioSource.PlayOneShot(slideClip);
            if (doorBody != null)
            {
                Vector3 start = doorBody.localPosition;
                Vector3 end = start + Vector3.up * openDistance;
                float t = 0f;
                while (t < openTime)
                {
                    t += Time.deltaTime;
                    doorBody.localPosition = Vector3.Lerp(start, end, Mathf.SmoothStep(0f, 1f, t / openTime));
                    yield return null;
                }
                doorBody.localPosition = end;
            }
            yield return new WaitForSeconds(delayBeforeLoad);
            if (SceneFader.Instance != null) SceneFader.Instance.FadeToScene(targetScene);
            else SceneManager.LoadScene(targetScene);
        }
    }
}
