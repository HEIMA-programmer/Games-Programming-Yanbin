using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EchoShift.EditorTools
{
    /// <summary>
    /// EditorWindow that browses every sliced sprite under Assets/Art/Imported/,
    /// drawing each frame as a thumbnail. Click a thumbnail to copy its sprite name
    /// to the clipboard so it can be pasted into EchoBuildUtils.ImportedSprites.
    /// Run after Build All (so spritesheets are sliced).
    /// </summary>
    public class EchoSpritePicker : EditorWindow
    {
        [MenuItem("Aftertrace/Sprite Picker", false, 35)]
        public static void Open()
        {
            var w = GetWindow<EchoSpritePicker>("Sprite Picker");
            w.minSize = new Vector2(400, 300);
        }

        const string Root = "Assets/Art/Imported/";
        Vector2 scroll;
        string filter = "";
        float thumbSize = 64f;
        readonly Dictionary<string, bool> foldouts = new Dictionary<string, bool>();

        void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "Click any frame to copy its sprite name. Paste into EchoBuildUtils.ImportedSprites " +
                "(e.g. \"Tileset_18\"). Re-run Build All after editing the mapping.",
                MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            filter = EditorGUILayout.TextField("Search name:", filter);
            thumbSize = EditorGUILayout.Slider("Thumb size", thumbSize, 24f, 128f);
            EditorGUILayout.EndHorizontal();

            scroll = EditorGUILayout.BeginScrollView(scroll);

            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { Root.TrimEnd('/') });
            System.Array.Sort(guids, (a, b) =>
                string.Compare(AssetDatabase.GUIDToAssetPath(a), AssetDatabase.GUIDToAssetPath(b)));

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                DrawSheet(path);
            }

            EditorGUILayout.EndScrollView();
        }

        void DrawSheet(string path)
        {
            var all = AssetDatabase.LoadAllAssetsAtPath(path);
            var sprites = new List<Sprite>();
            foreach (var a in all) if (a is Sprite s) sprites.Add(s);
            if (sprites.Count == 0) return;

            string relName = path.Substring(Root.Length);
            if (!foldouts.ContainsKey(path)) foldouts[path] = true;
            foldouts[path] = EditorGUILayout.Foldout(foldouts[path],
                $"{relName} — {sprites.Count} sprite(s)", true, EditorStyles.foldoutHeader);
            if (!foldouts[path]) return;

            float cell = thumbSize + 18f;
            int cols = Mathf.Max(1, Mathf.FloorToInt((position.width - 40f) / cell));

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.BeginHorizontal();
            int colInRow = 0;
            foreach (var s in sprites)
            {
                if (!string.IsNullOrEmpty(filter)
                    && s.name.IndexOf(filter, System.StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                DrawSpriteCell(s);
                colInRow++;
                if (colInRow >= cols)
                {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    colInRow = 0;
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(4);
        }

        void DrawSpriteCell(Sprite s)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(thumbSize + 4f));
            var rect = GUILayoutUtility.GetRect(thumbSize, thumbSize,
                GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));

            // Checkered background so transparent 1-bit sprites are visible.
            EditorGUI.DrawRect(rect, new Color(0.15f, 0.15f, 0.2f, 1f));

            if (Event.current.type == EventType.Repaint && s.texture != null)
            {
                var tr = s.textureRect;
                var uv = new Rect(
                    tr.x / s.texture.width,
                    tr.y / s.texture.height,
                    tr.width / s.texture.width,
                    tr.height / s.texture.height);
                GUI.DrawTextureWithTexCoords(rect, s.texture, uv, true);
            }

            if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
            {
                EditorGUIUtility.systemCopyBuffer = s.name;
                Debug.Log($"[SpritePicker] Copied: {s.name}");
            }

            var label = s.name.Length > 12 ? s.name.Substring(s.name.Length - 12) : s.name;
            EditorGUILayout.LabelField(label, EditorStyles.miniLabel, GUILayout.Width(thumbSize + 4f));
            EditorGUILayout.EndVertical();
        }
    }
}
