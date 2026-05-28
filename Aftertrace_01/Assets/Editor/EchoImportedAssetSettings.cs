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

        // Per-asset PPU so each sprite lands at roughly 1 world unit at its native scale.
        static float PpuFor(string path)
        {
            // Object/character sheets with 16×16 frames (icons, items, cups).
            if (path.Contains("/CraftPix1Bit/Objects/Items.png")) return 16f;
            if (path.Contains("/CraftPix1Bit/Objects/Cups.png"))  return 16f;
            // 32×32 frames.
            if (path.Contains("/CraftPix1Bit/Objects/Boxes.png")) return 32f;
            if (path.Contains("/CraftPix1Bit/Enemies/Alien2.png")) return 32f;
            // 80×80 decoration tiles.
            if (path.Contains("/CraftPix1Bit/Tileset/Background_n_details.png")) return 80f;
            // Tileset_GUI frames at 16 — used as menu / pause / how-to-play panels.
            if (path.Contains("/CraftPix1Bit/GUI/Tileset_GUI.png")) return 16f;
            // 48 px cells for character / enemy / trap / object spritesheets.
            if (path.Contains("/CraftPix1Bit/Main_Characters/")) return 48f;
            if (path.Contains("/CraftPix1Bit/Enemies/"))         return 48f;
            if (path.Contains("/CraftPix1Bit/Traps/"))           return 48f;
            if (path.Contains("/CraftPix1Bit/Objects/"))         return 48f;
            // 16 px tile cells.
            if (path.Contains("/CraftPix1Bit/Tileset/"))         return 16f;
            if (path.Contains("/CraftPix1Bit/GUI/"))             return 16f;
            return 32f; // Kenney UI + anything else
        }
    }
}
