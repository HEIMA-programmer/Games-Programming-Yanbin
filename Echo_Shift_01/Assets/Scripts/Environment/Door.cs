using System.Collections;
using UnityEngine;

namespace EchoShift
{
    /// <summary>
    /// Slides its body upward to open. Opens when its required plate(s) are pressed
    /// (AND or OR), or via Open()/Close() for scripted gates. LockClosed() forces it
    /// shut permanently for one-way progression.
    /// </summary>
    public class Door : MonoBehaviour
    {
        public PressurePlate[] requiredPlates;
        public bool requireAll = true;
        public bool latch = false;   // once opened by plates, stay open even if released
        public Transform doorBody;
        public float openDistance = 4.6f;
        public float openTime = 0.3f;
        public AudioSource audioSource;
        public AudioClip slideClip;
        public AudioClip closeClip;

        Vector3 closedPos;
        Vector3 openPos;
        bool isOpen;
        bool locked;
        bool latched;
        Coroutine anim;

        void Awake()
        {
            if (doorBody == null) doorBody = transform;
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
            if (anim != null) StopCoroutine(anim);
            anim = StartCoroutine(Slide(open ? openPos : closedPos));
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
    }
}
