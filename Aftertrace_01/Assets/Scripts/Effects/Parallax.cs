using UnityEngine;

namespace EchoShift
{
    /// <summary>
    /// Scrolls a background layer at a fraction of camera movement to fake depth.
    /// factor 0 = locked to world, 1 = locked to camera (so smaller = further away / slower).
    /// The result is snapped to whole pixels so the layer never breaks the Pixel Perfect
    /// Camera's whole-pixel rendering — un-snapped parallax is the classic source of the
    /// "background shimmers while scrolling" bug on a pixel-perfect setup.
    /// </summary>
    public class Parallax : MonoBehaviour
    {
        public Transform cameraTransform;
        [Range(0f, 1f)] public float factor = 0.5f;
        public bool vertical = false;

        [Tooltip("Snap the layer to whole pixels to match the Pixel Perfect Camera. Keep equal to Assets PPU.")]
        public bool pixelSnap = true;
        public int pixelsPerUnit = 32;

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

            if (pixelSnap && pixelsPerUnit > 0)
            {
                x = Mathf.Round(x * pixelsPerUnit) / pixelsPerUnit;
                y = Mathf.Round(y * pixelsPerUnit) / pixelsPerUnit;
            }
            transform.position = new Vector3(x, y, transform.position.z);
        }
    }
}
