using System.Collections;
using UnityEngine;

namespace EchoShift
{
    /// <summary>
    /// Persistent BGM manager (DontDestroyOnLoad). Crossfades between background tracks
    /// using two AudioSources. SFX are NOT routed here — they play on per-object
    /// AudioSources. Lives on the App prefab loaded by AppBootstrap.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Background tracks (assigned on the App prefab)")]
        public AudioClip menuBgm;
        public AudioClip levelBgm;
        public AudioClip victoryBgm;

        [Range(0f, 1f)] public float musicVolume = 0.55f;

        AudioSource a;
        AudioSource b;
        AudioSource active;
        Coroutine fade;

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
        public void PlayVictoryMusic() => PlayMusic(victoryBgm);

        public void PlayMusic(AudioClip clip, float fadeTime = 1.2f)
        {
            if (clip == null) return;
            if (active != null && active.clip == clip && active.isPlaying) return;
            if (fade != null) StopCoroutine(fade);
            fade = StartCoroutine(CrossFade(clip, fadeTime));
        }

        public void StopMusic(float fadeTime = 1f)
        {
            if (fade != null) StopCoroutine(fade);
            fade = StartCoroutine(FadeOutAll(fadeTime));
        }

        IEnumerator CrossFade(AudioClip clip, float fadeTime)
        {
            AudioSource from = active;
            AudioSource to = (active == a) ? b : a;
            to.clip = clip;
            to.volume = 0f;
            to.Play();
            active = to;

            float t = 0f;
            float fromStart = from != null ? from.volume : 0f;
            while (t < fadeTime)
            {
                t += Time.unscaledDeltaTime;
                float n = fadeTime <= 0f ? 1f : t / fadeTime;
                to.volume = Mathf.Lerp(0f, musicVolume, n);
                if (from != null) from.volume = Mathf.Lerp(fromStart, 0f, n);
                yield return null;
            }
            to.volume = musicVolume;
            if (from != null) { from.Stop(); from.volume = 0f; }
            fade = null;
        }

        IEnumerator FadeOutAll(float fadeTime)
        {
            float t = 0f;
            float va = a.volume, vb = b.volume;
            while (t < fadeTime)
            {
                t += Time.unscaledDeltaTime;
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
