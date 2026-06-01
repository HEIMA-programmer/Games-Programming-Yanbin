using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EchoShift.EditorTools
{
    /// <summary>
    /// One-click builders for Aftertrace. "Build All" generates shared assets (art,
    /// materials, audio, prefabs, persistent App) plus the three scenes and registers
    /// them in Build Settings. Individual scene builders are also exposed. Idempotent.
    /// </summary>
    public static class AftertraceSetup
    {
        [MenuItem("Aftertrace/Build All", false, 0)]
        public static void BuildAll()
        {
            if (!EnsureTmp()) return;
            if (!ConfirmRegen("Build All", "ALL scenes (MainMenu + the 4 level blockouts)")) return;
            try
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                BuildShared();
                EchoMenuScene.Build();
                EchoLevel0.Build();
                EchoScene.Build();
                EchoLevel2.Build();
                EchoLevel3.Build();
                RegisterScenes();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorSceneManager.OpenScene(EchoBuildUtils.MenuScenePath);
                Debug.Log("✅ Aftertrace built: shared assets + MainMenu + 4 level BLOCKOUTS (colliders + entities, no terrain art). Hand-author art on tilemap layers in each level.");
            }
            catch (System.Exception e) { Debug.LogError("[Aftertrace] Build All failed: " + e); }
        }

        [MenuItem("Aftertrace/Build Main Menu", false, 20)]
        public static void BuildMainMenu()
        {
            if (!EnsureTmp()) return;
            if (!ConfirmRegen("Build Main Menu", "the MainMenu scene")) return;
            try
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                BuildShared();
                EchoMenuScene.Build();
                RegisterScenes();
                Done("Main Menu", EchoBuildUtils.MenuScenePath);
            }
            catch (System.Exception e) { Debug.LogError("[Aftertrace] Build Main Menu failed: " + e); }
        }

        [MenuItem("Aftertrace/Build Level 1", false, 21)]
        public static void BuildLevel1()
        {
            if (!EnsureTmp()) return;
            if (!ConfirmRegen("Build Level 1", "the Level 1 scene")) return;
            try
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                BuildShared();
                EchoScene.Build();
                RegisterScenes();
                Done("Level 1", EchoBuildUtils.Level1ScenePath);
            }
            catch (System.Exception e) { Debug.LogError("[Aftertrace] Build Level 1 failed: " + e); }
        }

        [MenuItem("Aftertrace/Build Level 2", false, 22)]
        public static void BuildLevel2()
        {
            if (!EnsureTmp()) return;
            if (!ConfirmRegen("Build Level 2", "the Level 2 scene")) return;
            try
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                BuildShared();
                EchoLevel2.Build();
                RegisterScenes();
                Done("Level 2", EchoBuildUtils.Level2ScenePath);
            }
            catch (System.Exception e) { Debug.LogError("[Aftertrace] Build Level 2 failed: " + e); }
        }

        [MenuItem("Aftertrace/Build Level 0", false, 23)]
        public static void BuildLevel0()
        {
            if (!EnsureTmp()) return;
            if (!ConfirmRegen("Build Level 0", "the Level 0 scene")) return;
            try
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                BuildShared();
                EchoLevel0.Build();
                RegisterScenes();
                Done("Level 0", EchoBuildUtils.Level0ScenePath);
            }
            catch (System.Exception e) { Debug.LogError("[Aftertrace] Build Level 0 failed: " + e); }
        }

        [MenuItem("Aftertrace/Build Level 3", false, 24)]
        public static void BuildLevel3()
        {
            if (!EnsureTmp()) return;
            if (!ConfirmRegen("Build Level 3", "the Level 3 scene")) return;
            try
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                BuildShared();
                EchoLevel3.Build();
                RegisterScenes();
                Done("Level 3", EchoBuildUtils.Level3ScenePath);
            }
            catch (System.Exception e) { Debug.LogError("[Aftertrace] Build Level 3 failed: " + e); }
        }

        // SAFE clean: only removes the orphaned Tile cache left by the retired procedural
        // terrain painter. Never touches prefabs/materials/sprites/audio/fonts/scenes, so it
        // can't wipe hand-authored work. (The old "delete everything" clean was removed.)
        [MenuItem("Aftertrace/Clean Orphaned Tile Cache", false, 40)]
        public static void CleanGenerated()
        {
            if (!EditorUtility.DisplayDialog("Delete orphaned tile cache?",
                "Deletes Assets/Art/Tiles — the Tile assets the old procedural terrain painter generated. Nothing references them after the blockout switch.\n\nLeaves prefabs, materials, sprites, audio, fonts and scenes untouched. Continue?",
                "Delete", "Cancel")) return;
            EchoBuildUtils.DeleteIfExists("Assets/Art/Tiles");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Aftertrace] Deleted orphaned tile cache (Assets/Art/Tiles).");
        }

        // Shared assets used by every scene (idempotent — regenerated in place).
        static void BuildShared()
        {
            EchoBuildUtils.EnsureAllFolders();
            EchoBuildUtils.EnsureFolder(EchoBuildUtils.ResourcesDir);
            EchoBuildUtils.EnsureSortingLayers();
            EchoBuildUtils.EnsurePhysicsLayer(EchoBuildUtils.GroundLayer);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EchoArt.GenerateAll();
            EchoSpriteSlicer.SliceAll();  // slice CraftPix / Kenney spritesheets so LoadSprite can address named frames
            EchoMaterials.GenerateAll();
            EchoAudio.GenerateAll();
            EchoPrefabs.GenerateAll();
            EchoFont.EnsureFontAsset();   // load/generate UI fonts (Exo 2 body + Orbitron title)
        }

        static void Done(string label, string scenePath)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(scenePath);
            Debug.Log($"✅ Aftertrace {label} built! Press Play to test.");
        }

        static void RegisterScenes()
        {
            var list = new List<EditorBuildSettingsScene>();
            AddScene(list, EchoBuildUtils.MenuScenePath);
            AddScene(list, EchoBuildUtils.Level0ScenePath);
            AddScene(list, EchoBuildUtils.Level1ScenePath);
            AddScene(list, EchoBuildUtils.Level2ScenePath);
            AddScene(list, EchoBuildUtils.Level3ScenePath);
            EditorBuildSettings.scenes = list.ToArray();
        }

        static void AddScene(List<EditorBuildSettingsScene> list, string path)
        {
            if (File.Exists(path)) list.Add(new EditorBuildSettingsScene(path, true));
        }

        // Scene builders OVERWRITE their scene with a freshly-generated gameplay blockout
        // (colliders + entities only — terrain/background art is hand-authored on tilemap
        // layers). Guard every builder behind a confirm so a stray click can't wipe hand art.
        static bool ConfirmRegen(string label, string what)
            => EditorUtility.DisplayDialog(
                "Regenerate gameplay blockout?",
                $"\"{label}\" rebuilds {what} from code as a clean gameplay BLOCKOUT — colliders " +
                "+ entities only, no terrain/background art.\n\nThis OVERWRITES any hand-authored " +
                "art already in those scenes. Continue?",
                "Regenerate (overwrite)", "Cancel");

        // ---- TMP gate ----
        static bool EnsureTmp()
        {
            if (TmpEssentialsPresent()) return true;
            if (TryImportTmpEssentials())
                Debug.Log("[Aftertrace] Imported TMP Essential Resources. Click the Aftertrace menu item again to finish the build.");
            else
                Debug.LogWarning("[Aftertrace] Could not auto-import TMP Essentials. Use Window ▸ TextMeshPro ▸ Import TMP Essential Resources, then re-run.");
            return false;
        }

        static bool TmpEssentialsPresent() => File.Exists("Assets/TextMesh Pro/Resources/TMP Settings.asset");

        static bool TryImportTmpEssentials()
        {
            try
            {
                TMP_PackageResourceImporter.ImportResources(true, false, false);
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[Aftertrace] TMP import failed: " + e.Message);
                return false;
            }
        }
    }
}
