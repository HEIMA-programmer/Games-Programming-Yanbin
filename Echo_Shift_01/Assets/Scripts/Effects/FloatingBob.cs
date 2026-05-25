using UnityEngine;

namespace EchoShift
{
    /// <summary>Gentle sinusoidal vertical float for collectibles and the R prompt.</summary>
    public class FloatingBob : MonoBehaviour
    {
        public float amplitude = 0.18f;
        public float speed = 2f;
        public float phase = 0f;

        Vector3 start;

        void OnEnable()
        {
            start = transform.localPosition;
        }

        void Update()
        {
            Vector3 p = start;
            p.y += Mathf.Sin((Time.time + phase) * speed) * amplitude;
            transform.localPosition = p;
        }
    }
}
