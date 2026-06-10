using UnityEngine;

namespace EchoShift
{
    /// <summary>
    /// Declares this scene's background track. On load it hands the clip to the
    /// persistent AudioManager, which crossfades from whatever was playing — if the
    /// previous scene declared the SAME clip, AudioManager's dedupe keeps it running
    /// seamlessly (e.g. the menu lullaby continuing into the opening cutscene).
    /// GameManager skips its generic level-music fallback when one of these is present.
    /// </summary>
    public class SceneMusic : MonoBehaviour
    {
        public AudioClip track;
        [Tooltip("Per-track loudness trim: sourced music isn't mastered to one level.")]
        [Range(0f, 1f)] public float volumeScale = 1f;
        [Tooltip("Fade-in override; 0 = use AudioManager's transition defaults (recommended).")]
        public float fadeTime = 0f;

        void Start()
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayMusic(track, fadeTime, volumeScale);
        }
    }
}
