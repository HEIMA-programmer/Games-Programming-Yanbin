using System.Text;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace EchoShift.EditorTools
{
    /// <summary>
    /// Wires the project's custom UI font (Exo 2, OFL). Drop the font's .ttf into Assets/Fonts/.
    /// Build All loads — or, best-effort, generates — a TMP_FontAsset from it and assigns it to
    /// <see cref="EchoBuildUtils.CustomFont"/>, so every CreateText / CreateWallNarrative uses it.
    ///
    /// The generated asset is forced to <b>Static</b> atlas mode with the glyphs the game uses
    /// pre-baked: a Dynamic font asset rewrites its own atlas as glyphs are rendered at runtime,
    /// which shows up as a perpetually "modified" .asset in git. Static = the atlas is fixed, so
    /// opening or playing the game never edits the file.
    ///
    /// If no font is present (or generation fails), CustomFont stays null and TMP's default
    /// (Liberation Sans) is used — the build never breaks.
    /// </summary>
    public static class EchoFont
    {
        public const string FontDir = "Assets/Fonts";

        public static void EnsureFontAsset()
        {
            EchoBuildUtils.CustomFont = null;
            EchoBuildUtils.EnsureFolder(FontDir);   // give the user's .ttf a home

            // Find any existing TMP_FontAsset already in Assets/Fonts/.
            TMP_FontAsset existing = null;
            foreach (string guid in AssetDatabase.FindAssets("t:TMP_FontAsset", new[] { FontDir }))
            {
                existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guid));
                if (existing != null) break;
            }

            // 1) A pre-baked Static asset is already good — reuse it as-is (no regeneration → no churn).
            if (existing != null && existing.atlasPopulationMode == AtlasPopulationMode.Static)
            {
                EchoBuildUtils.CustomFont = existing;
                return;
            }

            // 2) Otherwise (re)generate a Static, pre-baked asset from the first font file found.
            string ttfPath = null;
            foreach (string guid in AssetDatabase.FindAssets("t:Font", new[] { FontDir }))
            {
                ttfPath = AssetDatabase.GUIDToAssetPath(guid);
                break;
            }

            if (ttfPath != null)
            {
                var font = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);
                if (font != null)
                {
                    try
                    {
                        var fa = TMP_FontAsset.CreateFontAsset(font);   // dynamic atlas to start
                        if (fa != null)
                        {
                            // Pre-bake every glyph the UI uses, then freeze the atlas.
                            var chars = new StringBuilder();
                            for (int c = 32; c <= 126; c++) chars.Append((char)c);    // printable ASCII
                            for (int c = 160; c <= 255; c++) chars.Append((char)c);   // Latin-1 (·, ×, accents…)
                            chars.Append("←→↑↓—–…•“”‘’");                              // arrows, dashes, ellipsis, quotes
                            fa.TryAddCharacters(chars.ToString());
                            fa.atlasPopulationMode = AtlasPopulationMode.Static;

                            string assetPath = $"{FontDir}/{Path.GetFileNameWithoutExtension(ttfPath)} SDF.asset";
                            AssetDatabase.DeleteAsset(assetPath);
                            AssetDatabase.CreateAsset(fa, assetPath);

                            // Material + atlas texture must live inside the asset to survive reimport.
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

                            EchoBuildUtils.CustomFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
                            Debug.Log($"[EchoFont] Generated a static TMP font asset from {ttfPath}.");
                            return;
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning("[EchoFont] Auto-generating the TMP font asset failed — generate one once via " +
                            "Window ▸ TextMeshPro ▸ Font Asset Creator (load the .ttf, Generate Font Atlas, save into " +
                            "Assets/Fonts/), then re-run Build All. (" + e.Message + ")");
                    }
                }
            }

            // 3) Fallbacks: use an existing (e.g. dynamic) asset so the font still works; else TMP default.
            if (EchoBuildUtils.CustomFont == null && existing != null) EchoBuildUtils.CustomFont = existing;
            if (EchoBuildUtils.CustomFont == null)
                Debug.Log("[EchoFont] No usable font in Assets/Fonts/ — using TMP default. Drop an Exo 2 .ttf there to enable the custom font.");
        }
    }
}
