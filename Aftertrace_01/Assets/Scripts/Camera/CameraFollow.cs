using UnityEngine;
// URP's Pixel Perfect Camera. Aliased because UnityEngine.U2D also ships a same-named
// (built-in RP) component; this project's 2D Renderer uses the URP one (see
// Assets/Editor/EchoPixelPerfectCamera.cs, which adds + configures it per scene).
using PPC = UnityEngine.Experimental.Rendering.Universal.PixelPerfectCamera;

namespace EchoShift
{
    /// <summary>
    /// Smooth, lag-based camera follow. Pulls back while an echo clone is active so both
    /// bodies stay visible, and adds CameraShake offset on top.
    ///
    /// Who owns the zoom (orthographicSize) depends on whether a URP Pixel Perfect Camera
    /// is on the same camera:
    ///  • No PPC — this component owns orthographicSize and eases it toward
    ///    baseOrthoSize (+ cloneOrthoBoost) for a smooth pull-back.
    ///  • PPC present — the PPC drives orthographicSize during rendering (after LateUpdate)
    ///    to keep the 1-Bit art on whole pixels, so writing it here would just be overwritten
    ///    and the clone pull-back would die. Instead we shift the PPC's *reference resolution*:
    ///    a larger reference => lower integer zoom => more world on screen. That stays
    ///    pixel-perfect (no fractional scaling / edge shimmer). It snaps by whole zoom steps
    ///    rather than easing, because a smoothly varying pixel-perfect zoom isn't possible.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(2.5f, 1.2f, -10f);
        public float smoothTime = 0.18f;
        public float minY = -1.5f;
        // Horizontal clamp so the view never leaves the dressed part of the level
        // (set per scene; defaults follow freely).
        public float minX = float.NegativeInfinity;
        public float maxX = float.PositiveInfinity;

        [Header("Clone pull-back — when NO Pixel Perfect Camera")]
        public float baseOrthoSize = 6.5f;
        public float cloneOrthoBoost = 1.3f;
        public float orthoLerp = 2.5f;

        [Header("Clone pull-back — when a Pixel Perfect Camera is present")]
        [Tooltip("Whole pixel-perfect zoom steps to pull back while an echo clone is active. " +
                 "1 = the gentlest crisp step the display allows; 0 disables the PPC pull-back. " +
                 "The step is discrete (snaps) and its size depends on the window resolution.")]
        public int clonePixelZoomOutSteps = 1;

        Camera cam;
        CameraShake shake;
        PPC ppc;
        Vector3 vel;
        Vector3 basePos;
        bool inited;
        int baseRefX, baseRefY;

        void Awake()
        {
            cam = GetComponent<Camera>();
            shake = GetComponent<CameraShake>();
            ppc = GetComponent<PPC>();
            cam.orthographic = true;

            if (ppc != null)
            {
                // Remember the authored reference resolution so we can restore it after a pull-back.
                baseRefX = ppc.refResolutionX;
                baseRefY = ppc.refResolutionY;
            }
            else
            {
                cam.orthographicSize = baseOrthoSize;
            }
        }

        void LateUpdate()
        {
            if (target == null) return;

            Vector3 desired = target.position + offset;
            if (desired.y < minY) desired.y = minY;
            desired.x = Mathf.Clamp(desired.x, minX, maxX);
            desired.z = offset.z;

            if (!inited) { basePos = desired; inited = true; }
            basePos = Vector3.SmoothDamp(basePos, desired, ref vel, smoothTime);

            Vector3 pos = basePos;
            if (shake != null) pos += shake.Offset;
            transform.position = pos;

            bool cloneActive = GameManager.Instance != null && GameManager.Instance.IsCloneActive;
            ApplyCloneZoom(cloneActive);
        }

        // Pull the camera back while a clone is live so both bodies fit on screen.
        void ApplyCloneZoom(bool cloneActive)
        {
            if (ppc == null)
            {
                // We own the size: ease toward the (optionally boosted) base size.
                float targetSize = baseOrthoSize + (cloneActive ? cloneOrthoBoost : 0f);
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize,
                    1f - Mathf.Exp(-orthoLerp * Time.deltaTime));
                return;
            }

            // The PPC owns the size. Shift the integer zoom by raising the reference resolution
            // while a clone is active, then restore it. Everything stays on whole pixels.
            // PPC zoom math (PixelSnapping, CropFrame.None):
            //   zoom = max(1, min(screenW / refX, screenH / refY))   [integer]
            //   orthoSize = screenH / (2 * zoom * assetsPPU)
            // so a lower zoom = a larger orthoSize = more world visible.
            int sw = Mathf.Max(1, Screen.width);
            int sh = Mathf.Max(1, Screen.height);
            int baseZoom = Mathf.Max(1, Mathf.Min(sw / baseRefX, sh / baseRefY));
            int targetZoom = Mathf.Max(1, baseZoom - Mathf.Max(0, clonePixelZoomOutSteps));

            int refX = baseRefX, refY = baseRefY;
            if (cloneActive && targetZoom < baseZoom)
            {
                // Reference resolution whose integer zoom at this window size is targetZoom.
                // (If the window is too small to spare a whole zoom step, targetZoom == baseZoom
                //  above and we simply stay at the base reference — still crisp, just no pull-back.)
                refX = Mathf.Max(baseRefX, sw / targetZoom);
                refY = Mathf.Max(baseRefY, sh / targetZoom);
            }

            if (ppc.refResolutionX != refX) ppc.refResolutionX = refX;
            if (ppc.refResolutionY != refY) ppc.refResolutionY = refY;
        }

        // Never leave the PPC stuck at a boosted reference resolution (e.g. disabled mid-pull-back).
        void OnDisable()
        {
            if (ppc != null)
            {
                if (baseRefX > 0) ppc.refResolutionX = baseRefX;
                if (baseRefY > 0) ppc.refResolutionY = baseRefY;
            }
        }
    }
}
