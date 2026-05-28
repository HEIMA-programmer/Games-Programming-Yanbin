using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace EchoShift.EditorTools
{
    /// <summary>
    /// Creates the shared sprite materials and an (empty) VolumeProfile.
    /// Pure 1-Bit target: both "Lit" and "Unlit" logical materials bind the Unlit
    /// shader so every sprite renders flat at full brightness, ignoring all 2D lights.
    /// </summary>
    public static class EchoMaterials
    {
        public const string LitName = "EchoLit";
        public const string UnlitName = "EchoUnlit";
        public const string ParticleName = "EchoParticle";

        public static void GenerateAll()
        {
            EchoBuildUtils.EnsureFolder(EchoBuildUtils.MaterialDir);

            // Pure 1-Bit target: NOTHING is lit. Both logical materials bind the Unlit
            // shader, so every sprite renders flat at full brightness and ignores all
            // Light2D — CraftPix white pixels stay pure white on black. The Light2D
            // objects the builders still scatter become harmless no-ops (a later pass
            // can delete them outright).
            CreateMaterial(LitName, "Universal Render Pipeline/2D/Sprite-Unlit-Default");
            CreateMaterial(UnlitName, "Universal Render Pipeline/2D/Sprite-Unlit-Default");
            CreateParticleMaterial();
            CreateBloomProfile();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        // Particles + trails: built-in "Sprites/Default" with the soft dot texture is
        // guaranteed to render (vertex-colored, alpha-blended) — no URP shader binding risk.
        static void CreateParticleMaterial()
        {
            Shader shader = Shader.Find("Sprites/Default");
            var mat = new Material(shader) { name = ParticleName };
            Texture2D dot = AssetDatabase.LoadAssetAtPath<Texture2D>($"{EchoBuildUtils.SpriteDir}/particle.png");
            if (dot != null) mat.mainTexture = dot;
            string path = $"{EchoBuildUtils.MaterialDir}/{ParticleName}.mat";
            EchoBuildUtils.DeleteIfExists(path);
            AssetDatabase.CreateAsset(mat, path);
        }

        static void CreateMaterial(string name, string shaderName)
        {
            Shader shader = Shader.Find(shaderName);
            if (shader == null)
            {
                Debug.LogWarning($"[EchoShift] Shader not found: {shaderName}");
                shader = Shader.Find("Sprites/Default");
            }
            var mat = new Material(shader) { name = name };
            string path = $"{EchoBuildUtils.MaterialDir}/{name}.mat";
            EchoBuildUtils.DeleteIfExists(path);
            AssetDatabase.CreateAsset(mat, path);
        }

        // Pure 1-Bit target: no post-processing. We still create the profile asset (the
        // scene builders reference it on a global Volume), but leave it EMPTY so the
        // Volume is a no-op. Bloom on a pure-white image would bleed every edge and wash
        // the whole screen grey — the opposite of the crisp 1-bit look.
        static void CreateBloomProfile()
        {
            EchoBuildUtils.DeleteIfExists(EchoBuildUtils.BloomProfilePath);
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, EchoBuildUtils.BloomProfilePath);
            EditorUtility.SetDirty(profile);
        }
    }
}
