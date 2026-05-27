using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace EchoShift.EditorTools
{
    /// <summary>
    /// Wires the project's custom UI font (Exo 2, OFL). Drop the font's .ttf into Assets/Fonts/.
    /// Build All then loads — or, best-effort, generates — a TMP_FontAsset from it and assigns it to
    /// <see cref="EchoBuildUtils.CustomFont"/>, so every CreateText / CreateWallNarrative uses it.
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

            // 1) Prefer an existing TMP_FontAsset (e.g. one made once via the Font Asset Creator and committed).
            foreach (string guid in AssetDatabase.FindAssets("t:TMP_FontAsset", new[] { FontDir }))
            {
                var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guid));
                if (existing != null) { EchoBuildUtils.CustomFont = existing; return; }
            }

            // 2) Otherwise best-effort: build a dynamic TMP_FontAsset from the first font file found.
            string ttfPath = null;
            foreach (string guid in AssetDatabase.FindAssets("t:Font", new[] { FontDir }))
            {
                ttfPath = AssetDatabase.GUIDToAssetPath(guid);
                break;
            }
            if (ttfPath == null)
            {
                Debug.Log("[EchoFont] No font in Assets/Fonts/ — using TMP default. Drop an Exo 2 .ttf there to enable the custom font.");
                return;
            }

            var font = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);
            if (font == null) return;

            try
            {
                var fa = TMP_FontAsset.CreateFontAsset(font);
                if (fa == null) return;

                string assetPath = $"{FontDir}/{Path.GetFileNameWithoutExtension(ttfPath)} SDF.asset";
                AssetDatabase.DeleteAsset(assetPath);
                AssetDatabase.CreateAsset(fa, assetPath);

                // The companion material + atlas texture must live inside the asset to survive reimport.
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
                Debug.Log($"[EchoFont] Generated TMP font asset from {ttfPath}.");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[EchoFont] Auto-generating the TMP font asset failed — generate one once via " +
                    "Window ▸ TextMeshPro ▸ Font Asset Creator (load the .ttf, Generate Font Atlas, save into " +
                    "Assets/Fonts/), then re-run Build All. Using TMP default for now. (" + e.Message + ")");
            }
        }
    }
}
