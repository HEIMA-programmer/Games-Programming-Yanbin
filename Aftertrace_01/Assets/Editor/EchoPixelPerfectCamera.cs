using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
// Alias the URP component explicitly. The 2D feature set ALSO ships a standalone
// UnityEngine.U2D.PixelPerfectCamera (built-in RP); under URP that one misbehaves.
// This project's pipeline uses a 2D Renderer (Assets/Settings/Renderer2D.asset), so the
// URP-bundled component below is the correct one.
using PPC = UnityEngine.Experimental.Rendering.Universal.PixelPerfectCamera;

namespace EchoShift.EditorTools
{
    /// <summary>
    /// Adds + configures URP's Pixel Perfect Camera on the active scene's main camera so the
    /// 1-Bit white-on-black art renders at whole pixels (otherwise the edges shimmer in motion).
    /// This is a SETUP tool — it never places or generates art. Run once per scene.
    /// </summary>
    public static class EchoPixelPerfectCamera
    {
        // 640x360 base: integer-scales x2 -> 720p, x3 -> 1080p. At PPU 32 this drives the
        // orthographic size to ~5.625, close to the levels' current 5.4-6.5 so the on-screen
        // zoom barely shifts. Bump to 480x270 for a chunkier (more zoomed-in) retro look.
        const int Ppu = 32;
        const int RefX = 640;
        const int RefY = 360;

        [MenuItem("Aftertrace/Art/Configure Pixel Perfect Camera", false, 100)]
        public static void Configure()
        {
            Camera cam = Camera.main != null ? Camera.main : Object.FindObjectOfType<Camera>();
            if (cam == null)
            {
                EditorUtility.DisplayDialog("Pixel Perfect Camera",
                    "No Camera found in the active scene. Open a level scene first, then re-run.", "OK");
                return;
            }

            cam.orthographic = true;

            var ppc = cam.GetComponent<PPC>();
            if (ppc == null) ppc = Undo.AddComponent<PPC>(cam.gameObject);

            ppc.assetsPPU = Ppu;
            ppc.refResolutionX = RefX;
            ppc.refResolutionY = RefY;
            ppc.gridSnapping = PPC.GridSnapping.PixelSnapping; // crisp; NOT UpscaleRenderTexture (fights parallax/bloom)
            ppc.cropFrame = PPC.CropFrame.None;                // fill the window during dev; switch to Windowbox for hard bars

            EditorUtility.SetDirty(cam.gameObject);
            EditorSceneManager.MarkSceneDirty(cam.gameObject.scene);
            Debug.Log($"[Aftertrace/Art] Pixel Perfect Camera configured on '{cam.name}': " +
                      $"PPU {Ppu}, Ref {RefX}x{RefY}, PixelSnapping, CropFrame None. " +
                      "The component now drives orthographic size (~5.6). SAVE the scene to keep it.");
        }
    }
}
