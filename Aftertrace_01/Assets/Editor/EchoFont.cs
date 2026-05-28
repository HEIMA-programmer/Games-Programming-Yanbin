using System.Text;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace EchoShift.EditorTools
{
    /// <summary>
    /// Wires the project's UI fonts:
    ///   • Body — Exo 2 (OFL) → <see cref="EchoBuildUtils.CustomFont"/>
    ///   • Title — Orbitron (OFL) → <see cref="EchoBuildUtils.TitleFont"/>
    ///
    /// For each TTF in Assets/Fonts/ matching a known base name, Build All loads — or, best-effort,
    /// generates — a TMP_FontAsset and assigns it. Generated assets are forced to <b>Static</b>
    /// atlas mode with the glyphs the game uses pre-baked: a Dynamic font asset rewrites its own
    /// atlas as glyphs are rendered, which shows up as a perpetually "modified" .asset in git.
    /// Static = the atlas is fixed, so opening or playing never edits the file.
    ///
    /// If a font is missing or generation fails, the corresponding pointer stays null and the
    /// callers fall back gracefully (TitleFont → CustomFont → TMP default Liberation Sans).
    /// </summary>
    public static class EchoFont
    {
        public const string FontDir = "Assets/Fonts";

        public static void EnsureFontAsset()
        {
            EchoBuildUtils.CustomFont = null;
            EchoBuildUtils.TitleFont = null;
            EchoBuildUtils.EnsureFolder(FontDir);

            EchoBuildUtils.CustomFont = LoadOrBake("Exo2");        // body — used by CreateText
            // VT323 is a retro pixel-terminal display font (OFL) — fits 1-Bit kit perfectly.
            // Falls back to Orbitron then TMP default if VT323 isn't present.
            EchoBuildUtils.TitleFont  = LoadOrBake("VT323") ?? LoadOrBake("Orbitron");
        }

        // Finds (or bakes once) a Static TMP_FontAsset whose source TTF filename contains nameHint.
        static TMP_FontAsset LoadOrBake(string nameHint)
        {
            // 1) An existing Static asset matching the hint — reuse (no regeneration → no git churn).
            foreach (string guid in AssetDatabase.FindAssets("t:TMP_FontAsset", new[] { FontDir }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.Contains(nameHint)) continue;
                var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
                if (existing != null && existing.atlasPopulationMode == AtlasPopulationMode.Static) return existing;
            }

            // 2) Find a source TTF whose name contains the hint.
            string ttfPath = null;
            foreach (string guid in AssetDatabase.FindAssets("t:Font", new[] { FontDir }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.Contains(nameHint)) continue;
                ttfPath = path;
                break;
            }
            if (ttfPath == null) return null;

            var font = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);
            if (font == null) return null;

            try
            {
                var fa = TMP_FontAsset.CreateFontAsset(font); // dynamic atlas to start
                if (fa == null) return null;

                // Pre-bake every glyph the UI uses, then freeze.
                var chars = new StringBuilder();
                for (int c = 32; c <= 126; c++) chars.Append((char)c);    // printable ASCII
                for (int c = 160; c <= 255; c++) chars.Append((char)c);   // Latin-1
                chars.Append("←→↑↓—–…•“”‘’");                              // arrows, dashes, ellipsis, quotes
                fa.TryAddCharacters(chars.ToString());
                fa.atlasPopulationMode = AtlasPopulationMode.Static;

                string assetPath = $"{FontDir}/{Path.GetFileNameWithoutExtension(ttfPath)} SDF.asset";
                AssetDatabase.DeleteAsset(assetPath);
                AssetDatabase.CreateAsset(fa, assetPath);

                if (fa.material != null)
                {
                    fa.material.name = font.name + " Material";
                    AssetDatabase.AddObjectToAsset(fa.material, fa);
                }
                if (fa.atlasTextures != null)
                    foreach (var tex in fa.atlasTextures)
                        if (tex != null) AssetDatabase.AddObjectToAsset(tex, fa);

                fa.ReadFontAssetDefinition();
                EditorUtility.SetDirty(fa);
                AssetDatabase.SaveAssets();

                var baked = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
                Debug.Log($"[EchoFont] Baked static TMP font from {ttfPath}.");
                return baked;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[EchoFont] Auto-generating TMP font for {nameHint} failed — open " +
                    "Window ▸ TextMeshPro ▸ Font Asset Creator, generate manually, save into " +
                    $"Assets/Fonts/, then re-run Build All. ({e.Message})");
                return null;
            }
        }
    }
}
