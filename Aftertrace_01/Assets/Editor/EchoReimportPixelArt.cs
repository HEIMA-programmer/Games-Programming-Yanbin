using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EchoShift.EditorTools
{
    /// <summary>
    /// Forces every texture under Assets/Art/Imported/ to re-run EchoImportedAssetSettings,
    /// guaranteeing a UNIFORM PPU 32 (+ Point filter, no compression, FullRect mesh) across ALL
    /// imported art — including the boss (Alien3-6), trap (Trap1-5) and Numbers sheets that were
    /// never in the slicer list and still carry stale per-sheet PPU (48 / 16). Without this they
    /// render at a different pixel scale than the terrain and break pixel-perfect alignment.
    /// Pure import hygiene: it slices nothing and places nothing.
    /// </summary>
    public static class EchoReimportPixelArt
    {
        const string Root = "Assets/Art/Imported";

        [MenuItem("Aftertrace/Art/Reimport All Imported Art @ PPU 32", false, 102)]
        public static void ReimportAll()
        {
            if (!AssetDatabase.IsValidFolder(Root))
            {
                EditorUtility.DisplayDialog("Reimport", $"Folder not found: {Root}", "OK");
                return;
            }

            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { Root });
            var paths = new List<string>();
            foreach (var g in guids)
            {
                var p = AssetDatabase.GUIDToAssetPath(g);
                if (p.StartsWith(Root)) paths.Add(p);
            }

            try
            {
                AssetDatabase.StartAssetEditing();
                foreach (var p in paths)
                    AssetDatabase.ImportAsset(p, ImportAssetOptions.ForceUpdate);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }

            Debug.Log($"[Aftertrace/Art] Reimported {paths.Count} textures under {Root} -> PPU 32, Point, " +
                      "Uncompressed, FullRect. Boss/Trap/Number sheets are now PPU 32 as well (still UNSLICED — " +
                      "those get sliced by-image when a level actually needs them, not blind-gridded here).");
        }
    }
}
