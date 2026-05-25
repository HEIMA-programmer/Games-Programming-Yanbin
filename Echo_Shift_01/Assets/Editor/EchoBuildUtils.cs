using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace EchoShift.EditorTools
{
    /// <summary>
    /// Shared paths, folder/layer setup, and GameObject/Sprite/Light helpers used by the
    /// Echo Shift level builder. All operations are idempotent.
    /// </summary>
    public static class EchoBuildUtils
    {
        public const string ArtDir = "Assets/Art";
        public const string SpriteDir = "Assets/Art/Sprites";
        public const string MaterialDir = "Assets/Art/Materials";
        public const string PrefabDir = "Assets/Prefabs";
        public const string AudioDir = "Assets/Audio";
        public const string SceneDir = "Assets/_Scenes";
        public const string ScenePath = "Assets/_Scenes/Level_01.unity";
        public const string BloomProfilePath = "Assets/Art/Materials/EchoBloomProfile.asset";

        public const string GroundLayer = "Ground";

        // Sorting layers, back to front (Default stays implicitly at the back, unused for gameplay).
        public static readonly string[] SortingLayers =
            { "Background", "Midground", "Environment", "Player", "Foreground", "UI" };

        // ---- Palette -------------------------------------------------------
        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color c);
            return c;
        }

        public static readonly Color ColBackground = Hex("#0a0e1a");
        public static readonly Color ColPlayer = Hex("#00d4ff");
        public static readonly Color ColPlayerCore = Hex("#bff6ff");
        public static readonly Color ColEcho = Hex("#aaddff");
        public static readonly Color ColPlateAmber = Hex("#ffaa00");
        public static readonly Color ColPlateGreen = Hex("#00ff88");
        public static readonly Color ColDoor = Hex("#556677");
        public static readonly Color ColPlatform = Hex("#1a2233");
        public static readonly Color ColPlatformEdge = Hex("#33425a");
        public static readonly Color ColFragment = Hex("#ffcc44");
        public static readonly Color ColEnd = Hex("#44ffaa");

        // ---- Folders -------------------------------------------------------
        public static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string leaf = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        public static void EnsureAllFolders()
        {
            EnsureFolder(ArtDir);
            EnsureFolder(SpriteDir);
            EnsureFolder(MaterialDir);
            EnsureFolder(PrefabDir);
            EnsureFolder(AudioDir);
            EnsureFolder(SceneDir);
        }

        public static void DeleteIfExists(string assetPath)
        {
            if (AssetDatabase.LoadMainAssetAtPath(assetPath) != null || AssetDatabase.IsValidFolder(assetPath))
                AssetDatabase.DeleteAsset(assetPath);
        }

        public static void CleanGenerated()
        {
            DeleteIfExists(SpriteDir);
            DeleteIfExists(MaterialDir);
            DeleteIfExists(PrefabDir);
            DeleteIfExists(AudioDir);
            DeleteIfExists(ScenePath);
        }

        // ---- Sorting layers (via TagManager) -------------------------------
        public static void EnsureSortingLayers()
        {
            Object tagManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0];
            SerializedObject so = new SerializedObject(tagManager);
            SerializedProperty layers = so.FindProperty("m_SortingLayers");
            if (layers == null) return;

            foreach (string name in SortingLayers)
            {
                if (SortingLayerNameExists(layers, name)) continue;
                layers.InsertArrayElementAtIndex(layers.arraySize);
                SerializedProperty el = layers.GetArrayElementAtIndex(layers.arraySize - 1);
                el.FindPropertyRelative("name").stringValue = name;
                el.FindPropertyRelative("uniqueID").intValue = UniqueSortingId(layers, name);
                SerializedProperty locked = el.FindPropertyRelative("locked");
                if (locked != null) locked.boolValue = false;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(tagManager);
        }

        static bool SortingLayerNameExists(SerializedProperty layers, string name)
        {
            for (int i = 0; i < layers.arraySize; i++)
                if (layers.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue == name)
                    return true;
            return false;
        }

        static int UniqueSortingId(SerializedProperty layers, string name)
        {
            int id = Mathf.Abs(Animator.StringToHash(name));
            if (id == 0) id = 1;
            bool collides = true;
            while (collides)
            {
                collides = false;
                for (int i = 0; i < layers.arraySize; i++)
                {
                    if (layers.GetArrayElementAtIndex(i).FindPropertyRelative("uniqueID").intValue == id)
                    {
                        id++;
                        collides = true;
                        break;
                    }
                }
            }
            return id;
        }

        // ---- Physics layer -------------------------------------------------
        public static int EnsurePhysicsLayer(string name)
        {
            int existing = LayerMask.NameToLayer(name);
            if (existing != -1) return existing;

            Object tagManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0];
            SerializedObject so = new SerializedObject(tagManager);
            SerializedProperty layersProp = so.FindProperty("layers");
            for (int i = 8; i < 32; i++)
            {
                SerializedProperty el = layersProp.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(el.stringValue))
                {
                    el.stringValue = name;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(tagManager);
                    return i;
                }
            }
            return -1;
        }

        // ---- Loading -------------------------------------------------------
        public static Sprite LoadSprite(string fileName)
            => AssetDatabase.LoadAssetAtPath<Sprite>($"{SpriteDir}/{fileName}.png");

        public static Material LoadMaterial(string fileName)
            => AssetDatabase.LoadAssetAtPath<Material>($"{MaterialDir}/{fileName}.mat");

        public static AudioClip LoadAudio(string fileName)
            => AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioDir}/{fileName}.wav");

        // ---- GameObject / Sprite / Light helpers ---------------------------
        public static GameObject SpriteGO(string name, Sprite sprite, string sortingLayer, int order,
            Transform parent = null, Material material = null, Color? color = null)
        {
            GameObject go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent, false);
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingLayerName = sortingLayer;
            sr.sortingOrder = order;
            if (material != null) sr.sharedMaterial = material;
            if (color.HasValue) sr.color = color.Value;
            return go;
        }

        public static Light2D AddPointLight(GameObject go, Color color, float intensity, float outerRadius, float innerRadius = 0f)
        {
            Light2D light = go.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.pointLightOuterRadius = outerRadius;
            light.pointLightInnerRadius = innerRadius;
            light.falloffIntensity = 0.6f;
            return light;
        }

        public static Light2D AddGlobalLight(GameObject go, Color color, float intensity)
        {
            Light2D light = go.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.color = color;
            light.intensity = intensity;
            return light;
        }
    }
}
