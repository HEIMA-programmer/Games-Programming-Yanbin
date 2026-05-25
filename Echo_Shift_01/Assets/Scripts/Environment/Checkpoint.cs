using UnityEngine;

namespace EchoShift
{
    /// <summary>
    /// Invisible trigger at the entrance of a puzzle area. When the player enters, it
    /// becomes the active respawn point (Level 2 enemies knock the player back to it).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Checkpoint : MonoBehaviour
    {
        [Tooltip("Where the player respawns. If left at (0,0,0), uses this object's position.")]
        public Vector3 respawnPoint;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerController>() == null) return;
            Vector3 p = (respawnPoint == Vector3.zero) ? transform.position : respawnPoint;
            if (GameManager.Instance != null) GameManager.Instance.SetCheckpoint(p);
        }
    }
}
