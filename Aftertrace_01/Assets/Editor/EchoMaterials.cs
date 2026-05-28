using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace EchoShift.EditorTools
{
    /// <summary>
    /// Creates the shared Lit / Unlit sprite materials and a Bloom VolumeProfile.
    /// Lit responds to 2D lights (world surfaces); Unlit always renders bright so
    /// glows and the player read clearly and feed bloom.
    /// </summary>
    public static class EchoMaterials
    {
        public const string LitName = "EchoLit";
        public const string UnlitName = "EchoUnlit";
        public const string ParticleName = "EchoParticle";

        public static void GenerateAll()
        {
            EchoBuildUtils.EnsureFolder(EchoBuildUtils.MaterialDir);

            CreateMaterial(LitName, "Universal Render Pipeline/2D/Sprite-Lit-Default");
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

        static void CreateBloomProfile()
        {
            EchoBuildUtils.DeleteIfExists(EchoBuildUtils.BloomProfilePath);
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, EchoBuildUtils.BloomProfilePath);

            var bloom = profile.Add<Bloom>(true);
            bloom.threshold.Override(0.82f);
            bloom.intensity.Override(1.15f);
            bloom.scatter.Override(0.62f);
            bloom.tint.Override(Color.white);

            // VolumeComponents must be persisted as sub-assets of the profile, otherwise
            // the Bloom override is dropped on the next asset reload.
            bloom.hideFlags = HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(bloom, profile);
            EditorUtility.SetDirty(profile);
        }
    }
}
