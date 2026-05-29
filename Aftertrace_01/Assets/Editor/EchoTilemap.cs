using EchoShift;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace EchoShift.EditorTools
{
    /// <summary>
    /// Code-painted Tilemap for terrain VISUALS, assembled from the CraftPix tileset's REAL
    /// rock tiles (organic top crest / dithered body / rocky bottom edge) — the kit's intended
    /// workflow, which a single tiled SpriteRenderer can't reproduce. Build All paints it from
    /// code (tilemap.SetTile), so there is NO manual Tile Palette work.
    ///
    /// Collision is deliberately NOT on the tilemap: each block keeps its own BoxCollider2D
    /// (see SolidBlock), so gameplay/physics are byte-for-byte unchanged — the tilemap only
    /// replaces the old tiled sprite + procedural white edge.
    /// </summary>
    public static class EchoTilemap
    {
        public const string TileDir = "Assets/Art/Tiles";
        const string TilesetPath = "Assets/Art/Imported/CraftPix1Bit/Tileset/Tileset.png";
        const float Cell = 0.5f;   // 16px tile @ PPU 32 = 0.5 world units

        // Rock terrain tiles, identified from the index-labelled Tileset.png map:
        // 6/7/8 = rocky top surface (air above, solid below); 23/24/25 = dithered rock body
        // (varying dot density); 40/41/42 = rocky bottom edge.
        static readonly int[] RockTop = { 6, 7, 8 };
        static readonly int[] RockFill = { 23, 24, 25 };
        static readonly int[] RockBottom = { 40, 41, 42 };

        static Tilemap terrain;

        /// <summary>Create the shared terrain Grid+Tilemap for the current level. Call once
        /// before any PaintSolid. mat = the 1-Bit (unlit) material so terrain renders pure white.</summary>
        public static Tilemap BeginTerrain(Transform parent, Material mat)
        {
            var gridGO = new GameObject("TerrainGrid");
            gridGO.transform.SetParent(parent, false);
            var grid = gridGO.AddComponent<Grid>();
            grid.cellSize = new Vector3(Cell, Cell, 0f);

            var tmGO = new GameObject("TerrainTilemap");
            tmGO.transform.SetParent(gridGO.transform, false);
            var tm = tmGO.AddComponent<Tilemap>();
            var tr = tmGO.AddComponent<TilemapRenderer>();
            tr.sortingLayerName = "Environment";
            tr.sortingOrder = 0;
            if (mat != null) tr.sharedMaterial = mat;
            terrain = tm;
            return tm;
        }

        /// <summary>Paint a block's cells with rock tiles: top row = crest, bottom row = base,
        /// middle = dithered body. Snaps to the 0.5u tile grid (collider stays exact).</summary>
        public static void PaintSolid(float cx, float cy, float w, float h)
        {
            if (terrain == null) return;
            int x0 = Mathf.RoundToInt((cx - w * 0.5f) / Cell);
            int y0 = Mathf.RoundToInt((cy - h * 0.5f) / Cell);
            int nx = Mathf.Max(1, Mathf.RoundToInt(w / Cell));
            int ny = Mathf.Max(1, Mathf.RoundToInt(h / Cell));
            for (int j = 0; j < ny; j++)
                for (int i = 0; i < nx; i++)
                {
                    int gx = x0 + i, gy = y0 + j;
                    int idx;
                    if (j == ny - 1) idx = RockTop[Mod(gx, RockTop.Length)];                 // top crest
                    else if (j == 0 && ny > 1) idx = RockBottom[Mod(gx, RockBottom.Length)];  // bottom edge
                    else idx = RockFill[Mod(gx + gy, RockFill.Length)];                       // varied body
                    terrain.SetTile(new Vector3Int(gx, gy, 0), GetTile(idx));
                }
        }

        // The CONNECTED background facility, stitched from the kit's DEDICATED background sheet
        // (Background_n_details, 80×80 = 2.5u tiles): diagonal-line walls as the base, with
        // pipes/panels woven in and ~⅓ black gaps. These tiles are black-with-white-linework,
        // so over the black void they read as recessive grey structure (NOT the bright vertical
        // bars the white Tileset tiles gave). Tinted dim + parallaxed; painted by Build All.
        public static void BuildBackgroundWall(Transform parent, Transform cameraTransform, float worldWidth, Material mat)
        {
            const float BgCell = 2.5f;   // 80px Background_n_details tile @ PPU 32
            var gridGO = new GameObject("BgGrid");
            gridGO.transform.SetParent(parent, false);
            gridGO.AddComponent<Grid>().cellSize = new Vector3(BgCell, BgCell, 0f);
            var px = gridGO.AddComponent<Parallax>(); px.factor = 0.06f; px.cameraTransform = cameraTransform;

            var tmGO = new GameObject("BgWall");
            tmGO.transform.SetParent(gridGO.transform, false);
            var tm = tmGO.AddComponent<Tilemap>();
            tm.color = new Color(1f, 1f, 1f, 0.45f);   // black-based art → ~0.45 keeps white linework as recessive grey
            var tr = tmGO.AddComponent<TilemapRenderer>();
            tr.sortingLayerName = "Background";
            tr.sortingOrder = -12;
            if (mat != null) tr.sharedMaterial = mat;

            // Background_n_details variants: 1 = diagonal-line wall (base); 3 = vertical pipes,
            // 2 = pipe-cross, 7 = cable detail, 5 = tech panel (woven in as structure).
            int[] structure = { 3, 2, 7, 5 };
            var prev = UnityEngine.Random.state;
            UnityEngine.Random.InitState(Mathf.RoundToInt(worldWidth * 53f) + 1234);
            int cx1 = Mathf.CeilToInt(worldWidth / BgCell) + 4;
            for (int gy = -2; gy < 7; gy++)             // world y ≈ -5 .. 17
                for (int gx = -4; gx < cx1; gx++)
                {
                    float r = UnityEngine.Random.value;
                    if (r < 0.30f) continue;                                   // ~30% black gaps
                    int variant = (r < 0.80f) ? 1                              // diagonal-line base wall
                                  : structure[UnityEngine.Random.Range(0, structure.Length)];   // woven pipes/panels
                    tm.SetTile(new Vector3Int(gx, gy, 0), GetBgTile(variant));
                }
            UnityEngine.Random.state = prev;
        }

        // Wrap a Background_n_details frame in a persistent Tile asset (idempotent, cached).
        static Tile GetBgTile(int index)
        {
            string frame = $"Background_n_details_{index}";
            string path = $"{TileDir}/{frame}.asset";
            var t = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (t != null) return t;
            EchoBuildUtils.EnsureFolder(TileDir);
            t = ScriptableObject.CreateInstance<Tile>();
            t.sprite = EchoBuildUtils.LoadImportedSprite(
                "Assets/Art/Imported/CraftPix1Bit/Tileset/Background_n_details.png", frame);
            t.colliderType = Tile.ColliderType.None;
            AssetDatabase.CreateAsset(t, path);
            return t;
        }

        static int Mod(int a, int m) => ((a % m) + m) % m;

        // Wrap a sliced Tileset sprite in a persistent Tile asset (idempotent, cached on disk).
        static Tile GetTile(int index)
        {
            string frame = $"Tileset_{index}";
            string path = $"{TileDir}/{frame}.asset";
            var t = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (t != null) return t;
            EchoBuildUtils.EnsureFolder(TileDir);
            t = ScriptableObject.CreateInstance<Tile>();
            t.sprite = EchoBuildUtils.LoadImportedSprite(TilesetPath, frame);
            t.colliderType = Tile.ColliderType.None;   // collision is on the BoxColliders, not here
            AssetDatabase.CreateAsset(t, path);
            return t;
        }
    }
}
