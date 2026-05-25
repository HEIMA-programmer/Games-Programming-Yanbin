using UnityEngine;

namespace EchoShift
{
    /// <summary>
    /// Smooth, lag-based camera follow. Pulls back slightly while an echo clone is
    /// active so both bodies stay visible. Adds CameraShake offset on top.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(2.5f, 1.2f, -10f);
        public float smoothTime = 0.18f;
        public float minY = -1.5f;
        public float baseOrthoSize = 6.5f;
        public float cloneOrthoBoost = 1.3f;
        public float orthoLerp = 2.5f;

        Camera cam;
        CameraShake shake;
        Vector3 vel;
        Vector3 basePos;
        bool inited;

        void Awake()
        {
            cam = GetComponent<Camera>();
            shake = GetComponent<CameraShake>();
            cam.orthographic = true;
            cam.orthographicSize = baseOrthoSize;
        }

        void LateUpdate()
        {
            if (target == null) return;

            Vector3 desired = target.position + offset;
            if (desired.y < minY) desired.y = minY;
            desired.z = offset.z;

            if (!inited) { basePos = desired; inited = true; }
            basePos = Vector3.SmoothDamp(basePos, desired, ref vel, smoothTime);

            Vector3 pos = basePos;
            if (shake != null) pos += shake.Offset;
            transform.position = pos;

            bool cloneActive = GameManager.Instance != null && GameManager.Instance.IsCloneActive;
            float targetSize = baseOrthoSize + (cloneActive ? cloneOrthoBoost : 0f);
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, 1f - Mathf.Exp(-orthoLerp * Time.deltaTime));
        }
    }
}
