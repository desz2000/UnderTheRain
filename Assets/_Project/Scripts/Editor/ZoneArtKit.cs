using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnderTheRain.Editor
{
    /// <summary>
    /// Procedural pixel art for a zone, generated inside Unity from ONE colour ramp so that the backdrop,
    /// the floor/wall masses and their lit edges always belong together (one art pattern per zone).
    /// The kit paints small tileable textures (32 px per unit), imports them as sprites and skins the
    /// logical rectangles the zone setup builds: dark masses, a lit cap on every exposed top, edge
    /// lines on exposed sides, a tiled back wall with pilasters and lamps with a soft glow.
    /// Colliders are untouched: art is added as children of the solids.
    /// </summary>
    internal static class ZoneArtKit
    {
        public const int Ppu = 32;
        private const float Px = 1f / Ppu;
        private const float CapHeight = 8 * Px;
        private const float EdgeWidth = 4 * Px;
        private const float UnderHeight = 4 * Px;
        private const float OneWayHeight = 10 * Px;
        private const float Eps = 0.01f;

        public enum SolidKind
        {
            /// <summary>Floors, walls, ceilings: the dark mass of the level.</summary>
            Mass,
            /// <summary>Ledges, slabs, steps: lighter built structure standing in the room.</summary>
            Structure,
            /// <summary>Doors, gates, machines: bevelled panel tinted with the object's own colour.</summary>
            Panel
        }

        /// <summary>One hue family from darkest (0) to brightest (6), plus the two light accents.</summary>
        public sealed class Palette
        {
            public Color32[] ramp;
            public Color32 warm;
            public Color32 cool;
        }

        /// <summary>Prologue (zone 0): cold slate-teal underground, amber station lamps, cyan lab light.</summary>
        public static readonly Palette ProloguePalette = new Palette
        {
            ramp = new[]
            {
                Hex(0x0A1116), Hex(0x111C22), Hex(0x1A2A31), Hex(0x263E47),
                Hex(0x3A5B65), Hex(0x648C93), Hex(0xA6C7C4)
            },
            warm = Hex(0xF2B45A),
            cool = Hex(0x4ECDC4)
        };

        public static Palette Pal { get; private set; }
        public static Sprite Mass { get; private set; }
        public static Sprite Block { get; private set; }
        public static Sprite Cap { get; private set; }
        public static Sprite Edge { get; private set; }
        public static Sprite Under { get; private set; }
        public static Sprite BackWall { get; private set; }
        public static Sprite BackWallLab { get; private set; }
        public static Sprite Pillar { get; private set; }
        public static Sprite OneWay { get; private set; }
        public static Sprite Glow { get; private set; }
        public static Sprite Panel { get; private set; }
        public static Sprite Fade { get; private set; }
        public static Sprite Arch { get; private set; }

        private struct Solid
        {
            public GameObject go;
            public Rect rect;
            public SolidKind kind;
            public Color tint;
        }

        private static readonly List<Solid> s_solids = new List<Solid>();

        // ───────────────────────────── generation ─────────────────────────────

        /// <summary>Paints (or repaints) every texture of the kit and loads the sprites.</summary>
        public static void EnsureKit(string directory, Palette palette)
        {
            Pal = palette;
            s_solids.Clear();
            Directory.CreateDirectory(directory);

            Mass = Save(directory, "Mass", PaintMass(palette));
            Block = Save(directory, "Block", PaintBlock(palette));
            Cap = Save(directory, "Cap", PaintCap(palette));
            Edge = Save(directory, "Edge", PaintEdge(palette));
            Under = Save(directory, "Under", PaintUnder(palette));
            BackWall = Save(directory, "BackWall", PaintBackWall(palette));
            BackWallLab = Save(directory, "BackWallLab", PaintBackWallLab(palette));
            Pillar = Save(directory, "Pillar", PaintPillar(palette));
            OneWay = Save(directory, "OneWay", PaintOneWay(palette));
            Glow = Save(directory, "Glow", PaintGlow());
            Panel = Save(directory, "Panel", PaintPanel());
            Fade = Save(directory, "Fade", PaintFade());
            Arch = Save(directory, "Arch", PaintArch(palette));
        }

        private sealed class Canvas
        {
            public readonly int W;
            public readonly int H;
            public readonly Color32[] Pixels;

            public Canvas(int w, int h, Color32 fill)
            {
                W = w;
                H = h;
                Pixels = new Color32[w * h];
                for (int i = 0; i < Pixels.Length; i++) Pixels[i] = fill;
            }

            /// <summary>Sets a pixel; y is measured from the TOP row. Coordinates wrap (tileable painting).</summary>
            public void Set(int x, int yTop, Color32 c)
            {
                x = ((x % W) + W) % W;
                yTop = ((yTop % H) + H) % H;
                Pixels[(H - 1 - yTop) * W + x] = c;
            }

            public Color32 Get(int x, int yTop)
            {
                x = ((x % W) + W) % W;
                yTop = ((yTop % H) + H) % H;
                return Pixels[(H - 1 - yTop) * W + x];
            }

            public void Rect(int x, int yTop, int w, int h, Color32 c)
            {
                for (int j = 0; j < h; j++)
                    for (int i = 0; i < w; i++)
                        Set(x + i, yTop + j, c);
            }
        }

        private static Color32 Hex(int rgb) => new Color32((byte)(rgb >> 16), (byte)((rgb >> 8) & 0xFF), (byte)(rgb & 0xFF), 255);

        private static Color32 Mix(Color32 a, Color32 b, float t) => Color32.Lerp(a, b, t);

        private static readonly Color32 Clear = new Color32(0, 0, 0, 0);

        /// <summary>Dark masonry of floors and walls: big low-contrast blocks so the mass reads as one shape.</summary>
        private static Canvas PaintMass(Palette p)
        {
            var rng = new System.Random(11);
            Color32 mortar = Mix(p.ramp[0], new Color32(0, 0, 0, 255), 0.45f);
            var c = new Canvas(64, 64, p.ramp[0]);
            for (int row = 0; row < 4; row++)
            {
                int offset = row % 2 == 0 ? 0 : 16;
                for (int col = 0; col < 2; col++)
                {
                    int x = offset + col * 32;
                    int y = row * 16;
                    Color32 fill = Mix(p.ramp[0], p.ramp[1], 0.25f + 0.5f * (float)rng.NextDouble());
                    c.Rect(x, y, 32, 16, fill);
                    c.Rect(x, y, 32, 1, Mix(fill, p.ramp[2], 0.35f));
                    c.Rect(x, y + 15, 32, 1, mortar);
                    c.Rect(x + 31, y, 1, 16, mortar);
                    for (int n = 0; n < 5; n++)
                    {
                        c.Set(x + 2 + rng.Next(27), y + 2 + rng.Next(12), Mix(fill, p.ramp[0], 0.6f));
                    }
                }
            }
            return c;
        }

        /// <summary>Lighter cut stone for ledges, slabs and steps.</summary>
        private static Canvas PaintBlock(Palette p)
        {
            var rng = new System.Random(23);
            var c = new Canvas(32, 32, p.ramp[2]);
            for (int row = 0; row < 2; row++)
            {
                int offset = row % 2 == 0 ? 0 : 8;
                for (int col = 0; col < 2; col++)
                {
                    int x = offset + col * 16;
                    int y = row * 16;
                    Color32 fill = Mix(p.ramp[2], p.ramp[3], 0.3f + 0.6f * (float)rng.NextDouble());
                    c.Rect(x, y, 16, 16, fill);
                    c.Rect(x, y, 16, 1, Mix(fill, p.ramp[4], 0.55f));
                    c.Rect(x, y, 1, 16, Mix(fill, p.ramp[4], 0.3f));
                    c.Rect(x, y + 15, 16, 1, p.ramp[1]);
                    c.Rect(x + 15, y, 1, 16, p.ramp[1]);
                    for (int n = 0; n < 3; n++)
                    {
                        c.Set(x + 2 + rng.Next(12), y + 3 + rng.Next(10), Mix(fill, p.ramp[1], 0.5f));
                    }
                }
            }
            return c;
        }

        /// <summary>Lit lip on every walkable top: the brightest line of the ramp, then a band of slabs.</summary>
        private static Canvas PaintCap(Palette p)
        {
            var rng = new System.Random(37);
            var c = new Canvas(64, 8, p.ramp[3]);
            for (int x = 0; x < 64; x++)
            {
                c.Set(x, 0, rng.Next(9) == 0 ? p.ramp[5] : p.ramp[6]);
                c.Set(x, 1, p.ramp[5]);
                c.Set(x, 2, p.ramp[4]);
                c.Set(x, 3, rng.Next(6) == 0 ? p.ramp[3] : p.ramp[4]);
                c.Set(x, 4, p.ramp[3]);
                c.Set(x, 5, p.ramp[3]);
                c.Set(x, 6, p.ramp[2]);
                c.Set(x, 7, p.ramp[1]);
            }
            for (int joint = 0; joint < 64; joint += 16)
            {
                int x = joint + (joint == 32 ? 3 : 0);
                for (int y = 2; y < 7; y++) c.Set(x, y, p.ramp[2]);
            }
            return c;
        }

        /// <summary>Lit line on exposed vertical faces (drawn for a face that looks LEFT; flipped for the right).</summary>
        private static Canvas PaintEdge(Palette p)
        {
            var c = new Canvas(4, 64, p.ramp[1]);
            for (int y = 0; y < 64; y++)
            {
                bool joint = y % 16 == 15;
                c.Set(0, y, joint ? p.ramp[2] : p.ramp[4]);
                c.Set(1, y, joint ? p.ramp[1] : p.ramp[3]);
                c.Set(2, y, joint ? p.ramp[1] : p.ramp[2]);
                c.Set(3, y, p.ramp[1]);
            }
            return c;
        }

        /// <summary>Shadowed underside of ceilings and ledges.</summary>
        private static Canvas PaintUnder(Palette p)
        {
            Color32 deep = Mix(p.ramp[0], new Color32(0, 0, 0, 255), 0.5f);
            var c = new Canvas(64, 4, p.ramp[1]);
            for (int x = 0; x < 64; x++)
            {
                c.Set(x, 0, p.ramp[1]);
                c.Set(x, 1, p.ramp[1]);
                c.Set(x, 2, x % 16 == 0 ? deep : p.ramp[2]);
                c.Set(x, 3, deep);
            }
            return c;
        }

        /// <summary>Station back wall: ceramic tiles of the same ramp, a trim band, missing tiles, cracks and drips.</summary>
        private static Canvas PaintBackWall(Palette p)
        {
            var rng = new System.Random(53);
            Color32 grout = p.ramp[1];
            var c = new Canvas(256, 256, p.ramp[2]);
            for (int ty = 0; ty < 16; ty++)
            {
                for (int tx = 0; tx < 16; tx++)
                {
                    int x = tx * 16;
                    int y = ty * 16;
                    bool band = ty == 5;
                    bool missing = !band && rng.Next(100) < 3;
                    Color32 fill;
                    if (missing) fill = Mix(p.ramp[1], p.ramp[0], 0.2f);
                    else if (band) fill = Mix(p.ramp[2], p.ramp[3], 0.75f);
                    else fill = Mix(Mix(p.ramp[1], p.ramp[2], 0.55f), p.ramp[2], 0.9f * (float)rng.NextDouble());

                    c.Rect(x, y, 16, 16, fill);
                    if (!missing)
                    {
                        c.Rect(x + 1, y + 1, 14, 1, Mix(fill, p.ramp[4], 0.12f));
                        c.Rect(x + 1, y + 14, 14, 1, Mix(fill, p.ramp[1], 0.4f));
                    }
                    else
                    {
                        for (int n = 0; n < 10; n++) c.Set(x + 1 + rng.Next(14), y + 1 + rng.Next(14), p.ramp[1]);
                    }
                    c.Rect(x, y, 16, 1, grout);
                    c.Rect(x, y, 1, 16, grout);
                }
            }

            // Hairline cracks wandering down from random tiles.
            for (int k = 0; k < 7; k++)
            {
                int x = rng.Next(256);
                int y = rng.Next(256);
                int length = 14 + rng.Next(22);
                for (int i = 0; i < length; i++)
                {
                    c.Set(x, y, p.ramp[1]);
                    y++;
                    x += rng.Next(3) - 1;
                }
            }

            // Grime dripping from under the trim band.
            for (int k = 0; k < 16; k++)
            {
                int x = rng.Next(256);
                int length = 4 + rng.Next(18);
                for (int i = 0; i < length; i++)
                {
                    c.Set(x, 96 + i, Mix(c.Get(x, 96 + i), p.ramp[0], 0.3f));
                }
            }
            return c;
        }

        /// <summary>Lab back wall: riveted panels of the same ramp with a dim cyan service line.</summary>
        private static Canvas PaintBackWallLab(Palette p)
        {
            var rng = new System.Random(71);
            var c = new Canvas(128, 128, p.ramp[2]);
            for (int row = 0; row < 4; row++)
            {
                int offset = row % 2 == 0 ? 0 : 32;
                for (int col = 0; col < 2; col++)
                {
                    int x = offset + col * 64;
                    int y = row * 32;
                    Color32 fill = Mix(Mix(p.ramp[1], p.ramp[2], 0.6f), p.ramp[2], 0.9f * (float)rng.NextDouble());
                    c.Rect(x, y, 64, 32, fill);
                    c.Rect(x + 1, y + 1, 62, 1, Mix(fill, p.ramp[4], 0.16f));
                    c.Rect(x, y + 31, 64, 1, p.ramp[1]);
                    c.Rect(x + 63, y, 1, 32, p.ramp[1]);
                    c.Rect(x, y, 1, 32, Mix(fill, p.ramp[3], 0.25f));
                    foreach (int rx in new[] { 3, 59 })
                    {
                        foreach (int ry in new[] { 4, 26 })
                        {
                            c.Set(x + rx, y + ry, p.ramp[4]);
                            c.Set(x + rx, y + ry + 1, p.ramp[1]);
                        }
                    }
                    if (rng.Next(3) == 0)
                    {
                        for (int v = 0; v < 5; v++) c.Rect(x + 22, y + 9 + v * 3, 20, 1, p.ramp[1]);
                    }
                }
            }

            // Service conduit with dashes of the lab's cyan, kept dim so it stays in the background.
            c.Rect(0, 60, 128, 6, p.ramp[1]);
            c.Rect(0, 60, 128, 1, p.ramp[3]);
            for (int x = 4; x < 128; x += 16) c.Rect(x, 62, 8, 2, Mix(p.cool, p.ramp[2], 0.45f));
            return c;
        }

        /// <summary>Background pilaster, a step lighter than the wall so the room gets a rhythm.</summary>
        private static Canvas PaintPillar(Palette p)
        {
            Color32 body = Mix(p.ramp[2], p.ramp[3], 0.4f);
            var c = new Canvas(32, 64, body);
            for (int y = 0; y < 64; y++)
            {
                c.Set(0, y, p.ramp[1]);
                c.Set(1, y, Mix(p.ramp[3], p.ramp[4], 0.3f));
                c.Set(2, y, p.ramp[3]);
                c.Set(28, y, p.ramp[2]);
                c.Set(29, y, p.ramp[2]);
                c.Set(30, y, Mix(p.ramp[1], p.ramp[2], 0.5f));
                c.Set(31, y, p.ramp[1]);
            }
            for (int y = 31; y < 64; y += 32)
            {
                c.Rect(1, y, 30, 1, p.ramp[1]);
                c.Rect(1, y + 1, 30, 1, Mix(body, p.ramp[4], 0.3f));
            }
            return c;
        }

        /// <summary>Thin grating you can jump up through: bright rail on top, open slats below.</summary>
        private static Canvas PaintOneWay(Palette p)
        {
            var c = new Canvas(32, 10, Clear);
            for (int x = 0; x < 32; x++)
            {
                c.Set(x, 0, p.ramp[6]);
                c.Set(x, 1, p.ramp[5]);
                c.Set(x, 2, p.ramp[4]);
                c.Set(x, 3, p.ramp[3]);
                if (x % 4 < 2)
                {
                    for (int y = 4; y < 8; y++) c.Set(x, y, y == 4 ? p.ramp[3] : p.ramp[2]);
                }
                c.Set(x, 8, p.ramp[2]);
                c.Set(x, 9, p.ramp[1]);
            }
            return c;
        }

        /// <summary>Soft round light, stepped so it still reads as pixel art. Tinted by the renderer.</summary>
        private static Canvas PaintGlow()
        {
            var c = new Canvas(128, 128, Clear);
            for (int y = 0; y < 128; y++)
            {
                for (int x = 0; x < 128; x++)
                {
                    float dx = (x - 63.5f) / 64f;
                    float dy = (y - 63.5f) / 64f;
                    float falloff = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy));
                    float stepped = Mathf.Floor(Mathf.Pow(falloff, 2.4f) * 16f) / 16f;
                    if (stepped > 0f) c.Set(x, y, new Color32(255, 255, 255, (byte)(stepped * 255f)));
                }
            }
            return c;
        }

        /// <summary>Neutral bevelled plate; the renderer tints it with each prop's colour.</summary>
        private static Canvas PaintPanel()
        {
            Color32 body = new Color32(196, 196, 196, 255);
            var c = new Canvas(32, 32, body);
            c.Rect(0, 0, 32, 1, new Color32(255, 255, 255, 255));
            c.Rect(0, 0, 1, 32, new Color32(232, 232, 232, 255));
            c.Rect(0, 31, 32, 1, new Color32(84, 84, 84, 255));
            c.Rect(31, 0, 1, 32, new Color32(110, 110, 110, 255));
            c.Rect(3, 15, 26, 1, new Color32(150, 150, 150, 255));
            c.Rect(3, 16, 26, 1, new Color32(224, 224, 224, 255));
            foreach (int rx in new[] { 3, 28 })
            {
                foreach (int ry in new[] { 3, 28 })
                {
                    c.Set(rx, ry, new Color32(120, 120, 120, 255));
                }
            }
            return c;
        }

        /// <summary>Darkness gathering under the ceiling (opaque at the top row, clear at the bottom).</summary>
        private static Canvas PaintFade()
        {
            var c = new Canvas(4, 32, Clear);
            for (int y = 0; y < 32; y++)
            {
                float t = 1f - y / 31f;
                byte alpha = (byte)(Mathf.Floor(t * t * 6f) / 6f * 170f);
                for (int x = 0; x < 4; x++) c.Set(x, y, new Color32(4, 8, 10, alpha));
            }
            return c;
        }

        /// <summary>Arched alcove for station back walls: a dark recess framed by lit stones (3 x 4 units).</summary>
        private static Canvas PaintArch(Palette p)
        {
            const int w = 96, h = 128, rim = 7;
            const float cx = 47.5f, springY = 48f, radius = 44f;
            var rng = new System.Random(97);
            Color32 recess = Mix(p.ramp[0], p.ramp[1], 0.55f);
            var c = new Canvas(w, h, Clear);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float dx = x - cx;
                    // Distance to the outline: a semicircle above the spring line, straight jambs below.
                    float outer = y < springY ? radius - Mathf.Sqrt(dx * dx + (springY - y) * (springY - y)) : radius - Mathf.Abs(dx);
                    if (outer < 0f) continue;

                    if (outer < rim)
                    {
                        // Voussoirs: stones split by joints that follow the curve (or run level on the jambs).
                        float along = y < springY ? Mathf.Atan2(springY - y, dx) * radius : y;
                        bool joint = Mathf.Abs(along % 14f) < 1f;
                        Color32 stone = outer < 1f ? p.ramp[4] : outer > rim - 2f ? p.ramp[1] : Mix(p.ramp[2], p.ramp[3], 0.7f);
                        c.Set(x, y, joint ? p.ramp[1] : stone);
                    }
                    else
                    {
                        bool course = y % 12 == 0 || (x + (y / 12 % 2) * 12) % 24 == 0;
                        c.Set(x, y, course ? Mix(recess, p.ramp[0], 0.7f) : rng.Next(40) == 0 ? p.ramp[1] : recess);
                    }
                }
            }
            return c;
        }

        private static Sprite Save(string directory, string name, Canvas canvas)
        {
            string path = $"{directory}/{name}.png";
            var texture = new Texture2D(canvas.W, canvas.H, TextureFormat.RGBA32, false);
            texture.SetPixels32(canvas.Pixels);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            // Full rect meshes are required by tiled SpriteRenderers.
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            importer.SetTextureSettings(settings);
            importer.spritePixelsPerUnit = Ppu;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        // ───────────────────────────── skinning ─────────────────────────────

        /// <summary>Remembers a solid so <see cref="SkinSolids"/> can dress it once the whole room is known.</summary>
        public static void RegisterSolid(GameObject go, Rect worldRect, SolidKind kind, Color tint)
        {
            s_solids.Add(new Solid { go = go, rect = worldRect, kind = kind, tint = tint });
        }

        /// <summary>
        /// Dresses every registered solid: fill, lit cap on the parts of its top that nothing covers,
        /// edge lines on exposed sides and a shadow lip on exposed undersides. Solids only hide each
        /// other's edges inside the same room.
        /// </summary>
        public static void SkinSolids()
        {
            foreach (var solid in s_solids)
            {
                if (solid.go == null) continue;
                Rect r = solid.rect;
                Rect room = RoomRectOf(solid.go.transform);

                if (solid.kind == SolidKind.Panel)
                {
                    Tiled("Art_Fill", solid.go.transform, r, Panel, -2, solid.tint);
                    continue;
                }

                Tiled("Art_Fill", solid.go.transform, r, solid.kind == SolidKind.Mass ? Mass : Block, -2, Color.white);

                var others = new List<Rect>();
                foreach (var other in s_solids)
                {
                    if (other.go == solid.go || other.go == null) continue;
                    if (other.go.transform.parent != solid.go.transform.parent) continue;
                    others.Add(other.rect);
                }

                float capHeight = Mathf.Min(CapHeight, r.height);
                if (r.yMax < room.yMax - Eps)
                {
                    foreach (var (a, b) in Exposed(r.xMin, r.xMax, others, o => o.yMin <= r.yMax + Eps && o.yMax > r.yMax + Eps, true))
                    {
                        Tiled("Art_Cap", solid.go.transform, Rect.MinMaxRect(a, r.yMax - capHeight, b, r.yMax), Cap, -1, Color.white, capHeight / CapHeight);
                    }
                }

                if (r.yMin > room.yMin + Eps && r.height > CapHeight + UnderHeight)
                {
                    foreach (var (a, b) in Exposed(r.xMin, r.xMax, others, o => o.yMax >= r.yMin - Eps && o.yMin < r.yMin - Eps, true))
                    {
                        Tiled("Art_Under", solid.go.transform, Rect.MinMaxRect(a, r.yMin, b, r.yMin + UnderHeight), Under, -1, Color.white);
                    }
                }

                if (r.width <= 2f * EdgeWidth) continue;
                float sideTop = r.yMax - capHeight;
                if (sideTop - r.yMin < 0.25f) continue;

                if (r.xMin > room.xMin + Eps)
                {
                    foreach (var (a, b) in Exposed(r.yMin, sideTop, others, o => o.xMax >= r.xMin - Eps && o.xMin < r.xMin - Eps, false))
                    {
                        Tiled("Art_EdgeL", solid.go.transform, Rect.MinMaxRect(r.xMin, a, r.xMin + EdgeWidth, b), Edge, -1, Color.white);
                    }
                }

                if (r.xMax < room.xMax - Eps)
                {
                    foreach (var (a, b) in Exposed(r.yMin, sideTop, others, o => o.xMin <= r.xMax + Eps && o.xMax > r.xMax + Eps, false))
                    {
                        Tiled("Art_EdgeR", solid.go.transform, Rect.MinMaxRect(r.xMax - EdgeWidth, a, r.xMax, b), Edge, -1, Color.white).flipX = true;
                    }
                }
            }

            s_solids.Clear();
        }

        /// <summary>Art for a one-way platform: a thin grating hanging from the walkable line.</summary>
        public static void SkinOneWay(GameObject go, float x1, float x2, float topY)
        {
            Tiled("Art_OneWay", go.transform, Rect.MinMaxRect(x1, topY - OneWayHeight, x2, topY), OneWay, -1, Color.white);
        }

        /// <summary>Back wall of a room, pilasters every 8 units, ceiling darkness and a row of lamps.</summary>
        public static void AddBackdrop(Transform room, Rect rect, bool lab, float floorY)
        {
            Tiled("Art_BackWall", room, rect, lab ? BackWallLab : BackWall, -30, Color.white);

            Color lampColor = lab ? (Color)Pal.cool : (Color)Pal.warm;
            int index = 0;
            for (float x = rect.xMin + 4f; x < rect.xMax - 2f; x += 8f, index++)
            {
                Tiled($"Art_Pillar_{index}", room, Rect.MinMaxRect(x - 0.5f, rect.yMin, x + 0.5f, rect.yMax), Pillar, -29, Color.white);
                if (index % 2 == 0) AddLamp($"Art_Lamp_{index}", room, new Vector2(x + 4f, rect.yMax - 3.2f), lampColor, 8f);

                // Station walls get arched alcoves standing on the main floor line, between pilasters.
                if (!lab && index % 2 == 1 && x + 6f < rect.xMax && floorY + 4f < rect.yMax - 1f)
                {
                    var arch = NewChild($"Art_Arch_{index}", room, new Vector2(x + 4f, floorY + 2f));
                    var archRenderer = arch.AddComponent<SpriteRenderer>();
                    archRenderer.sprite = Arch;
                    archRenderer.sortingOrder = -28;
                }
            }

            float fadeHeight = Mathf.Min(5f, rect.height * 0.4f);
            var fade = NewChild("Art_CeilingFade", room, new Vector2(rect.center.x, rect.yMax - fadeHeight * 0.5f));
            fade.transform.localScale = Vector3.Scale(fade.transform.localScale, new Vector3(rect.width / (4 * Px), fadeHeight, 1f));
            var fadeRenderer = fade.AddComponent<SpriteRenderer>();
            fadeRenderer.sprite = Fade;
            fadeRenderer.sortingOrder = -27;
        }

        /// <summary>Small fixture plus a soft glow in the light's colour.</summary>
        public static void AddLamp(string name, Transform parent, Vector2 worldPos, Color color, float glowDiameter)
        {
            var fixture = NewChild(name, parent, worldPos);
            fixture.transform.localScale = Vector3.Scale(fixture.transform.localScale, new Vector3(0.5f, 0.1875f, 1f));
            var fixtureRenderer = fixture.AddComponent<SpriteRenderer>();
            fixtureRenderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/WhiteSquare.png");
            fixtureRenderer.color = Color.Lerp(color, Color.white, 0.35f);
            fixtureRenderer.sortingOrder = -25;
            AddGlow(name + "_Glow", parent, worldPos, color, glowDiameter, -26);
        }

        /// <summary>Soft light halo. Glow sprites are 4 units wide, so the scale is a quarter of the diameter.</summary>
        public static void AddGlow(string name, Transform parent, Vector2 worldPos, Color color, float diameter, int order)
        {
            var glow = NewChild(name, parent, worldPos);
            glow.transform.localScale = Vector3.Scale(glow.transform.localScale, new Vector3(diameter * 0.25f, diameter * 0.25f, 1f));
            var renderer = glow.AddComponent<SpriteRenderer>();
            renderer.sprite = Glow;
            Color soft = Color.Lerp(color, Color.white, 0.25f);
            renderer.color = new Color(soft.r, soft.g, soft.b, 0.16f);
            renderer.sortingOrder = order;
        }

        /// <summary>
        /// Tiled sprite covering a world rectangle. Works under scaled parents (solids keep their scaled
        /// transform for the collider): the child cancels the parent's scale so pixels stay square.
        /// </summary>
        public static SpriteRenderer Tiled(string name, Transform parent, Rect worldRect, Sprite sprite, int order, Color tint, float squashY = 1f)
        {
            var go = NewChild(name, parent, worldRect.center);
            if (squashY < 0.999f)
            {
                go.transform.localScale = Vector3.Scale(go.transform.localScale, new Vector3(1f, squashY, 1f));
            }
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.tileMode = SpriteTileMode.Continuous;
            renderer.size = new Vector2(worldRect.width, worldRect.height / Mathf.Max(squashY, 0.001f));
            renderer.color = tint;
            renderer.sortingOrder = order;
            return renderer;
        }

        /// <summary>Tint for panels and props: the object's own colour pulled towards the zone ramp.</summary>
        public static Color ZoneTint(Color color)
        {
            Color pulled = Color.Lerp(color, (Color)Pal.ramp[4], 0.62f);
            pulled.a = color.a;
            return pulled;
        }

        private static GameObject NewChild(string name, Transform parent, Vector2 worldPos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(worldPos.x, worldPos.y, 0f);
            Vector3 scale = parent.lossyScale;
            go.transform.localScale = new Vector3(1f / scale.x, 1f / scale.y, 1f);
            return go;
        }

        private static Rect RoomRectOf(Transform solid)
        {
            var room = solid.GetComponentInParent<UnderTheRain.World.RoomBounds>(true);
            if (room == null) return Rect.MinMaxRect(-1e6f, -1e6f, 1e6f, 1e6f);
            var collider = room.GetComponent<BoxCollider2D>();
            Vector2 center = (Vector2)room.transform.position + collider.offset;
            return new Rect(center - collider.size * 0.5f, collider.size);
        }

        /// <summary>Parts of [a, b] not covered by any rect that satisfies <paramref name="covers"/>.</summary>
        private static List<(float, float)> Exposed(float a, float b, List<Rect> others, System.Predicate<Rect> covers, bool alongX)
        {
            var spans = new List<(float, float)> { (a, b) };
            foreach (var other in others)
            {
                if (!covers(other)) continue;
                float lo = alongX ? other.xMin : other.yMin;
                float hi = alongX ? other.xMax : other.yMax;
                var next = new List<(float, float)>();
                foreach (var (s, e) in spans)
                {
                    if (hi <= s + Eps || lo >= e - Eps)
                    {
                        next.Add((s, e));
                        continue;
                    }
                    if (lo > s + Eps) next.Add((s, lo));
                    if (hi < e - Eps) next.Add((hi, e));
                }
                spans = next;
            }
            spans.RemoveAll(span => span.Item2 - span.Item1 < 2f * Px);
            return spans;
        }
    }
}
