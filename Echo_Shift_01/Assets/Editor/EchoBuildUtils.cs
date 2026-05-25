using System.IO;
using EchoShift;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

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
        public const string ResourcesDir = "Assets/Resources";
        public const string AppPrefabPath = "Assets/Resources/App.prefab";
        public const string Level1ScenePath = "Assets/_Scenes/Level_01.unity";
        public const string Level2ScenePath = "Assets/_Scenes/Level_02.unity";
        public const string MenuScenePath = "Assets/_Scenes/MainMenu.unity";
        public const string Level0ScenePath = "Assets/_Scenes/Level_00.unity";
        public const string Level3ScenePath = "Assets/_Scenes/Level_03.unity";

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

        // ---- UI helpers ---------------------------------------------------
        public static Canvas CreateOverlayCanvas(string name, int sortingOrder)
        {
            var go = new GameObject(name);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        // uGUI buttons need exactly one EventSystem in the scene to receive clicks.
        public static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        public static RectTransform FullStretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }

        public static RectTransform Place(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        public static Image CreateImage(string name, Transform parent, Sprite sprite, Color color, bool sliced = false)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            if (sprite != null && sliced) img.type = Image.Type.Sliced;
            return img;
        }

        public static TMP_Text CreateText(string name, Transform parent, string text, float fontSize, Color color, TextAlignmentOptions align)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = fontSize;
            t.color = color;
            t.alignment = align;
            t.raycastTarget = false;
            t.enableWordWrapping = true;
            return t;
        }

        public static Button CreateButton(string name, Transform parent, string label, Sprite bg,
            AudioSource uiSource, AudioClip hover, AudioClip click)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(360f, 74f);

            var img = go.AddComponent<Image>();
            img.sprite = bg;
            if (bg != null) img.type = Image.Type.Sliced;
            img.color = Color.white;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(0.6f, 0.92f, 1f, 1f);
            cb.pressedColor = new Color(0.45f, 0.7f, 0.9f, 1f);
            cb.selectedColor = Color.white;
            cb.colorMultiplier = 1f;
            cb.fadeDuration = 0.12f;
            btn.colors = cb;

            TMP_Text t = CreateText("Label", go.transform, label, 30f, new Color(0.85f, 0.96f, 1f, 1f), TextAlignmentOptions.Center);
            FullStretch(t.rectTransform);

            var sfx = go.AddComponent<ButtonSfx>();
            sfx.source = uiSource;
            sfx.hoverClip = hover;
            sfx.clickClip = click;
            return btn;
        }

        // World-space wall display for environmental narrative in breathing corridors.
        public static GameObject CreateWallNarrative(Transform parent, Vector3 pos, string text, Color? color = null, bool flicker = true)
        {
            var root = new GameObject("WallNarrative");
            if (parent != null) root.transform.SetParent(parent, false);
            root.transform.position = pos;

            var panel = SpriteGO("Screen", LoadSprite("platform"), "Midground", 2, root.transform,
                LoadMaterial(EchoMaterials.UnlitName), new Color(0.03f, 0.06f, 0.12f, 0.7f));
            panel.transform.localScale = new Vector3(3.6f, 1.3f, 1f);

            var tgo = new GameObject("Text");
            tgo.transform.SetParent(root.transform, false);
            tgo.transform.localPosition = new Vector3(0f, 0f, -0.02f);
            var tmp = tgo.AddComponent<TextMeshPro>();
            tmp.text = text;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 2.1f;
            tmp.color = color ?? new Color(0f, 0.83f, 1f, 1f);
            tmp.rectTransform.sizeDelta = new Vector2(6.4f, 2.4f);
            var mr = tgo.GetComponent<MeshRenderer>();
            if (mr != null) { mr.sortingLayerID = SortingLayer.NameToID("Foreground"); mr.sortingOrder = 3; }

            if (flicker)
            {
                var fl = root.AddComponent<Flicker>();
                fl.spriteTarget = panel.GetComponent<SpriteRenderer>();
                fl.min = 0.62f;
                fl.max = 1f;
            }
            return root;
        }
    }
}
