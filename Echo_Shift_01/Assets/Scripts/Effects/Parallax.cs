using UnityEngine;

namespace EchoShift
{
    /// <summary>
    /// Scrolls a background layer at a fraction of camera movement to fake depth.
    /// factor 0 = locked to world, 1 = locked to camera.
    /// </summary>
    public class Parallax : MonoBehaviour
    {
        public Transform cameraTransform;
        [Range(0f, 1f)] public float factor = 0.5f;
        public bool vertical = false;

        Vector3 startSelf;
        Vector3 startCam;

        void Start()
        {
            if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
            startSelf = transform.position;
            if (cameraTransform != null) startCam = cameraTransform.position;
        }

        void LateUpdate()
        {
            if (cameraTransform == null) return;
            Vector3 d = cameraTransform.position - startCam;
            float x = startSelf.x + d.x * factor;
            float y = vertical ? startSelf.y + d.y * factor : startSelf.y;
            transform.position = new Vector3(x, y, transform.position.z);
        }
    }
}
