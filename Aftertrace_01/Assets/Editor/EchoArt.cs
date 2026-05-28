using System.IO;
using UnityEditor;
using UnityEngine;

namespace EchoShift.EditorTools
{
    /// <summary>
    /// Procedurally draws every sprite pixel-by-pixel into Texture2D, saves PNGs to
    /// Assets/Art/Sprites/, and configures each importer (Sprite, PPU, filter, pivot).
    /// </summary>
    public static class EchoArt
    {
        public static void GenerateAll()
        {
            EchoBuildUtils.EnsureFolder(EchoBuildUtils.SpriteDir);

            // External CraftPix 1-Bit sprites supplant these — generators are kept
            // as fallback (and as commit-history rollback) but no longer regenerate:
            //   BuildPlayer / BuildEcho / BuildPlatform / BuildDoor /
            //   BuildDrone / BuildEndArch / BuildFragment
            BuildPlate();
            BuildParticle();
            BuildGlow();
            BuildRing();
            BuildBgDot();
            BuildBgEquip();
            BuildKeycap();
            BuildVignette();
            BuildCone();
            BuildDiamondFilled();
            BuildDiamondOutline();
            BuildButtonBg();
            BuildFrameBorder();
            BuildArrow();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        // ---------------------------------------------------------------- sprites

        static void BuildPlayer()
        {
            var c = new Canvas(32, 32);
            Color body = EchoBuildUtils.ColPlayer;
            Color feet = body * 0.55f; feet.a = 1f;
            Color visor = EchoBuildUtils.ColPlayerCore;

            // feet
            c.RoundedBox(11f, 3.5f, 2.6f, 2.6f, 1f, feet);
            c.RoundedBox(20f, 3.5f, 2.6f, 2.6f, 1f, feet);
            // body
            c.RoundedBox(16f, 17f, 10f, 12f, 6f, body);
            // edge highlight (top)
            Color edge = Color.Lerp(body, Color.white, 0.4f); edge.a = 0.5f;
            c.RoundedBoxOutline(16f, 17f, 10f, 12f, 6f, edge);
            // soft inner core glow
            c.Glow(16f, 18f, 7f, new Color(visor.r, visor.g, visor.b, 0.45f), 1.6f);
            // visor bright bar
            c.RoundedBox(16f, 22f, 7.5f, 1.7f, 1.2f, visor);

            Save(c, "player", 32, FilterMode.Point);
        }

        static void BuildEcho()
        {
            var c = new Canvas(32, 32);
            Color body = EchoBuildUtils.ColEcho;
            // soft outer feather
            c.Glow(16f, 17f, 13f, new Color(body.r, body.g, body.b, 0.28f), 2.2f);
            // feet
            Color feet = body * 0.8f; feet.a = 1f;
            c.RoundedBox(11f, 3.5f, 2.6f, 2.6f, 1f, feet);
            c.RoundedBox(20f, 3.5f, 2.6f, 2.6f, 1f, feet);
            // body (slightly soft via outline blend)
            c.RoundedBox(16f, 17f, 10f, 12f, 6.5f, body);
            Color core = Color.Lerp(body, Color.white, 0.6f);
            c.Glow(16f, 18f, 6f, new Color(core.r, core.g, core.b, 0.5f), 1.5f);
            c.RoundedBox(16f, 22f, 7f, 1.6f, 1.1f, core);

            Save(c, "echo", 32, FilterMode.Point);
        }

        static void BuildPlatform()
        {
            var c = new Canvas(32, 32);
            c.FilledRect(0, 0, 32, 32, EchoBuildUtils.ColPlatform);
            // 1px lighter border to read as a lab panel
            c.RectBorder(0, 0, 32, 32, 1, EchoBuildUtils.ColPlatformEdge);
            // subtle inner panel inset
            Color inset = EchoBuildUtils.ColPlatformEdge; inset.a = 0.4f;
            c.RectBorder(4, 4, 24, 24, 1, inset);
            Save(c, "platform", 32, FilterMode.Point);
        }

        static void BuildPlate()
        {
            // Near-white so runtime tint (amber/green) shows cleanly.
            var c = new Canvas(48, 8);
            Color bar = new Color(0.92f, 0.94f, 0.96f, 1f);
            c.RoundedBox(24f, 4f, 23f, 3.4f, 3.2f, bar);
            Color top = Color.Lerp(bar, Color.white, 0.6f); top.a = 0.7f;
            c.RoundedBox(24f, 5f, 21f, 1f, 1f, top);
            Save(c, "plate", 32, FilterMode.Bilinear);
        }

        static void BuildDoor()
        {
            var c = new Canvas(16, 64);
            c.FilledRect(2, 0, 12, 64, EchoBuildUtils.ColDoor);
            Color edge = Color.Lerp(EchoBuildUtils.ColDoor, Color.white, 0.25f);
            c.RectBorder(2, 0, 12, 64, 1, edge);
            // center seam
            Color seam = EchoBuildUtils.ColDoor * 0.7f; seam.a = 1f;
            c.FilledRect(7, 0, 2, 64, seam);
            // horizontal panel lines
            Color line = edge; line.a = 0.5f;
            for (int y = 8; y < 64; y += 12) c.FilledRect(2, y, 12, 1, line);
            Save(c, "door", 32, FilterMode.Point);
        }

        static void BuildFragment()
        {
            var c = new Canvas(16, 16);
            Color gold = EchoBuildUtils.ColFragment;
            c.Glow(8f, 8f, 8f, new Color(gold.r, gold.g, gold.b, 0.4f), 2f);
            c.Diamond(8f, 8f, 6f, gold);
            // facet shading: brighter top-left half
            Color bright = Color.Lerp(gold, Color.white, 0.6f);
            c.DiamondHalf(8f, 8f, 5f, bright, true);
            Save(c, "fragment", 32, FilterMode.Point);
        }

        static void BuildEndArch()
        {
            var c = new Canvas(32, 64);
            Color frame = EchoBuildUtils.ColEnd;
            // glow behind
            c.Glow(16f, 30f, 22f, new Color(frame.r, frame.g, frame.b, 0.22f), 2f);
            // pillars
            c.RoundedBox(7f, 26f, 3f, 24f, 1.5f, frame);
            c.RoundedBox(25f, 26f, 3f, 24f, 1.5f, frame);
            // top arc (thick ring upper half)
            c.ArcTop(16f, 50f, 11f, 7.5f, frame);
            // inner soft light
            c.Glow(16f, 34f, 9f, new Color(frame.r, frame.g, frame.b, 0.3f), 1.6f);
            Save(c, "endarch", 32, FilterMode.Point);
        }

        static void BuildParticle()
        {
            var c = new Canvas(8, 8);
            c.Glow(4f, 4f, 4f, new Color(1f, 1f, 1f, 1f), 1.8f);
            Save(c, "particle", 32, FilterMode.Bilinear);
        }

        static void BuildGlow()
        {
            var c = new Canvas(64, 64);
            c.Glow(32f, 32f, 32f, new Color(1f, 1f, 1f, 1f), 2.2f);
            Save(c, "glow", 32, FilterMode.Bilinear);
        }

        static void BuildRing()
        {
            var c = new Canvas(48, 48);
            c.Ring(24f, 24f, 22f, 14f, new Color(0.7f, 0.87f, 1f, 1f));
            Save(c, "ring", 32, FilterMode.Bilinear);
        }

        static void BuildBgDot()
        {
            var c = new Canvas(16, 16);
            c.Glow(8f, 8f, 8f, new Color(0.55f, 0.7f, 1f, 1f), 2.4f);
            Save(c, "bgdot", 32, FilterMode.Bilinear);
        }

        static void BuildBgEquip()
        {
            var c = new Canvas(64, 48);
            Color body = new Color(0.09f, 0.12f, 0.2f, 1f);
            c.FilledRect(0, 0, 64, 40, body);
            Color top = new Color(0.13f, 0.17f, 0.27f, 1f);
            c.FilledRect(8, 40, 20, 8, top);
            c.FilledRect(40, 40, 16, 6, top);
            Color line = new Color(0.2f, 0.26f, 0.4f, 0.6f);
            for (int x = 6; x < 64; x += 12) c.FilledRect(x, 6, 1, 28, line);
            Save(c, "bgequip", 32, FilterMode.Point);
        }

        static void BuildKeycap()
        {
            var c = new Canvas(24, 24);
            Color fill = new Color(0.04f, 0.07f, 0.13f, 0.82f);
            c.RoundedBox(12f, 12f, 10f, 10f, 4f, fill);
            Color border = EchoBuildUtils.ColPlayer;
            c.RoundedBoxOutline(12f, 12f, 10f, 10f, 4f, border);
            c.Glow(12f, 12f, 13f, new Color(border.r, border.g, border.b, 0.25f), 2.2f);
            Save(c, "keycap", 32, FilterMode.Bilinear);
        }

        static void BuildVignette()
        {
            int n = 256;
            var c = new Canvas(n, n);
            Color red = new Color(1f, 0.25f, 0.12f, 1f);
            float half = n * 0.5f;
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float nx = Mathf.Abs((x + 0.5f) - half) / half;
                    float ny = Mathf.Abs((y + 0.5f) - half) / half;
                    float edge = Mathf.Max(nx, ny);
                    float a = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.55f, 1f, edge));
                    c.px[y * n + x] = new Color(red.r, red.g, red.b, a);
                }
            Save(c, "vignette", 100, FilterMode.Bilinear);
        }

        static void BuildDrone()
        {
            var c = new Canvas(32, 32);
            Color body = new Color(1f, 0.36f, 0.16f, 1f);   // threatening red-orange
            Color edge = Color.Lerp(body, Color.white, 0.4f);
            Color eye = new Color(1f, 0.9f, 0.4f, 1f);
            c.Glow(16f, 16f, 14f, new Color(body.r, body.g, body.b, 0.2f), 2.2f);
            c.Diamond(16f, 16f, 11f, body);
            c.DiamondOutline(16f, 16f, 11f, 1.4f, edge);
            c.Glow(16f, 16f, 5f, new Color(eye.r, eye.g, eye.b, 0.9f), 1.4f);
            c.Diamond(16f, 16f, 3f, Color.Lerp(eye, Color.white, 0.5f));
            Save(c, "drone", 32, FilterMode.Point);
        }

        static void BuildCone()
        {
            // Proportioned so the apex (x=0) opens at ~26° half-angle to the mouth, matching
            // PatrolDrone.viewHalfAngle. The prefab scales this uniformly so the visible cone
            // equals the real detection range — what you see is what detects you.
            int w = 48, h = 48;
            var c = new Canvas(w, h);
            Color col = new Color(1f, 0.5f, 0.25f, 1f);
            for (int x = 0; x < w; x++)
            {
                float t = (float)x / (w - 1);
                float halfH = Mathf.Lerp(0.5f, 23f, t);   // tan(26°) ≈ 0.49 → 23px over 48px
                float a = 0.55f * (1f - t * 0.65f);
                for (int y = 0; y < h; y++)
                {
                    float dy = Mathf.Abs(y + 0.5f - h * 0.5f);
                    float cov = Mathf.Clamp01(halfH - dy + 0.5f);
                    if (cov <= 0f) continue;
                    float aa = a * cov;
                    int i = y * w + x;
                    c.px[i] = new Color(col.r, col.g, col.b, Mathf.Max(c.px[i].a, aa));
                }
            }
            Save(c, "cone", 32, FilterMode.Bilinear);
        }

        static void BuildDiamondFilled()
        {
            var c = new Canvas(16, 16);
            c.Glow(8f, 8f, 8f, new Color(1f, 1f, 1f, 0.25f), 2f);
            c.Diamond(8f, 8f, 6f, Color.white);
            Save(c, "diamond_filled", 32, FilterMode.Bilinear);
        }

        static void BuildDiamondOutline()
        {
            var c = new Canvas(16, 16);
            c.DiamondOutline(8f, 8f, 6f, 1.4f, Color.white);
            Save(c, "diamond_outline", 32, FilterMode.Bilinear);
        }

        static void BuildButtonBg()
        {
            var c = new Canvas(32, 32);
            // 1-Bit panel: dark fill + crisp white outline (replaces old cyan border).
            c.RoundedBox(16f, 16f, 15f, 15f, 6f, new Color(0.0f, 0.0f, 0.0f, 0.92f));
            c.RoundedBoxOutline(16f, 16f, 15f, 15f, 6f, new Color(0.92f, 0.96f, 1f, 0.95f));
            SaveSliced(c, "button", 32, 9);
        }

        // Closed rectangular frame — transparent middle + thick white border. Used
        // as a 9-sliced scene-frame Image so it scales to any UI viewport.
        static void BuildFrameBorder()
        {
            var c = new Canvas(48, 48);
            Color line = new Color(0.92f, 0.96f, 1f, 1f);
            // Triple-stroke rounded outline for a chunkier "lab frame" look.
            c.RoundedBoxOutline(24f, 24f, 23f, 23f, 8f, line);
            c.RoundedBoxOutline(24f, 24f, 22f, 22f, 7.5f, line);
            c.RoundedBoxOutline(24f, 24f, 21f, 21f, 7f, line);
            SaveSliced(c, "frame", 32, 14);
        }

        static void BuildArrow()
        {
            var c = new Canvas(16, 16);
            Color col = new Color(0.6f, 0.9f, 1f, 1f);
            c.Glow(8f, 9f, 8f, new Color(col.r, col.g, col.b, 0.3f), 2f);
            for (int y = 6; y <= 13; y++)
            {
                float t = (y - 6) / 7f;
                int half = Mathf.Max(1, Mathf.RoundToInt(Mathf.Lerp(6f, 1f, t)));
                for (int x = 8 - half; x <= 8 + half; x++)
                    if (x >= 0 && x < 16) c.px[y * 16 + x] = col;
            }
            c.FilledRect(6, 1, 4, 6, col);
            Save(c, "arrow", 32, FilterMode.Bilinear);
        }

        // ---------------------------------------------------------------- io

        static void Save(Canvas c, string name, int ppu, FilterMode filter)
        {
            Texture2D tex = c.ToTexture();
            byte[] png = tex.EncodeToPNG();
            Object.DestroyImmediate(tex);

            string path = $"{EchoBuildUtils.SpriteDir}/{name}.png";
            File.WriteAllBytes(path, png);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            var ti = (TextureImporter)AssetImporter.GetAtPath(path);
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = ppu;
            ti.filterMode = filter;
            ti.wrapMode = TextureWrapMode.Clamp;
            ti.alphaIsTransparency = true;
            ti.mipmapEnabled = false;
            ti.textureCompression = TextureImporterCompression.Uncompressed;

            var s = new TextureImporterSettings();
            ti.ReadTextureSettings(s);
            s.spriteAlignment = (int)SpriteAlignment.Center;
            s.spriteMeshType = SpriteMeshType.FullRect;
            ti.SetTextureSettings(s);

            ti.SaveAndReimport();
        }

        static void SaveSliced(Canvas c, string name, int ppu, int border)
        {
            Texture2D tex = c.ToTexture();
            byte[] png = tex.EncodeToPNG();
            Object.DestroyImmediate(tex);

            string path = $"{EchoBuildUtils.SpriteDir}/{name}.png";
            File.WriteAllBytes(path, png);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            var ti = (TextureImporter)AssetImporter.GetAtPath(path);
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = ppu;
            ti.filterMode = FilterMode.Bilinear;
            ti.wrapMode = TextureWrapMode.Clamp;
            ti.alphaIsTransparency = true;
            ti.mipmapEnabled = false;
            ti.textureCompression = TextureImporterCompression.Uncompressed;

            var s = new TextureImporterSettings();
            ti.ReadTextureSettings(s);
            s.spriteAlignment = (int)SpriteAlignment.Center;
            s.spriteMeshType = SpriteMeshType.FullRect;
            s.spriteBorder = new Vector4(border, border, border, border);
            ti.SetTextureSettings(s);

            ti.SaveAndReimport();
        }

        // ---------------------------------------------------------------- canvas

        class Canvas
        {
            public readonly int w;
            public readonly int h;
            public readonly Color[] px;

            public Canvas(int w, int h)
            {
                this.w = w;
                this.h = h;
                px = new Color[w * h]; // default (0,0,0,0)
            }

            public Texture2D ToTexture()
            {
                var t = new Texture2D(w, h, TextureFormat.RGBA32, false);
                t.SetPixels(px);
                t.Apply();
                return t;
            }

            void Blend(int x, int y, Color s)
            {
                if (x < 0 || y < 0 || x >= w || y >= h || s.a <= 0f) return;
                Color d = px[y * w + x];
                float a = s.a + d.a * (1f - s.a);
                if (a <= 0.0001f) { px[y * w + x] = new Color(0, 0, 0, 0); return; }
                px[y * w + x] = new Color(
                    (s.r * s.a + d.r * d.a * (1f - s.a)) / a,
                    (s.g * s.a + d.g * d.a * (1f - s.a)) / a,
                    (s.b * s.a + d.b * d.a * (1f - s.a)) / a,
                    a);
            }

            public void FilledRect(int x0, int y0, int rw, int rh, Color col)
            {
                for (int y = y0; y < y0 + rh; y++)
                    for (int x = x0; x < x0 + rw; x++)
                        Blend(x, y, col);
            }

            public void RectBorder(int x0, int y0, int rw, int rh, int t, Color col)
            {
                FilledRect(x0, y0, rw, t, col);
                FilledRect(x0, y0 + rh - t, rw, t, col);
                FilledRect(x0, y0, t, rh, col);
                FilledRect(x0 + rw - t, y0, t, rh, col);
            }

            // Rounded box by signed distance; cx,cy center, halfW/halfH half-extents.
            public void RoundedBox(float cx, float cy, float halfW, float halfH, float r, Color col)
            {
                int x0 = Mathf.FloorToInt(cx - halfW - 1), x1 = Mathf.CeilToInt(cx + halfW + 1);
                int y0 = Mathf.FloorToInt(cy - halfH - 1), y1 = Mathf.CeilToInt(cy + halfH + 1);
                for (int y = y0; y <= y1; y++)
                    for (int x = x0; x <= x1; x++)
                    {
                        float d = BoxSdf((x + 0.5f) - cx, (y + 0.5f) - cy, halfW, halfH, r);
                        float cov = Mathf.Clamp01(0.5f - d);
                        if (cov > 0f) { Color s = col; s.a *= cov; Blend(x, y, s); }
                    }
            }

            public void RoundedBoxOutline(float cx, float cy, float halfW, float halfH, float r, Color col)
            {
                int x0 = Mathf.FloorToInt(cx - halfW - 1), x1 = Mathf.CeilToInt(cx + halfW + 1);
                int y0 = Mathf.FloorToInt(cy - halfH - 1), y1 = Mathf.CeilToInt(cy + halfH + 1);
                for (int y = y0; y <= y1; y++)
                    for (int x = x0; x <= x1; x++)
                    {
                        float d = BoxSdf((x + 0.5f) - cx, (y + 0.5f) - cy, halfW, halfH, r);
                        float edge = 1f - Mathf.Clamp01(Mathf.Abs(d) - 0.1f);
                        if (d < 0.6f && d > -1.4f)
                        {
                            Color s = col; s.a *= Mathf.Clamp01(edge);
                            Blend(x, y, s);
                        }
                    }
            }

            static float BoxSdf(float px, float py, float halfW, float halfH, float r)
            {
                float qx = Mathf.Abs(px) - (halfW - r);
                float qy = Mathf.Abs(py) - (halfH - r);
                float ax = Mathf.Max(qx, 0f), ay = Mathf.Max(qy, 0f);
                float outside = Mathf.Sqrt(ax * ax + ay * ay);
                float inside = Mathf.Min(Mathf.Max(qx, qy), 0f);
                return outside + inside - r;
            }

            public void Glow(float cx, float cy, float radius, Color col, float exp)
            {
                int x0 = Mathf.FloorToInt(cx - radius - 1), x1 = Mathf.CeilToInt(cx + radius + 1);
                int y0 = Mathf.FloorToInt(cy - radius - 1), y1 = Mathf.CeilToInt(cy + radius + 1);
                for (int y = y0; y <= y1; y++)
                    for (int x = x0; x <= x1; x++)
                    {
                        float d = Mathf.Sqrt((x + 0.5f - cx) * (x + 0.5f - cx) + (y + 0.5f - cy) * (y + 0.5f - cy));
                        if (d > radius) continue;
                        float t = 1f - d / radius;
                        Color s = col; s.a *= Mathf.Pow(Mathf.Clamp01(t), exp);
                        Blend(x, y, s);
                    }
            }

            public void Diamond(float cx, float cy, float half, Color col)
            {
                int x0 = Mathf.FloorToInt(cx - half - 1), x1 = Mathf.CeilToInt(cx + half + 1);
                int y0 = Mathf.FloorToInt(cy - half - 1), y1 = Mathf.CeilToInt(cy + half + 1);
                for (int y = y0; y <= y1; y++)
                    for (int x = x0; x <= x1; x++)
                    {
                        float d = (Mathf.Abs(x + 0.5f - cx) + Mathf.Abs(y + 0.5f - cy)) - half;
                        float cov = Mathf.Clamp01(0.5f - d);
                        if (cov > 0f) { Color s = col; s.a *= cov; Blend(x, y, s); }
                    }
            }

            public void DiamondOutline(float cx, float cy, float half, float thickness, Color col)
            {
                int x0 = Mathf.FloorToInt(cx - half - 1), x1 = Mathf.CeilToInt(cx + half + 1);
                int y0 = Mathf.FloorToInt(cy - half - 1), y1 = Mathf.CeilToInt(cy + half + 1);
                for (int y = y0; y <= y1; y++)
                    for (int x = x0; x <= x1; x++)
                    {
                        float d = (Mathf.Abs(x + 0.5f - cx) + Mathf.Abs(y + 0.5f - cy)) - half;
                        float cov = Mathf.Clamp01(thickness * 0.5f + 0.5f - Mathf.Abs(d));
                        if (cov > 0f) { Color s = col; s.a *= cov; Blend(x, y, s); }
                    }
            }

            public void DiamondHalf(float cx, float cy, float half, Color col, bool topLeft)
            {
                int x0 = Mathf.FloorToInt(cx - half - 1), x1 = Mathf.CeilToInt(cx + half + 1);
                int y0 = Mathf.FloorToInt(cy - half - 1), y1 = Mathf.CeilToInt(cy + half + 1);
                for (int y = y0; y <= y1; y++)
                    for (int x = x0; x <= x1; x++)
                    {
                        float dx = x + 0.5f - cx, dy = y + 0.5f - cy;
                        float d = (Mathf.Abs(dx) + Mathf.Abs(dy)) - half;
                        bool side = (dx - dy) < 0f;
                        if (side != topLeft) continue;
                        float cov = Mathf.Clamp01(0.5f - d);
                        if (cov > 0f) { Color s = col; s.a *= cov; Blend(x, y, s); }
                    }
            }

            public void Ring(float cx, float cy, float outerR, float innerR, Color col)
            {
                float mid = (outerR + innerR) * 0.5f;
                float band = (outerR - innerR) * 0.5f;
                int x0 = Mathf.FloorToInt(cx - outerR - 1), x1 = Mathf.CeilToInt(cx + outerR + 1);
                int y0 = Mathf.FloorToInt(cy - outerR - 1), y1 = Mathf.CeilToInt(cy + outerR + 1);
                for (int y = y0; y <= y1; y++)
                    for (int x = x0; x <= x1; x++)
                    {
                        float d = Mathf.Sqrt((x + 0.5f - cx) * (x + 0.5f - cx) + (y + 0.5f - cy) * (y + 0.5f - cy));
                        float t = 1f - Mathf.Clamp01(Mathf.Abs(d - mid) / band);
                        if (t <= 0f) continue;
                        Color s = col; s.a *= t * t;
                        Blend(x, y, s);
                    }
            }

            // Upper-half thick arc for the end gateway.
            public void ArcTop(float cx, float cy, float outerR, float innerR, Color col)
            {
                float mid = (outerR + innerR) * 0.5f;
                float band = (outerR - innerR) * 0.5f;
                int x0 = Mathf.FloorToInt(cx - outerR - 1), x1 = Mathf.CeilToInt(cx + outerR + 1);
                int y0 = Mathf.FloorToInt(cy - 1), y1 = Mathf.CeilToInt(cy + outerR + 1);
                for (int y = y0; y <= y1; y++)
                    for (int x = x0; x <= x1; x++)
                    {
                        if (y + 0.5f < cy) continue;
                        float d = Mathf.Sqrt((x + 0.5f - cx) * (x + 0.5f - cx) + (y + 0.5f - cy) * (y + 0.5f - cy));
                        float t = 1f - Mathf.Clamp01(Mathf.Abs(d - mid) / band);
                        if (t <= 0f) continue;
                        Color s = col; s.a *= Mathf.Clamp01(t * 1.4f);
                        Blend(x, y, s);
                    }
            }
        }
    }
}
