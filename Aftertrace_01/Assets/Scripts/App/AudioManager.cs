using System.Collections;
using UnityEngine;

namespace EchoShift
{
    /// <summary>
    /// Persistent BGM manager (DontDestroyOnLoad). Moves between background tracks
    /// through silence using two AudioSources. SFX are NOT routed here - they play on
    /// per-object AudioSources. Lives on the App prefab loaded by AppBootstrap.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Background tracks (assigned on the App prefab)")]
        public AudioClip menuBgm;
        public AudioClip levelBgm;

        [Range(0f, 1f)] public float musicVolume = 0.55f;

        [Header("Transition feel")]
        [Tooltip("Track changes dip THROUGH SILENCE (out -> breath -> slow bloom) instead of crossfading - overlapping two unrelated keys reads as mush, and the silent beat lines up with the scene fader's black.")]
        public float fadeOutTime = 2.5f;
        public float silenceGap = 0.35f;
        public float fadeInTime = 2.5f;
        [Tooltip("Drift-away fade begun by SceneFader the moment a transition starts, so the old track sinks WITH the black instead of getting cut after the new scene pops.")]
        public float driftOutTime = 4.5f;

        // Perceptual curves: loudness is logarithmic, so a linear amplitude fade sounds
        // like a knob-turn (hangs loud, then dies suddenly). Squared fade-out drops early
        // and leaves a long whisper tail = "drifting away"; squared fade-in starts as a
        // whisper and swells late = "emerging".
        static float OutCurve(float n) => (1f - n) * (1f - n);
        static float InCurve(float n) => n * n;

        // Scene-load hitch frames carry hundreds of ms in one unscaledDeltaTime - an
        // unclamped fade loop swallows half its curve in one audible jump (the exact
        // "music cuts off" complaint). Clamping guarantees every fade is actually heard.
        static float Dt => Mathf.Min(Time.unscaledDeltaTime, 1f / 30f);

        AudioSource a;
        AudioSource b;
        AudioSource active;
        Coroutine fade;
        float currentScale = 1f;
        float Target => musicVolume * currentScale;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            a = NewSource();
            b = NewSource();
            active = a;
        }

        AudioSource NewSource()
        {
            var s = gameObject.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.loop = true;
            s.volume = 0f;
            s.spatialBlend = 0f;
            s.ignoreListenerPause = true;
            return s;
        }

        public void PlayMenuMusic() => PlayMusic(menuBgm);
        public void PlayLevelMusic() => PlayMusic(levelBgm);

        // volumeScale: per-track loudness trim (sourced tracks aren't mastered to one
        // level - a music box pierces, an ambient pad whispers). Final = musicVolume * scale.
        // fadeTime <= 0 means "use the manager's fadeInTime default".
        public void PlayMusic(AudioClip clip, float fadeTime = -1f, float volumeScale = 1f)
        {
            if (clip == null) return;
            if (active != null && active.clip == clip && active.isPlaying)
            {
                // same track requested (e.g. it was drifting out for a transition into a
                // scene that wants it too): cancel the drift and swell back up instead
                if (fade != null) StopCoroutine(fade);
                currentScale = Mathf.Clamp01(volumeScale);
                fade = StartCoroutine(RampActiveTo(Target, fadeTime > 0f ? fadeTime : 1f));
                return;
            }
            if (fade != null) StopCoroutine(fade);
            currentScale = Mathf.Clamp01(volumeScale);
            fade = StartCoroutine(Transition(clip, fadeTime > 0f ? fadeTime : fadeInTime));
        }

        public void StopMusic(float fadeTime = 1f)
        {
            if (fade != null) StopCoroutine(fade);
            fade = StartCoroutine(FadeOutAll(fadeTime));
        }

        /// <summary>
        /// Begin sinking the current track toward silence WITHOUT scheduling a new one.
        /// SceneFader calls this the moment a transition starts, so the music drifts away
        /// with the black. Whatever volume remains when the next scene's SceneMusic asks
        /// for its track is finished off gently by Transition's own fade-out.
        /// </summary>
        public void DriftOut(float time = -1f)
        {
            // Kill any in-flight transition FIRST: during its silence gap `active` is
            // still the stopped old source, and bailing out before stopping the handle
            // would let the pending bloom fire across the scene change - a ghost of the
            // PREVIOUS scene's track swelling up in the next one.
            if (fade != null) { StopCoroutine(fade); fade = null; }
            if (active == null || !active.isPlaying) return;
            fade = StartCoroutine(DriftRoutine(time > 0f ? time : driftOutTime));
        }

        IEnumerator DriftRoutine(float duration)
        {
            AudioSource src = active;
            float start = src.volume;
            float t = 0f;
            while (t < duration && src != null)
            {
                t += Dt;
                src.volume = start * OutCurve(Mathf.Clamp01(t / duration));
                yield return null;
            }
            if (src != null) { src.Stop(); src.volume = 0f; }
            fade = null;
        }

        IEnumerator RampActiveTo(float target, float duration)
        {
            AudioSource src = active;
            float start = src.volume;
            float t = 0f;
            while (t < duration && src != null)
            {
                t += Dt;
                src.volume = Mathf.Lerp(start, target, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / duration)));
                yield return null;
            }
            if (src != null) src.volume = target;
            fade = null;
        }

        IEnumerator Transition(AudioClip clip, float inTime)
        {
            AudioSource from = active;
            AudioSource to = (active == a) ? b : a;

            // 1. let the old track go first - a clean breath of silence over the
            //    scene fader's black, never two keys sounding at once. DriftOut has
            //    usually lowered it already; this finishes the tail from wherever it is.
            if (from != null && from.isPlaying && from.volume > 0.001f)
            {
                float t0 = 0f;
                float start = from.volume;
                while (t0 < fadeOutTime)
                {
                    t0 += Dt;
                    from.volume = start * OutCurve(fadeOutTime <= 0f ? 1f : Mathf.Clamp01(t0 / fadeOutTime));
                    yield return null;
                }
                from.Stop();
                from.volume = 0f;
                float g = 0f;
                while (g < silenceGap) { g += Dt; yield return null; }
            }

            // 2. bloom the new track in slowly - it emerges rather than starts
            to.clip = clip;
            to.volume = 0f;
            to.Play();
            active = to;
            float t = 0f;
            while (t < inTime)
            {
                t += Dt;
                to.volume = Target * InCurve(inTime <= 0f ? 1f : Mathf.Clamp01(t / inTime));
                yield return null;
            }
            to.volume = Target;
            fade = null;
        }

        IEnumerator FadeOutAll(float fadeTime)
        {
            float t = 0f;
            float va = a.volume, vb = b.volume;
            while (t < fadeTime)
            {
                t += Dt;
                float n = fadeTime <= 0f ? 1f : t / fadeTime;
                a.volume = Mathf.Lerp(va, 0f, n);
                b.volume = Mathf.Lerp(vb, 0f, n);
                yield return null;
            }
            a.Stop(); b.Stop();
            fade = null;
        }
    }
}
