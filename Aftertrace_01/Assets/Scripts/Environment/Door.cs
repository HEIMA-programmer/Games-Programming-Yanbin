using System.Collections;
using UnityEngine;

namespace EchoShift
{
    /// <summary>
    /// Puzzle door. Opens when its required plate(s) are pressed (AND or OR), or via
    /// Open()/Close() for scripted gates. LockClosed() forces it shut permanently for
    /// one-way progression.
    ///
    /// Presentation: if openFrames are assigned it plays an OPEN/CLOSE sprite animation
    /// (frame-based); otherwise it falls back to the legacy slide-up motion. Either way the
    /// gameplay (when it opens/closes, and the blocking collider) is unchanged — only the
    /// visual of going from shut to open differs.
    /// </summary>
    public class Door : MonoBehaviour
    {
        public PressurePlate[] requiredPlates;
        public bool requireAll = true;
        public bool latch = false;   // once opened by plates, stay open even if released
        public Transform doorBody;

        [Header("Open/Close animation (preferred)")]
        [Tooltip("Frames in order CLOSED -> OPEN. If empty, the door uses the legacy slide-up instead.")]
        public Sprite[] openFrames;
        [Tooltip("Frames OPEN -> CLOSED. Empty = openFrames reversed.")]
        public Sprite[] closeFrames;
        public float animTime = 0.3f;

        [Header("Legacy slide (used only when openFrames is empty)")]
        public float openDistance = 4.6f;
        public float openTime = 0.3f;

        [Header("Audio")]
        public AudioSource audioSource;
        public AudioClip slideClip;   // played on OPEN
        public AudioClip closeClip;   // played on CLOSE (falls back to slideClip)

        SpriteRenderer doorRenderer;
        Collider2D bodyCollider;
        Vector3 closedPos;
        Vector3 openPos;
        bool isOpen;
        bool locked;
        bool latched;
        Coroutine anim;

        bool UseFrames => openFrames != null && openFrames.Length > 0;

        void Awake()
        {
            if (doorBody == null) doorBody = transform;
            doorRenderer = doorBody.GetComponent<SpriteRenderer>();
            bodyCollider = doorBody.GetComponent<Collider2D>();
            closedPos = doorBody.localPosition;
            openPos = closedPos + Vector3.up * openDistance;
        }

        void Update()
        {
            if (locked) return;
            if (requiredPlates == null || requiredPlates.Length == 0) return;

            bool cond = requireAll ? AllPressed() : AnyPressed();
            if (cond)
            {
                latched = true;
                SetOpen(true);
            }
            else
            {
                SetOpen(latch && latched);
            }
        }

        bool AllPressed()
        {
            foreach (PressurePlate p in requiredPlates)
                if (p == null || !p.IsPressed) return false;
            return true;
        }

        bool AnyPressed()
        {
            foreach (PressurePlate p in requiredPlates)
                if (p != null && p.IsPressed) return true;
            return false;
        }

        public void Open() => SetOpen(true);
        public void Close() => SetOpen(false);

        public void LockClosed()
        {
            locked = true;
            SetOpen(false);
        }

        void SetOpen(bool open)
        {
            if (open == isOpen) return;
            isOpen = open;
            if (audioSource != null)
            {
                AudioClip clip = open ? slideClip : (closeClip != null ? closeClip : slideClip);
                if (clip != null) audioSource.PlayOneShot(clip);
            }
            // Frame doors don't move the body, so the blocking collider must be toggled here:
            // off the instant it opens (let the player through), on the instant it closes.
            // (Legacy slide doors move the whole body away, so they keep their collider.)
            if (UseFrames && bodyCollider != null)
            {
                if (open) bodyCollider.enabled = false;
                else bodyCollider.enabled = true;
            }
            if (anim != null) StopCoroutine(anim);
            anim = StartCoroutine(UseFrames ? AnimateFrames(open) : Slide(open ? openPos : closedPos));
        }

        // Frame-based open/close. Holds the final frame; gameplay collider is untouched.
        IEnumerator AnimateFrames(bool open)
        {
            Sprite[] frames = open ? openFrames
                            : (closeFrames != null && closeFrames.Length > 0 ? closeFrames : Reversed(openFrames));
            if (doorRenderer == null || frames == null || frames.Length == 0) yield break;

            float t = 0f;
            while (t < animTime)
            {
                t += Time.deltaTime;
                int i = Mathf.Clamp(Mathf.FloorToInt(t / animTime * frames.Length), 0, frames.Length - 1);
                doorRenderer.sprite = frames[i];
                yield return null;
            }
            doorRenderer.sprite = frames[frames.Length - 1];
        }

        IEnumerator Slide(Vector3 target)
        {
            Vector3 start = doorBody.localPosition;
            float t = 0f;
            while (t < openTime)
            {
                t += Time.deltaTime;
                doorBody.localPosition = Vector3.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t / openTime));
                yield return null;
            }
            doorBody.localPosition = target;
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
