using UnityEngine;

namespace EchoShift
{
    /// <summary>
    /// Minimal frame-array animator for props (spinning fragments, checkpoint flags,
    /// hovering drones). Loops by default; one-shot holds the last frame.
    /// </summary>
    public class SpriteFrameLooper : MonoBehaviour
    {
        public SpriteRenderer target;
        public Sprite[] frames;
        public float fps = 8f;
        public bool loop = true;
        public bool playOnEnable = true;
        public bool randomStartFrame = false;

        int index;
        float timer;
        bool playing;

        void Awake()
        {
            if (target == null) target = GetComponent<SpriteRenderer>();
        }

        void OnEnable()
        {
            if (playOnEnable) Play();
        }

        public void Play()
        {
            if (target == null || frames == null || frames.Length == 0) return;
            index = randomStartFrame ? Random.Range(0, frames.Length) : 0;
            timer = 0f;
            playing = true;
            target.sprite = frames[index];
        }

        public void Play(Sprite[] newFrames, bool loopIt)
        {
            frames = newFrames;
            loop = loopIt;
            Play();
        }

        public void Stop() => playing = false;

        void Update()
        {
            if (!playing || target == null || frames == null || frames.Length == 0) return;
            timer += Time.deltaTime;
            float step = 1f / Mathf.Max(1f, fps);
            while (timer >= step)
            {
                timer -= step;
                if (loop) index = (index + 1) % frames.Length;
                else if (index < frames.Length - 1) index++;
                else { playing = false; break; }
                target.sprite = frames[index];
            }
        }
    }
}
