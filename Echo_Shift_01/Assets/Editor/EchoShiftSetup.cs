using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EchoShift.EditorTools
{
    /// <summary>
    /// One-click builder for Echo Shift Level 1. Generates all art, materials, audio,
    /// prefabs and the scene, wiring everything up. Idempotent: re-running cleans and
    /// rebuilds from scratch with no duplicates.
    /// </summary>
    public static class EchoShiftSetup
    {
        [MenuItem("EchoShift/Build Level 1")]
        public static void BuildLevel1()
        {
            // TextMeshPro needs its essential resources (font asset) before we can build
            // the UI/world text. Auto-import once, then ask for a single re-run.
            if (!TmpEssentialsPresent())
            {
                if (TryImportTmpEssentials())
                    Debug.Log("[EchoShift] Imported TMP Essential Resources. Click EchoShift ▸ Build Level 1 once more to finish the build.");
                else
                    Debug.LogWarning("[EchoShift] Could not auto-import TMP Essentials. Use Window ▸ TextMeshPro ▸ Import TMP Essential Resources, then re-run.");
                return;
            }

            try
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                EchoBuildUtils.CleanGenerated();
                EchoBuildUtils.EnsureAllFolders();
                EchoBuildUtils.EnsureSortingLayers();
                EchoBuildUtils.EnsurePhysicsLayer(EchoBuildUtils.GroundLayer);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EchoArt.GenerateAll();
                EchoMaterials.GenerateAll();
                EchoAudio.GenerateAll();
                EchoPrefabs.GenerateAll();
                EchoScene.Build();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("✅ Echo Shift Level 1 built! Press Play to test.");
            }
            catch (System.Exception e)
            {
                Debug.LogError("[EchoShift] Build failed: " + e);
            }
        }

        [MenuItem("EchoShift/Clean Generated Assets")]
        public static void CleanGenerated()
        {
            EchoBuildUtils.CleanGenerated();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[EchoShift] Cleaned generated assets.");
        }

        static bool TmpEssentialsPresent()
            => File.Exists("Assets/TextMesh Pro/Resources/TMP Settings.asset");

        static bool TryImportTmpEssentials()
        {
            try
            {
                TMP_PackageResourceImporter.ImportResources(true, false, false);
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[EchoShift] TMP import failed: " + e.Message);
                return false;
            }
        }
    }
}
