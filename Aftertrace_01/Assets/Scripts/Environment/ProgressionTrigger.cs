using UnityEngine;

namespace EchoShift
{
    /// <summary>
    /// Closes and locks a door once the player passes through it, giving one-way
    /// progression between areas.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ProgressionTrigger : MonoBehaviour
    {
        public Door doorToClose;

        bool fired;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (fired) return;
            if (other.GetComponentInParent<PlayerController>() == null) return;

            fired = true;
            if (doorToClose != null) doorToClose.LockClosed();
        }
    }
}
