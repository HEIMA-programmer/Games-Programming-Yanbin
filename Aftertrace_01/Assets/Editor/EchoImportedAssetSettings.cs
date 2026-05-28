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
        }

        // 48 px per cell for character/enemy/trap/object frames; 16 px for
        // tile and small GUI icons; default 32 (matches existing procedural).
        static float PpuFor(string path)
        {
            if (path.Contains("/CraftPix1Bit/Main_Characters/")) return 48f;
            if (path.Contains("/CraftPix1Bit/Enemies/"))         return 48f;
            if (path.Contains("/CraftPix1Bit/Traps/"))           return 48f;
            if (path.Contains("/CraftPix1Bit/Objects/"))         return 48f;
            if (path.Contains("/CraftPix1Bit/Tileset/"))         return 16f;
            if (path.Contains("/CraftPix1Bit/GUI/"))             return 16f;
            return 32f; // Kenney UI + anything else
        }
    }
}
