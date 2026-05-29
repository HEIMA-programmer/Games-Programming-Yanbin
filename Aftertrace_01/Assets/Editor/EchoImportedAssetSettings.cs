using UnityEditor;
using UnityEngine;

namespace EchoShift.EditorTools
{
    /// <summary>
    /// Configures TextureImporter defaults for everything under Assets/Art/Imported/
    /// so external pixel-art (CraftPix 1-Bit, Kenney UI) lands with correct
    /// Point filter, no compression, and a per-folder pixels-per-unit. Sprite
    /// slicing (Single -> Multiple grid) happens in a separate Build All step.
    /// </summary>
    public class EchoImportedAssetSettings : AssetPostprocessor
    {
        const string Root = "Assets/Art/Imported/";

        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(Root)) return;

            var ti = (TextureImporter)assetImporter;
            ti.textureType = TextureImporterType.Sprite;
            ti.filterMode = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.alphaIsTransparency = true;
            ti.mipmapEnabled = false;
            ti.wrapMode = TextureWrapMode.Clamp;
            ti.spritePixelsPerUnit = PpuFor(assetPath);

            // Full Rect (vs Tight) so Image.Type.Tiled / Sliced render without holes.
            var settings = new TextureImporterSettings();
            ti.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            ti.SetTextureSettings(settings);
        }

        // ONE project-wide PPU for ALL imported pixel art. Pixel-art golden rule: a source
        // pixel must render at the SAME on-screen size everywhere, so every sheet shares one
        // PPU and lets world SIZE vary instead — a 48px character spans 1.5u = exactly 3× a
        // 16px tile's 0.5u, which is the kit's intended proportion. The old per-sheet scheme
        // (16/32/48/80, "each sprite ≈ 1 unit") made a single pixel five different on-screen
        // sizes and shattered the unified 1-Bit grid (the root cause of "doesn't match the
        // showcase"). 32 is chosen so characters keep ≈their previous world size, leaving all
        // level geometry and colliders untouched; terrain tiles just become 0.5u and tile
        // denser (closer to the reference's fine dithering).
        static float PpuFor(string path) => 32f;
    }
}
