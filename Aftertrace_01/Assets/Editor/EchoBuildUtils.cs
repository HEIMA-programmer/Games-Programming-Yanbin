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
            };

        // Public read access so level/menu builders can grab specific decoration frames.
        public static Sprite LoadImportedSprite(string assetPath, string frameName)
            => LoadImportedFrame(assetPath, frameName);

        // Rotates through the 8 sliced backdrop variants in Background_n_details.png.
        public static Sprite GetBackgroundVariant(int index)
            => LoadImportedFrame(
                "Assets/Art/Imported/CraftPix1Bit/Tileset/Background_n_details.png",
                $"Background_n_details_{((index % 8) + 8) % 8}");

        // Rotates through Tileset_details frames (304 × 144 → 19 × 9 = 171 frames) for
        // foreground prop sprinkling — circuits, pipes, panels.
        public static Sprite GetTilesetDetail(int index)
            => LoadImportedFrame(
                "Assets/Art/Imported/CraftPix1Bit/Tileset/Tileset_details.png",
                $"Tileset_details_{((index % 171) + 171) % 171}");

        // Tileset_Borders: 11×13 = 143 framed square/rect tiles for accents.
        public static Sprite GetBorder(int index)
            => LoadImportedFrame(
                "Assets/Art/Imported/CraftPix1Bit/Tileset/Tileset_Borders.png",
                $"Tileset_Borders_{((index % 143) + 143) % 143}");

        // Boxes: 4×4 = 16 crate variants at 32×32.
        public static Sprite GetBox(int index)
            => LoadImportedFrame(
                "Assets/Art/Imported/CraftPix1Bit/Objects/Boxes.png",
                $"Boxes_{((index % 16) + 16) % 16}");

        // Trap6: 8×6 = 48 grinder/crusher / dome machine frames at 48×48.
        public static Sprite GetMachine(int index)
            => LoadImportedFrame(
                "Assets/Art/Imported/CraftPix1Bit/Traps/Trap6.png",
                $"Trap6_{((index % 48) + 48) % 48}");

        // Char_Boy: spritesheet for background NPC silhouettes — 48 frames.
        public static Sprite GetNpcBoy(int index)
            => LoadImportedFrame(
                "Assets/Art/Imported/CraftPix1Bit/Main_Characters/Char_Boy.png",
                $"Char_Boy_{((index % 48) + 48) % 48}");

        // Char_Girl alternative NPC.
        public static Sprite GetNpcGirl(int index)
            => LoadImportedFrame(
                "Assets/Art/Imported/CraftPix1Bit/Main_Characters/Char_Girl.png",
                $"Char_Girl_{((index % 48) + 48) % 48}");

        // Tileset_GUI: 11×23 = 253 panel/button/frame tiles — for menu UI.
        public static Sprite GetGuiTile(int index)
            => LoadImportedFrame(
                "Assets/Art/Imported/CraftPix1Bit/GUI/Tileset_GUI.png",
                $"Tileset_GUI_{((index % 253) + 253) % 253}");

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

        // Sprinkles a few dark "craters" across a large terrain mass so it reads as textured
        // rock with depth instead of a flat uniform slab (the dark holes in the CraftPix
        // reference rock). Only big ground blocks get them; thin platforms / walls are skipped.
        // Deterministic: seeded from the block's position so rebuilds are stable.
        public static void SprinkleCraters(GameObject block, float w, float h, Material material)
        {
            if (w < 5f || h < 2f) return;
            var bsr = block.GetComponent<SpriteRenderer>();
            string layer = bsr != null ? bsr.sortingLayerName : "Environment";
            int baseOrder = bsr != null ? bsr.sortingOrder : 0;

            // 1) Shadow band along the bottom → volume (lit top, shadowed underside).
            float bandH = Mathf.Min(h, 1.5f);
            var shade = SpriteGO("RockShade", LoadSprite("rockshade"), layer, baseOrder + 1, block.transform, material, Color.white);
            var ssr = shade.GetComponent<SpriteRenderer>();
            ssr.drawMode = SpriteDrawMode.Tiled;                       // tile horizontally; native 1.5u height = no vertical repeat
            ssr.size = new Vector2(w, bandH);
            shade.transform.localPosition = new Vector3(0f, -h * 0.5f + bandH * 0.5f, -0.004f);

            // 2) Dark cavities, biased toward the lower (shadowed) half. Deterministic per block.
            UnityEngine.Random.State prev = UnityEngine.Random.state;
            Vector3 p = block.transform.position;
            UnityEngine.Random.InitState(Mathf.RoundToInt(p.x * 13.1f + p.y * 7.7f + w * 3.3f));

            int count = Mathf.Clamp(Mathf.FloorToInt(w / 6f), 1, 6);
            float maxR = Mathf.Min(0.6f, h * 0.32f);
            for (int i = 0; i < count; i++)
            {
                float r = UnityEngine.Random.Range(maxR * 0.4f, maxR);
                float yTop = h * 0.5f - 0.35f - r;        // stay below the top-edge surface line
                float yBot = -h * 0.5f + r + 0.1f;
                if (yBot >= yTop) continue;
                float bias = Mathf.Pow(UnityEngine.Random.value, 1.6f);  // skew toward the bottom
                float y = Mathf.Lerp(yBot, yTop, bias);
                float x = UnityEngine.Random.Range(-w * 0.5f + r + 0.2f, w * 0.5f - r - 0.2f);
                string sprite = (i % 2 == 0) ? "crater" : "crater2";
                var cr = SpriteGO("Crater", LoadSprite(sprite), layer, baseOrder + 2, block.transform, material, Color.white);
                cr.transform.localScale = new Vector3(r * 2f, r * 2f, 1f);  // crater sprite is 1u diameter
                cr.transform.localPosition = new Vector3(x, y, -0.005f);
            }
            UnityEngine.Random.state = prev;
        }

        // Sparse decorative props (kit crates) resting ON a large ground block's surface —
        // the deliberate "stuff on the floor" the reference scenes use, drawn from the kit's
        // Boxes sheet (not procedural). Decoration only (no collider). Deterministic per block.
        public static void PlaceGroundProps(GameObject block, float w, float h, Material unlit)
        {
            if (w < 10f || h < 2f) return;
            var bsr = block.GetComponent<SpriteRenderer>();
            string layer = bsr != null ? bsr.sortingLayerName : "Environment";
            int order = (bsr != null ? bsr.sortingOrder : 0) + 3;

            UnityEngine.Random.State prev = UnityEngine.Random.state;
            Vector3 p = block.transform.position;
            UnityEngine.Random.InitState(Mathf.RoundToInt(p.x * 7.3f + p.y * 3.1f + w) + 555);

            int n = Mathf.Clamp(Mathf.RoundToInt(w / 18f), 1, 3);
            for (int i = 0; i < n; i++)
            {
                var sp = GetBox(UnityEngine.Random.Range(0, 16));
                if (sp == null) continue;
                float scale = UnityEngine.Random.Range(0.75f, 1.05f);  // box sprite is 1u @ PPU 32
                var d = SpriteGO("Prop", sp, layer, order, block.transform, unlit, Color.white);
                float x = UnityEngine.Random.Range(-w * 0.5f + 1.2f, w * 0.5f - 1.2f);
                d.transform.localPosition = new Vector3(x, h * 0.5f + scale * 0.5f - 0.06f, -0.02f);
                d.transform.localScale = new Vector3((UnityEngine.Random.value > 0.5f ? 1f : -1f) * scale, scale, 1f);
            }
            UnityEngine.Random.state = prev;
        }

        // The CraftPix reference backdrop is ONE connected pattern tiled to FILL the whole
        // space — a continuous grey facility wall (bolted panels + grid lines), mostly dark
        // with thin 1-Bit line-work, NOT scattered pieces and NOT random noise. We tile one
        // seamless facility-panel tile (bgpanel) across the level as that connected fill, dim
        // so the white terrain stays the foreground, then layer a few brighter kit pipe/panel
        // accents + far moons + a starfield on top for depth. All on the BACKGROUND layer
        // (behind terrain). Deterministic (seeded).
        public static void BuildAtmosphere(Transform bgParent, Transform cameraTransform, float worldWidth, Material unlit)
        {
            UnityEngine.Random.State prev = UnityEngine.Random.state;
            UnityEngine.Random.InitState(Mathf.RoundToInt(worldWidth * 101f) + 9173);

            // CONNECTED WALL — a dim diagonal-girder facility wall painted from the kit's REAL
            // diagonal-hatch tile onto a background Tilemap (with black gaps so it reads as a
            // built wall, not a uniform fill). Replaces the old procedural bgpanel sprite.
            EchoTilemap.BuildBackgroundWall(bgParent, cameraTransform, worldWidth, unlit);

            // (Structure — pipes/panels — is now woven into the BgWall tilemap above, so the
            // old separate scattered-accent layer was removed to avoid doubling up.)

            // PLANETS — the kit's dithered moon tile (variant 4), dim + far, a couple.
            var sky = new GameObject("BG_Sky");
            sky.transform.SetParent(bgParent, false);
            var skyP = sky.AddComponent<Parallax>(); skyP.factor = 0.1f; skyP.cameraTransform = cameraTransform;
            int planets = worldWidth > 90f ? 3 : 2;
            for (int i = 0; i < planets; i++)
            {
                var d = SpriteGO("Planet", GetBackgroundVariant(4), "Background", -9, sky.transform, unlit,
                    new Color(1f, 1f, 1f, UnityEngine.Random.Range(0.30f, 0.50f)));
                d.transform.localPosition = new Vector3(UnityEngine.Random.Range(0.1f, 0.9f) * worldWidth, UnityEngine.Random.Range(7f, 12f), 11f);
                // PPU unified at 32 (was 80 for this sheet) → 80px = 2.5u native; 0.72–1.44
                // keeps the old on-screen planet size (was 1.8–3.6 at the old 1u native).
                d.transform.localScale = Vector3.one * UnityEngine.Random.Range(0.72f, 1.44f);
            }

            // STARFIELD — sparse white dots, varied brightness, mostly in the sky.
            var stars = new GameObject("BG_Stars");
            stars.transform.SetParent(bgParent, false);
            var starP = stars.AddComponent<Parallax>(); starP.factor = 0.08f; starP.cameraTransform = cameraTransform;
            int dots = Mathf.Clamp(Mathf.RoundToInt(worldWidth * 0.5f), 16, 80);
            for (int i = 0; i < dots; i++)
            {
                var d = SpriteGO("Star", LoadSprite("bgdot"), "Background", -10, stars.transform, unlit,
                    new Color(1f, 1f, 1f, UnityEngine.Random.Range(0.25f, 0.85f)));
                d.transform.localPosition = new Vector3(UnityEngine.Random.Range(-4f, worldWidth + 4f), UnityEngine.Random.Range(3.5f, 13f), 10f);
                d.transform.localScale = Vector3.one * UnityEngine.Random.Range(0.18f, 0.48f);
            }

            UnityEngine.Random.state = prev;
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

        // Closed rounded-rectangle frame using the procedural "frame" sliced sprite.
        // One Image stretched to fill the parent canvas with optional edge inset.
        public static GameObject AddSceneFrame(Transform parent, Color color, float inset = 6f)
        {
            var go = new GameObject("SceneFrame");
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.sprite = LoadSprite("frame");
            img.type = Image.Type.Sliced;
            img.color = color;
            img.raycastTarget = false;
            FullStretch(img.rectTransform);
            img.rectTransform.offsetMin = new Vector2(inset, inset);
            img.rectTransform.offsetMax = new Vector2(-inset, -inset);
            return go;
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

        // World-space environmental display: 9-sliced rounded panel + Orbitron text + outline.
        // Reads like a tactical/comms screen rather than a stretched lab tile.
        public static GameObject CreateWallNarrative(Transform parent, Vector3 pos, string text, Color? color = null, bool flicker = true)
        {
            var root = new GameObject("WallNarrative");
            if (parent != null) root.transform.SetParent(parent, false);
            root.transform.position = pos;

            // Tactical panel — the procedural button sprite (rounded rect + cyan border) sliced to size.
            var panel = SpriteGO("Panel", LoadSprite("button"), "Midground", 2, root.transform,
                LoadMaterial(EchoMaterials.UnlitName), new Color(0.03f, 0.08f, 0.14f, 0.88f));
            var psr = panel.GetComponent<SpriteRenderer>();
            psr.drawMode = SpriteDrawMode.Sliced;
            psr.size = new Vector2(7.6f, 2.6f);

            var tgo = new GameObject("Text");
            tgo.transform.SetParent(root.transform, false);
            tgo.transform.localPosition = new Vector3(0f, 0f, -0.02f);
            var tmp = tgo.AddComponent<TextMeshPro>();
            tmp.text = text;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 2.1f;
            tmp.color = color ?? new Color(0.75f, 0.95f, 1f, 1f);
            if (TitleFont != null) tmp.font = TitleFont;
            else if (CustomFont != null) tmp.font = CustomFont;
            tmp.fontStyle = FontStyles.Bold;
            tmp.characterSpacing = 4f;
            tmp.rectTransform.sizeDelta = new Vector2(7.0f, 2.4f);
            var mr = tgo.GetComponent<MeshRenderer>();
            if (mr != null) { mr.sortingLayerID = SortingLayer.NameToID("Foreground"); mr.sortingOrder = 3; }

            ApplyOutline(tmp, new Color(0f, 0.05f, 0.12f, 0.98f), 0.22f);

            if (flicker)
            {
                var fl = root.AddComponent<Flicker>();
                fl.spriteTarget = psr;
                fl.min = 0.78f;
                fl.max = 1f;
            }
            return root;
        }
    }
}
