using UnityEngine;

namespace EchoShift
{
    /// <summary>
    /// Invisible trigger placed below a level: any player that falls into it is respawned at
    /// the last checkpoint (via GameManager). Echo clones are ignored. Needs a GameManager with
    /// a checkpoint set in the scene; otherwise it is a no-op.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class KillZone : MonoBehaviour
    {
        void Reset()
        {
            var c = GetComponent<Collider2D>();
            if (c != null) c.isTrigger = true;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerController>() == null) return;
            if (GameManager.Instance != null) GameManager.Instance.RespawnPlayer();
        }
    }
}
