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
    /// Aftertrace level builder. All operations are idempotent.
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

        /// <summary>Custom UI font (Exo 2, OFL), set by EchoFont.EnsureFontAsset during Build All.
        /// Null → fall back to TMP's default (Liberation Sans), so a missing font never breaks the build.</summary>
        public static TMP_FontAsset CustomFont;

        /// <summary>Display/title font (Orbitron, OFL), set by EchoFont.EnsureFontAsset. Used for
        /// menu titles via CreateTitleText. Null → falls back to CustomFont.</summary>
        public static TMP_FontAsset TitleFont;

        // ---- Palette -------------------------------------------------------
        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color c);
            return c;
        }

        public static readonly Color ColBackground = Hex("#000000"); // pure black — 1-Bit contrast
        // Cyan accents — for HUD, lights, glow effects (NOT for 1-Bit sprite tinting).
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

        // Pure 1-Bit sprite tints — everything renders PURE WHITE on black. There is no
        // grey ramp: depth and material reads come from SHAPE, scale, dither density and
        // sparseness, NOT from hue or brightness. Tinting a white sprite grey only dims
        // its baked dithering into mud, so we keep all tints at full white.
        public static readonly Color TintPlayer   = Color.white;
        public static readonly Color TintEcho     = new Color(1f, 1f, 1f, 0.45f);  // ghost: alpha only, never colour
        public static readonly Color TintDrone    = Color.white;  // hostile read via its alien SHAPE, not a warm tint
        public static readonly Color TintDoor     = Color.white;
        public static readonly Color TintFragment = Color.white;
        public static readonly Color TintEnd      = Color.white;
        // Background layers stay pure white too; the CraftPix backdrops carry their own
        // baked dither (the "grey" planets are dot patterns, not flat grey), so full white
        // preserves that texture instead of crushing it.
        public static readonly Color TintBgFar    = Color.white;
        public static readonly Color TintBgMid    = Color.white;
        public static readonly Color TintBgNear   = Color.white;
        public static readonly Color TintNpc      = Color.white;
        public static readonly Color TintBorder   = Color.white;
        // Deprecated — kept for back compat.
        public static readonly Color TintDecor    = Color.white;
        public static readonly Color TintDetail   = Color.white;

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

        // Maps the project's logical sprite names to imported CraftPix / Kenney frames.
        // EchoSpriteSlicer slices the source PNGs into named sprites ("{base}_{index}");
        // anything missing here falls back to the procedural Assets/Art/Sprites/ asset.
        static readonly System.Collections.Generic.Dictionary<string, (string path, string frame)> ImportedSprites =
            new System.Collections.Generic.Dictionary<string, (string, string)>
            {
                { "player",          ("Assets/Art/Imported/CraftPix1Bit/Main_Characters/Char_Robot.png", "Char_Robot_0") },
                { "echo",            ("Assets/Art/Imported/CraftPix1Bit/Main_Characters/Char_Robot.png", "Char_Robot_0") },
                { "drone",           ("Assets/Art/Imported/CraftPix1Bit/Enemies/Alien1.png",             "Alien1_0") },
                // Tileset_24 = interior of the white dithered "rock mass" — the ONE tile that
                // tiles seamlessly (no built-in border), so Tiled drawMode fills any block size
                // into a clean white dotted terrain instead of repeating the old box-corner noise.
                { "platform",        ("Assets/Art/Imported/CraftPix1Bit/Tileset/Tileset.png",            "Tileset_24") },
                { "door",            ("Assets/Art/Imported/CraftPix1Bit/Objects/Door.png",               "Door_0") },
                { "fragment",        ("Assets/Art/Imported/CraftPix1Bit/Objects/Items.png",              "Items_4") },
                { "endarch",         ("Assets/Art/Imported/CraftPix1Bit/Objects/Door.png",               "Door_3") },
                { "arrow",           ("Assets/Art/Imported/CraftPix1Bit/GUI/Icons.png",                  "Icons_69") },
                // Walls / ceiling share the same seamless rock fill (Tileset_24) so all
                // terrain reads as one cohesive white dithered material (the reference look).
                { "platform_wall",   ("Assets/Art/Imported/CraftPix1Bit/Tileset/Tileset.png",            "Tileset_24") },
                { "platform_ceiling",("Assets/Art/Imported/CraftPix1Bit/Tileset/Tileset.png",            "Tileset_24") },
                { "checkpoint",      ("Assets/Art/Imported/CraftPix1Bit/Objects/checkpoint.png",         "checkpoint_0") },
                // HUD fragment counter — both states use the same gem; outline = lower alpha at use site.
                { "diamond_filled",  ("Assets/Art/Imported/CraftPix1Bit/Objects/Items.png",              "Items_4") },
                { "diamond_outline", ("Assets/Art/Imported/CraftPix1Bit/Objects/Items.png",              "Items_4") },
                // Background: first of 8 sliced 80×80 decoration variants.
                { "background",      ("Assets/Art/Imported/CraftPix1Bit/Tileset/Background_n_details.png", "Background_n_details_0") },
                { "box_lab",         ("Assets/Art/Imported/CraftPix1Bit/Objects/Boxes.png",              "Boxes_0") },
                { "box_stack",       ("Assets/Art/Imported/CraftPix1Bit/Objects/Boxes.png",              "Boxes_4") },
                { "debris",          ("Assets/Art/Imported/CraftPix1Bit/Objects/Boxes.png",              "Boxes_12") },
                { "memory_vial",     ("Assets/Art/Imported/CraftPix1Bit/Objects/Cups.png",               "Cups_12") },
                { "memory_core",     ("Assets/Art/Imported/CraftPix1Bit/Objects/Cups.png",               "Cups_24") },
                { "wall_pipe",       ("Assets/Art/Imported/CraftPix1Bit/Tileset/Tileset_details.png",    "Tileset_details_2") },
                { "wall_console",    ("Assets/Art/Imported/CraftPix1Bit/Tileset/Tileset_details.png",    "Tileset_details_13") },
                { "panel_border",    ("Assets/Art/Imported/CraftPix1Bit/Tileset/Tileset_Borders.png",    "Tileset_Borders_2") },
                { "gui_meter",       ("Assets/Art/Imported/CraftPix1Bit/GUI/GUI_Elements.png",           "GUI_Elements_0") },
                { "gui_faceplate",   ("Assets/Art/Imported/CraftPix1Bit/GUI/GUI_Elements.png",           "GUI_Elements_16") },
                { "trap_burst",      ("Assets/Art/Imported/CraftPix1Bit/Traps/Trap1.png",                "Trap1_4") },
                { "trap_flame",      ("Assets/Art/Imported/CraftPix1Bit/Traps/Trap2.png",                "Trap2_20") },
                { "trap_arc",        ("Assets/Art/Imported/CraftPix1Bit/Traps/Trap3.png",                "Trap3_4") },
            };

        // Public read access so level/menu builders can grab specific decoration frames.
        public static Sprite LoadImportedSprite(string assetPath, string frameName)
            => LoadImportedFrame(assetPath, frameName);

        public static Sprite LoadSprite(string fileName)
        {
            if (ImportedSprites.TryGetValue(fileName, out var info))
            {
                var s = LoadImportedFrame(info.path, info.frame);
                if (s != null) return s;
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>($"{SpriteDir}/{fileName}.png");
        }

        static Sprite LoadImportedFrame(string path, string frameName)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            Sprite first = null;
            foreach (var a in assets)
            {
                if (!(a is Sprite s)) continue;
                if (s.name == frameName) return s;
                if (first == null) first = s;
            }
            return first; // fallback if the named frame was not found (e.g. unsliced sheet)
        }

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

        // Crisp white surface line along the top of walkable terrain — the 1-Bit "edge"
        // read that makes the white dithered mass land as solid ground. Only wider-than-tall
        // blocks (floors/platforms) get it; walls (w < h) are skipped. Uses the solid "white"
        // sprite scaled to a thin bar (solid colour → scaling never distorts).
        public static void AddTopEdge(GameObject block, float w, float h, Material material)
        {
            if (w < h) return;
            var bsr = block.GetComponent<SpriteRenderer>();
            const float t = 0.12f;
            var edge = SpriteGO("TopEdge", LoadSprite("white"),
                bsr != null ? bsr.sortingLayerName : "Environment",
                (bsr != null ? bsr.sortingOrder : 0) + 1,
                block.transform, material, Color.white);
            edge.transform.localScale = new Vector3(w, t, 1f);
            edge.transform.localPosition = new Vector3(0f, h * 0.5f - t * 0.5f, -0.01f);
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
            if (CustomFont != null) t.font = CustomFont;
            return t;
        }

        // Display font for menu / pause / victory titles. Falls back to body font.
        public static TMP_Text CreateTitleText(string name, Transform parent, string text, float fontSize, Color color, TextAlignmentOptions align)
        {
            var t = CreateText(name, parent, text, fontSize, color, align);
            if (TitleFont != null) t.font = TitleFont;
            return t;
        }

        public enum FrameSide { Top, Bottom, Left, Right }

        // Thin white strip anchored to one side of the parent canvas — call 4× to frame the viewport.
        public static void AddFrameStrip(Transform parent, Color color, FrameSide side, float thickness = 5f, float inset = 5f)
        {
            var go = new GameObject("Frame_" + side);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            var rt = img.rectTransform;
            switch (side)
            {
                case FrameSide.Top:
                    rt.anchorMin = new Vector2(0f, 1f); rt.anchorMax = new Vector2(1f, 1f);
                    rt.pivot = new Vector2(0.5f, 1f);
                    rt.sizeDelta = new Vector2(0f, thickness);
                    rt.anchoredPosition = new Vector2(0f, -inset); break;
                case FrameSide.Bottom:
                    rt.anchorMin = new Vector2(0f, 0f); rt.anchorMax = new Vector2(1f, 0f);
                    rt.pivot = new Vector2(0.5f, 0f);
                    rt.sizeDelta = new Vector2(0f, thickness);
                    rt.anchoredPosition = new Vector2(0f, inset); break;
                case FrameSide.Left:
                    rt.anchorMin = new Vector2(0f, 0f); rt.anchorMax = new Vector2(0f, 1f);
                    rt.pivot = new Vector2(0f, 0.5f);
                    rt.sizeDelta = new Vector2(thickness, 0f);
                    rt.anchoredPosition = new Vector2(inset, 0f); break;
                case FrameSide.Right:
                    rt.anchorMin = new Vector2(1f, 0f); rt.anchorMax = new Vector2(1f, 1f);
                    rt.pivot = new Vector2(1f, 0.5f);
                    rt.sizeDelta = new Vector2(thickness, 0f);
                    rt.anchoredPosition = new Vector2(-inset, 0f); break;
            }
        }

        // Per-instance material so the outline does not bake into the shared font asset.
        // We assign via fontSharedMaterial (no edit-mode warning) but instantiate first.
        public static void ApplyOutline(TMP_Text t, Color outlineColor, float widthNorm)
        {
            if (t == null || t.fontSharedMaterial == null) return;
            var matInstance = Object.Instantiate(t.fontSharedMaterial);
            matInstance.name = t.fontSharedMaterial.name + " (Outline)";
            matInstance.SetColor(TMPro.ShaderUtilities.ID_OutlineColor, outlineColor);
            matInstance.SetFloat(TMPro.ShaderUtilities.ID_OutlineWidth, widthNorm);
            t.fontSharedMaterial = matInstance;
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
            // 1-Bit: greyscale hover/press feedback (no cyan).
            cb.highlightedColor = new Color(0.72f, 0.72f, 0.72f, 1f);
            cb.pressedColor = new Color(0.45f, 0.45f, 0.45f, 1f);
            cb.selectedColor = Color.white;
            cb.colorMultiplier = 1f;
            cb.fadeDuration = 0.12f;
            btn.colors = cb;

            TMP_Text t = CreateText("Label", go.transform, label, 36f, new Color(1f, 1f, 1f, 1f), TextAlignmentOptions.Center);
            t.fontStyle = FontStyles.Bold;
            FullStretch(t.rectTransform);

            var sfx = go.AddComponent<ButtonSfx>();
            sfx.source = uiSource;
            sfx.hoverClip = hover;
            sfx.clickClip = click;
            return btn;
        }

    }
}
