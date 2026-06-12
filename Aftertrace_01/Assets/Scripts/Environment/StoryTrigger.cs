using UnityEngine;

namespace EchoShift
{
    /// <summary>
    /// One-shot story/hint trigger. Echo clones never fire it; only the player does.
    /// Can instead fire on the first drone decoy-stun (fireOnDroneStun), for beats
    /// that coach a mechanic at the moment the player first pulls it off.
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
        [Tooltip("Fire the first time any drone is decoy-stunned, instead of on collider entry (the collider is ignored in this mode).")]
        public bool fireOnDroneStun = false;

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

        void OnEnable()
        {
            if (fireOnDroneStun) PatrolDrone.OnStunned += HandleDroneStunned;
        }

        void OnDisable()
        {
            if (fireOnDroneStun) PatrolDrone.OnStunned -= HandleDroneStunned;
        }

        void HandleDroneStunned(PatrolDrone drone) => Fire();

        void OnTriggerEnter2D(Collider2D other)
        {
            if (fireOnDroneStun) return;   // event-driven beat; the collider is inert
            if (other.GetComponentInParent<PlayerController>() == null) return;
            Fire();
        }

        void Fire()
        {
            if (fired && oneShot) return;
            if (NarrativeTerminal.Instance == null) return;

            fired = true;
            NarrativeBlockingMode mode = blocking ? NarrativeBlockingMode.Blocking : blockingMode;
            NarrativeTerminal.Instance.Play(lines, mode, title, speaker, memoryImage, null, portrait);
        }
    }
}
