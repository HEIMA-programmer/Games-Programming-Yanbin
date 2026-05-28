using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EchoShift.EditorTools
{
    /// <summary>
    /// Switches imported pixel-art spritesheets to Multiple mode and slices them on
    /// a fixed grid. Sprite names follow "{baseName}_{index}" (row-major, top-left = 0)
    /// so EchoBuildUtils.LoadSprite can address any frame.
    /// Idempotent — already-sliced sheets just get their grid rewritten.
    /// </summary>
    public static class EchoSpriteSlicer
    {
        // (path, cell width, cell height) — every imported sheet wired up in EchoBuildUtils.LoadSprite.
        static readonly (string path, int cellW, int cellH)[] Sheets = new[]
        {
            ("Assets/Art/Imported/CraftPix1Bit/Main_Characters/Char_Robot.png", 48, 48),
            ("Assets/Art/Imported/CraftPix1Bit/Main_Characters/Char_Boy.png",   48, 48),
            ("Assets/Art/Imported/CraftPix1Bit/Main_Characters/Char_Girl.png",  48, 48),
            ("Assets/Art/Imported/CraftPix1Bit/Main_Characters/Char_fire.png",  32, 32),
            ("Assets/Art/Imported/CraftPix1Bit/Enemies/Alien1.png",             48, 48),
            ("Assets/Art/Imported/CraftPix1Bit/Enemies/Alien2.png",             32, 32),
            ("Assets/Art/Imported/CraftPix1Bit/Tileset/Tileset.png",            16, 16),
            ("Assets/Art/Imported/CraftPix1Bit/Tileset/Tileset_Borders.png",    16, 16),
            ("Assets/Art/Imported/CraftPix1Bit/Tileset/Tileset_details.png",    16, 16),
            // 8 distinct 80x80 decoration variants (planets, circuits, stars, etc).
            ("Assets/Art/Imported/CraftPix1Bit/Tileset/Background_n_details.png", 80, 80),
            ("Assets/Art/Imported/CraftPix1Bit/Objects/Door.png",               48, 48),
            ("Assets/Art/Imported/CraftPix1Bit/Objects/Items.png",              16, 16),
            ("Assets/Art/Imported/CraftPix1Bit/Objects/checkpoint.png",         48, 48),
            ("Assets/Art/Imported/CraftPix1Bit/Objects/Boxes.png",              32, 32),
            ("Assets/Art/Imported/CraftPix1Bit/Objects/Cups.png",               16, 16),
            ("Assets/Art/Imported/CraftPix1Bit/GUI/Icons.png",                  16, 16),
            ("Assets/Art/Imported/CraftPix1Bit/GUI/GUI_Elements.png",           16, 16),
            ("Assets/Art/Imported/CraftPix1Bit/GUI/Tileset_GUI.png",            16, 16),
            // Architectural props — crusher / dome machines (48×48 grid).
            ("Assets/Art/Imported/CraftPix1Bit/Traps/Trap6.png",                48, 48),
        };

        // Single-mode (un-sliced) sheets that still need FullRect mesh for Tiled drawMode.
        static readonly string[] SingleSheets = new string[0];

        [MenuItem("Aftertrace/Slice Imported Spritesheets", false, 30)]
        public static void SliceAll()
        {
            int ok = 0, skipped = 0;
            foreach (var s in Sheets)
            {
                if (Slice(s.path, s.cellW, s.cellH)) ok++; else skipped++;
            }
            foreach (var p in SingleSheets) ReimportSingleSheet(p);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[EchoSpriteSlicer] Sliced {ok}/{Sheets.Length} sheets ({skipped} skipped); {SingleSheets.Length} single sheets reimported.");
        }

        static bool Slice(string path, int cellW, int cellH)
        {
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) { Debug.LogWarning($"[EchoSpriteSlicer] No importer: {path}"); return false; }

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex == null) { Debug.LogWarning($"[EchoSpriteSlicer] No texture: {path}"); return false; }

            int cols = tex.width / cellW;
            int rows = tex.height / cellH;
            if (cols <= 0 || rows <= 0)
            {
                Debug.LogWarning($"[EchoSpriteSlicer] Bad grid {tex.width}x{tex.height} / {cellW}x{cellH}: {path}");
                return false;
            }

            string baseName = Path.GetFileNameWithoutExtension(path);
            var sheet = new List<SpriteMetaData>(cols * rows);

            // Index 0 = top-left frame; Unity's texture y-origin is bottom-left so we flip.
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    int idx = row * cols + col;
                    int y = tex.height - (row + 1) * cellH;
                    sheet.Add(new SpriteMetaData
                    {
                        name = $"{baseName}_{idx}",
                        rect = new Rect(col * cellW, y, cellW, cellH),
                        alignment = (int)SpriteAlignment.Center,
                        pivot = new Vector2(0.5f, 0.5f)
                    });
                }
            }

            // Force FullRect mesh on the sheet itself — postprocessor only fires on first
            // import, but Tiled draw mode silently falls back to stretching if any sliced
            // sprite was generated with Tight mesh from an earlier import.
            var settings = new TextureImporterSettings();
            ti.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            ti.SetTextureSettings(settings);

            ti.spriteImportMode = SpriteImportMode.Multiple;
            #pragma warning disable 0618 // TextureImporter.spritesheet is "obsolete" but no stable replacement.
            ti.spritesheet = sheet.ToArray();
            #pragma warning restore 0618
            EditorUtility.SetDirty(ti);
            ti.SaveAndReimport();
            return true;
        }

        // Force-reimport sheets that aren't sliced (Single mode) so the postprocessor's
        // FullRect mesh takes effect on assets that pre-date EchoImportedAssetSettings.
        public static void ReimportSingleSheet(string path)
        {
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) return;
            var settings = new TextureImporterSettings();
            ti.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            ti.SetTextureSettings(settings);
            ti.SaveAndReimport();
        }
    }
}
