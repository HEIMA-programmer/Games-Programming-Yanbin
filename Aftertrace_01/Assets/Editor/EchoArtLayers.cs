using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace EchoShift.EditorTools
{
    /// <summary>
    /// Builds an empty, correctly-sorted multi-layer Grid in the active scene for hand-authoring
    /// 1-Bit tilemap art OVER the frozen gameplay blockout. Creates NO art — just labelled empty
    /// Tilemap layers with the right Sorting Layer + Order, plus pixel-clamped Parallax on the two
    /// background layers. Idempotent: re-running only adds the layers that are missing.
    ///
    /// Order stack on the shared "Environment" sorting layer (back -> front):
    ///   BlockoutFill (-5, the gray placeholder)  <  Terrain (-4)  <  Deco (-3)  <  entities (0, frozen).
    /// Keeping Terrain/Deco BELOW 0 guarantees hand-art never hides a door / plate / platform.
    /// </summary>
    public static class EchoArtLayers
    {
        const float Cell = 0.5f; // 16px tile @ PPU 32 = 0.5 world units

        // name, sortingLayer, order, parallaxFactor (<= 0 means no Parallax component)
        static readonly (string name, string layer, int order, float parallax)[] Layers =
        {
            ("BG_Far",     "Background",  0,  0.15f),
            ("BG_Near",    "Midground",   0,  0.35f),
            ("Terrain",    "Environment", -4, 0f),
            ("Deco",       "Environment", -3, 0f),
            ("Foreground", "Foreground",  0,  0f),
        };

        [MenuItem("Aftertrace/Art/Create Art Layer Skeleton", false, 101)]
        public static void Create()
        {
            EchoBuildUtils.EnsureSortingLayers(); // guarantee Background..UI exist before we assign them

            var scene = EditorSceneManager.GetActiveScene();
            var gridGo = GameObject.Find("ArtGrid");
            if (gridGo == null)
            {
                gridGo = new GameObject("ArtGrid");
                var grid = gridGo.AddComponent<Grid>();
                grid.cellSize = new Vector3(Cell, Cell, 0f);
                Undo.RegisterCreatedObjectUndo(gridGo, "Create ArtGrid");
            }

            // Flat 1-Bit: art layers must be UNLIT so 2D lights never tint them (cyan stays on
            // the player/Echo only). Without this the tilemap uses Sprite-Lit-Default and the
            // terrain reads cyan near lights / black in the dark.
            var unlit = EchoBuildUtils.LoadMaterial(EchoMaterials.UnlitName);

            int added = 0;
            foreach (var L in Layers)
            {
                if (gridGo.transform.Find(L.name) != null) continue; // already there

                var go = new GameObject(L.name);
                go.transform.SetParent(gridGo.transform, false);
                go.AddComponent<Tilemap>();
                var tr = go.AddComponent<TilemapRenderer>();
                tr.sortingLayerName = L.layer;
                tr.sortingOrder = L.order;
                if (unlit != null) tr.sharedMaterial = unlit;

                if (L.parallax > 0f)
                {
                    var p = go.AddComponent<EchoShift.Parallax>();
                    p.factor = L.parallax;
                    p.pixelsPerUnit = 32;
                }
                Undo.RegisterCreatedObjectUndo(go, "Create art layer " + L.name);
                added++;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log($"[Aftertrace/Art] ArtGrid ready (cell {Cell}). Added {added} layer(s). " +
                      "Stack: BG_Far->Background, BG_Near->Midground, Terrain->Environment(-4), " +
                      "Deco->Environment(-3), Foreground->Foreground. Paint Terrain to cover the gray " +
                      "BlockoutFill; entities stay on top automatically. SAVE the scene to keep it.");
        }
    }
}
