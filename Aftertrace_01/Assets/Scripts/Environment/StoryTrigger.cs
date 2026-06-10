using UnityEngine;

namespace EchoShift
{
    /// <summary>
    /// One-shot story/hint trigger. Echo clones never fire it; only the player does.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class StoryTrigger : MonoBehaviour
    {
        public string beatId = "";
        public string title = "";
        public string speaker = "TRACE";
        [TextArea] public string[] lines;
        public NarrativeBlockingMode blockingMode = NarrativeBlockingMode.Passive;
        public bool blocking = false; // Backward-compatible inspector field.
        public bool oneShot = true;
        public Sprite memoryImage;
        public Sprite portrait;   // optional speaker head; terminal falls back to its default

        bool fired;

        void Reset()
        {
            var c = GetComponent<Collider2D>();
            if (c != null) c.isTrigger = true;
        }

        void OnValidate()
        {
            if (blocking && blockingMode == NarrativeBlockingMode.Passive)
                blockingMode = NarrativeBlockingMode.Blocking;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (fired && oneShot) return;
            if (other.GetComponentInParent<PlayerController>() == null) return;
            if (NarrativeTerminal.Instance == null) return;

            fired = true;
            NarrativeBlockingMode mode = blocking ? NarrativeBlockingMode.Blocking : blockingMode;
            NarrativeTerminal.Instance.Play(lines, mode, title, speaker, memoryImage, null, portrait);
        }
    }
}
