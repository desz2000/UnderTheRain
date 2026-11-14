using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnderTheRain.World;

namespace UnderTheRain.Editor
{
    /// <summary>
    /// Procedural pixel art for a zone, generated inside Unity from ONE colour ramp so that the backdrop,
    /// the floor/wall masses, the props and the rubble always belong together (one art pattern per zone).
    /// The kit paints small textures (32 px per unit), imports them as sprites and dresses the logical
    /// rectangles the zone setup builds: dark masses with a lit cap on every exposed top, edge lines,
    /// scattered rubble, and a backdrop in parallax layers (far wall with its details, columns and pipes,
    /// lamps, hanging foreground cables). Colliders are untouched: art is added as children.
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

        // Sorting orders (characters and pickups stay at 0).
        private const int OrderFarWall = -40;
        private const int OrderWallDetail = -39;
        private const int OrderColumn = -35;
        private const int OrderGlow = -31;
        private const int OrderLamp = -30;
        private const int OrderFade = -29;
        private const int OrderBench = -4;
        private const int OrderFill = -3;
        private const int OrderTrim = -2;
        private const int OrderRubble = -1;
        private const int OrderForeground = 40;

        public enum SolidKind
        {
            /// <summary>Floors, walls, ceilings: the dark mass of the level.</summary>
            Mass,
            /// <summary>Ledges, slabs, steps: lighter built structure standing in the room.</summary>
            Structure,
            /// <summary>Doors, gates, hatches: metal shutter tinted with the object's own colour.</summary>
            Panel,
            /// <summary>A solid with its own painted sprite (wagon, crate) stretched over the collider.</summary>
            Prop
        }

        /// <summary>One hue family from darkest (0) to brightest (6), plus grime, the line colour and two lights.</summary>
        public sealed class Palette
        {
            public Color32[] ramp;
            public Color32 dirt;
            public Color32 line;
            public Color32 mark;
            public Color32 warm;
            public Color32 cool;
        }

        /// <summary>
        /// Prologue (zone 0): an abandoned metro station. Dirty warm greys, rust-brown grime, the faded
        /// ochre of Line 7, amber emergency lamps and the lab's cyan.
        /// </summary>
        public static readonly Palette ProloguePalette = new Palette
        {
            ramp = new[]
            {
                Hex(0x0B0C0C), Hex(0x151716), Hex(0x212423), Hex(0x323634),
                Hex(0x4B504C), Hex(0x777A72), Hex(0xB3B4A6)
            },
            dirt = Hex(0x4D3B28),
            line = Hex(0x8C7A44),
            mark = Hex(0x8E3B32),
            warm = Hex(0xF0B45C),
            cool = Hex(0x4ECDC4)
        };

        public static Palette Pal { get; private set; }
        public static Sprite Mass { get; private set; }
        public static Sprite Block { get; private set; }
        public static Sprite Cap { get; private set; }
        public static Sprite CapPlatform { get; private set; }
        public static Sprite Edge { get; private set; }
        public static Sprite Under { get; private set; }
        public static Sprite BackWall { get; private set; }
        public static Sprite BackWallLab { get; private set; }
        public static Sprite Column { get; private set; }
        public static Sprite PipeRun { get; private set; }
        public static Sprite CableSag { get; private set; }
        public static Sprite OneWay { get; private set; }
        public static Sprite Glow { get; private set; }
        public static Sprite Shutter { get; private set; }
        public static Sprite Fade { get; private set; }
        public static Sprite Track { get; private set; }
        public static Sprite Wagon { get; private set; }
        public static Sprite Crate { get; private set; }
        public static Sprite Booth { get; private set; }
        public static Sprite Carcass { get; private set; }
        public static Sprite SignPlate { get; private set; }
        public static Sprite Graffiti { get; private set; }
        public static Sprite Bench { get; private set; }
        public static Sprite Vent { get; private set; }
        public static Sprite[] Posters { get; private set; }
        public static Sprite[] Rubble { get; private set; }

        private struct Solid
        {
            public GameObject go;
            public Rect rect;
            public SolidKind kind;
            public Color tint;
            public Sprite sprite;
        }

        private static readonly List<Solid> s_solids = new List<Solid>();

        // ───────────────────────────── generation ─────────────────────────────

        /// <summary>Paints (or repaints) every texture of the kit and loads the sprites.</summary>
        public static void EnsureKit(string directory, Palette p)
        {
            Pal = p;
            s_solids.Clear();
            Directory.CreateDirectory(directory);

            Mass = Save(directory, "Mass", PaintMass(p));
            Block = Save(directory, "Block", PaintBlock(p));
            Cap = Save(directory, "Cap", PaintCap(p, false));
            CapPlatform = Save(directory, "CapPlatform", PaintCap(p, true));
            Edge = Save(directory, "Edge", PaintEdge(p));
            Under = Save(directory, "Under", PaintUnder(p));
            BackWall = Save(directory, "BackWall", PaintBackWall(p));
            BackWallLab = Save(directory, "BackWallLab", PaintBackWallLab(p));
            Column = Save(directory, "Column", PaintColumn(p));
            PipeRun = Save(directory, "PipeRun", PaintPipeRun(p));
            CableSag = Save(directory, "CableSag", PaintCableSag(p));
            OneWay = Save(directory, "OneWay", PaintOneWay(p));
            Glow = Save(directory, "Glow", PaintGlow());
            Shutter = Save(directory, "Shutter", PaintShutter());
            Fade = Save(directory, "Fade", PaintFade());
            Track = Save(directory, "Track", PaintTrack(p));
            Wagon = Save(directory, "Wagon", PaintWagon(p));
            Crate = Save(directory, "Crate", PaintCrate(p));
            Booth = Save(directory, "Booth", PaintBooth(p));
            Carcass = Save(directory, "Carcass", PaintCarcass(p));
            SignPlate = Save(directory, "SignPlate", PaintSignPlate(p));
            Graffiti = Save(directory, "Graffiti", PaintGraffiti(p));
            Bench = Save(directory, "Bench", PaintBench(p));
            Vent = Save(directory, "Vent", PaintVent(p));
            Posters = new[]
            {
                Save(directory, "Poster_A", PaintPoster(p, 131)),
                Save(directory, "Poster_B", PaintPoster(p, 137))
            };
            Rubble = new[]
            {
                Save(directory, "Rubble_A", PaintRubble(p, 96, 30, 211)),
                Save(directory, "Rubble_B", PaintRubble(p, 64, 20, 223)),
                Save(directory, "Rubble_C", PaintRubble(p, 40, 12, 227)),
                Save(directory, "Rubble_D", PaintRubble(p, 128, 44, 229))
            };

            // Removed from the kit: delete the stale assets of the first version.
            AssetDatabase.DeleteAsset($"{directory}/Arch.png");
            AssetDatabase.DeleteAsset($"{directory}/Pillar.png");
            AssetDatabase.DeleteAsset($"{directory}/Panel.png");
        }

        private sealed class Canvas
        {
            public readonly int W;
            public readonly int H;
            public readonly Color32[] Pixels;
            private readonly bool _wrap;

            public Canvas(int w, int h, Color32 fill, bool wrap = true)
            {
                W = w;
                H = h;
                _wrap = wrap;
                Pixels = new Color32[w * h];
                for (int i = 0; i < Pixels.Length; i++) Pixels[i] = fill;
            }

            private bool Resolve(ref int x, ref int yTop)
            {
                if (_wrap)
                {
                    x = ((x % W) + W) % W;
                    yTop = ((yTop % H) + H) % H;
                    return true;
                }
                return x >= 0 && x < W && yTop >= 0 && yTop < H;
            }

            /// <summary>Sets a pixel; y is measured from the TOP row. Tileable canvases wrap, props clip.</summary>
            public void Set(int x, int yTop, Color32 c)
            {
                if (Resolve(ref x, ref yTop)) Pixels[(H - 1 - yTop) * W + x] = c;
            }

            public Color32 Get(int x, int yTop)
            {
                return Resolve(ref x, ref yTop) ? Pixels[(H - 1 - yTop) * W + x] : new Color32(0, 0, 0, 0);
            }

            /// <summary>Tints an existing (non-transparent) pixel towards a colour.</summary>
            public void Blend(int x, int yTop, Color32 c, float t)
            {
                Color32 current = Get(x, yTop);
                if (current.a == 0) return;
                Color32 mixed = Color32.Lerp(current, c, t);
                mixed.a = current.a;
                Set(x, yTop, mixed);
            }

            public void Box(int x, int yTop, int w, int h, Color32 c)
            {
                for (int j = 0; j < h; j++)
                    for (int i = 0; i < w; i++)
                        Set(x + i, yTop + j, c);
            }

            public void Line(int x0, int y0, int x1, int y1, Color32 c)
            {
                int steps = Mathf.Max(Mathf.Abs(x1 - x0), Mathf.Abs(y1 - y0), 1);
                for (int i = 0; i <= steps; i++)
                {
                    Set(Mathf.RoundToInt(Mathf.Lerp(x0, x1, i / (float)steps)), Mathf.RoundToInt(Mathf.Lerp(y0, y1, i / (float)steps)), c);
                }
            }

            public void Disc(int cx, int cy, int radius, Color32 c)
            {
                for (int y = -radius; y <= radius; y++)
                    for (int x = -radius; x <= radius; x++)
                        if (x * x + y * y <= radius * radius) Set(cx + x, cy + y, c);
            }

            /// <summary>Irregular grime patch: tints what is already painted, denser at the centre.</summary>
            public void Stain(int cx, int cy, int radius, Color32 c, float strength, System.Random rng)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    for (int x = -radius; x <= radius; x++)
                    {
                        float d = Mathf.Sqrt(x * x + y * y) / radius;
                        if (d > 1f || rng.NextDouble() < 0.25f + d * d * 0.7f) continue;
                        Blend(cx + x, cy + y, c, strength * (1f - d));
                    }
                }
            }

            /// <summary>Dirt running down from a point.</summary>
            public void Drip(int x, int yTop, int length, Color32 c, float strength)
            {
                for (int i = 0; i < length; i++) Blend(x, yTop + i, c, strength * (1f - i / (float)length));
            }
        }

        private static Color32 Hex(int rgb) => new Color32((byte)(rgb >> 16), (byte)((rgb >> 8) & 0xFF), (byte)(rgb & 0xFF), 255);

        private static Color32 Mix(Color32 a, Color32 b, float t) => Color32.Lerp(a, b, t);

        private static readonly Color32 Clear = new Color32(0, 0, 0, 0);
        private static readonly Color32 Black = new Color32(0, 0, 0, 255);

        /// <summary>Dark concrete of floors and walls: big low-contrast blocks so the mass reads as one shape.</summary>
        private static Canvas PaintMass(Palette p)
        {
            var rng = new System.Random(11);
            Color32 joint = Mix(p.ramp[0], Black, 0.5f);
            var c = new Canvas(64, 64, p.ramp[0]);
            for (int row = 0; row < 4; row++)
            {
                int offset = row % 2 == 0 ? 0 : 16;
                for (int col = 0; col < 2; col++)
                {
                    int x = offset + col * 32;
                    int y = row * 16;
                    Color32 fill = Mix(p.ramp[0], p.ramp[1], 0.3f + 0.55f * (float)rng.NextDouble());
                    c.Box(x, y, 32, 16, fill);
                    c.Box(x, y, 32, 1, Mix(fill, p.ramp[2], 0.4f));
                    c.Box(x, y + 15, 32, 1, joint);
                    c.Box(x + 31, y, 1, 16, joint);
                    for (int n = 0; n < 6; n++) c.Set(x + 2 + rng.Next(27), y + 2 + rng.Next(12), Mix(fill, p.ramp[0], 0.6f));
                }
            }
            for (int n = 0; n < 5; n++) c.Stain(rng.Next(64), rng.Next(64), 5 + rng.Next(5), p.dirt, 0.12f, rng);
            return c;
        }

        /// <summary>Lighter poured concrete for ledges, slabs and steps, chipped and stained.</summary>
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
                    Color32 fill = Mix(p.ramp[2], p.ramp[3], 0.35f + 0.6f * (float)rng.NextDouble());
                    c.Box(x, y, 16, 16, fill);
                    c.Box(x, y, 16, 1, Mix(fill, p.ramp[4], 0.55f));
                    c.Box(x, y, 1, 16, Mix(fill, p.ramp[4], 0.3f));
                    c.Box(x, y + 15, 16, 1, p.ramp[1]);
                    c.Box(x + 15, y, 1, 16, p.ramp[1]);
                    for (int n = 0; n < 4; n++) c.Set(x + 2 + rng.Next(12), y + 3 + rng.Next(10), Mix(fill, p.ramp[1], 0.5f));
                }
            }
            for (int n = 0; n < 3; n++) c.Stain(rng.Next(32), rng.Next(32), 4, p.dirt, 0.16f, rng);
            return c;
        }

        /// <summary>
        /// Lit lip on every walkable top. Platform floors also carry what is left of the ochre safety strip.
        /// </summary>
        private static Canvas PaintCap(Palette p, bool platform)
        {
            var rng = new System.Random(platform ? 41 : 37);
            var c = new Canvas(64, 8, p.ramp[3]);
            for (int x = 0; x < 64; x++)
            {
                c.Set(x, 0, rng.Next(7) == 0 ? p.ramp[5] : p.ramp[6]);
                c.Set(x, 1, p.ramp[5]);
                c.Set(x, 2, p.ramp[4]);
                c.Set(x, 3, rng.Next(6) == 0 ? p.ramp[3] : p.ramp[4]);
                c.Set(x, 4, p.ramp[3]);
                c.Set(x, 5, p.ramp[3]);
                c.Set(x, 6, p.ramp[2]);
                c.Set(x, 7, p.ramp[1]);
                if (platform && x / 3 % 2 == 0 && rng.Next(100) < 62)
                {
                    c.Set(x, 2, Mix(p.line, p.ramp[5], 0.2f));
                    c.Set(x, 3, Mix(p.line, p.ramp[3], 0.3f));
                }
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
            Color32 deep = Mix(p.ramp[0], Black, 0.5f);
            var c = new Canvas(64, 4, p.ramp[1]);
            for (int x = 0; x < 64; x++)
            {
                c.Set(x, 2, x % 16 == 0 ? deep : p.ramp[2]);
                c.Set(x, 3, deep);
            }
            return c;
        }

        /// <summary>
        /// Station wall: rectangular subway tiles in brick bond, the Line 7 band, missing tiles showing the
        /// concrete, cracks, damp patches and rust running down. Kept dark and soft: it is the far layer.
        /// </summary>
        private static Canvas PaintBackWall(Palette p)
        {
            var rng = new System.Random(53);
            Color32 grout = Mix(p.ramp[1], p.ramp[0], 0.3f);
            var c = new Canvas(256, 256, p.ramp[2]);
            for (int row = 0; row < 32; row++)
            {
                int offset = row % 2 == 0 ? 0 : 8;
                bool band = row == 11 || row == 12;
                for (int col = 0; col < 16; col++)
                {
                    int x = offset + col * 16;
                    int y = row * 8;
                    bool missing = !band && rng.Next(100) < 4;
                    Color32 fill;
                    if (missing) fill = Mix(p.ramp[1], p.ramp[0], 0.35f);
                    else if (band) fill = Mix(Mix(p.line, p.ramp[2], 0.62f), p.ramp[1], 0.3f * (float)rng.NextDouble());
                    else fill = Mix(Mix(p.ramp[1], p.ramp[2], 0.6f), p.ramp[3], 0.5f * (float)rng.NextDouble());

                    c.Box(x, y, 16, 8, fill);
                    if (!missing) c.Box(x + 1, y + 1, 14, 1, Mix(fill, p.ramp[4], 0.16f));
                    c.Box(x, y, 16, 1, grout);
                    c.Box(x, y, 1, 8, grout);
                }
            }

            for (int k = 0; k < 8; k++)
            {
                int x = rng.Next(256);
                int y = rng.Next(256);
                int length = 16 + rng.Next(30);
                for (int i = 0; i < length; i++)
                {
                    c.Set(x, y, p.ramp[0]);
                    y++;
                    x += rng.Next(3) - 1;
                }
            }

            // Damp and soot in large soft patches, rust in smaller ones, grime running from under the band.
            for (int k = 0; k < 16; k++) c.Stain(rng.Next(256), rng.Next(256), 12 + rng.Next(22), p.ramp[0], 0.34f, rng);
            for (int k = 0; k < 7; k++) c.Stain(rng.Next(256), rng.Next(256), 8 + rng.Next(14), p.dirt, 0.2f, rng);
            for (int k = 0; k < 22; k++) c.Drip(rng.Next(256), 104, 8 + rng.Next(40), k % 3 == 0 ? p.dirt : p.ramp[0], 0.4f);
            return c;
        }

        /// <summary>Lab wall: riveted panels of the same ramp with a dim cyan service line, equally neglected.</summary>
        private static Canvas PaintBackWallLab(Palette p)
        {
            var rng = new System.Random(71);
            var c = new Canvas(256, 256, p.ramp[2]);
            for (int row = 0; row < 8; row++)
            {
                int offset = row % 2 == 0 ? 0 : 32;
                for (int col = 0; col < 4; col++)
                {
                    int x = offset + col * 64;
                    int y = row * 32;
                    Color32 fill = Mix(Mix(p.ramp[1], p.ramp[2], 0.65f), p.ramp[3], 0.4f * (float)rng.NextDouble());
                    c.Box(x, y, 64, 32, fill);
                    c.Box(x + 1, y + 1, 62, 1, Mix(fill, p.ramp[4], 0.2f));
                    c.Box(x, y + 31, 64, 1, p.ramp[0]);
                    c.Box(x + 63, y, 1, 32, p.ramp[0]);
                    c.Box(x, y, 1, 32, Mix(fill, p.ramp[3], 0.3f));
                    foreach (int rx in new[] { 3, 59 })
                    {
                        foreach (int ry in new[] { 4, 26 })
                        {
                            c.Set(x + rx, y + ry, p.ramp[4]);
                            c.Set(x + rx, y + ry + 1, p.ramp[0]);
                        }
                    }
                    if (rng.Next(3) == 0)
                    {
                        for (int v = 0; v < 5; v++) c.Box(x + 22, y + 9 + v * 3, 20, 1, p.ramp[0]);
                    }
                }
            }

            // Service conduit with dashes of the lab's cyan, some of them dead.
            c.Box(0, 124, 256, 6, p.ramp[0]);
            c.Box(0, 124, 256, 1, p.ramp[3]);
            for (int x = 4; x < 256; x += 16)
            {
                c.Box(x, 126, 8, 2, rng.Next(4) == 0 ? p.ramp[2] : Mix(p.cool, p.ramp[2], 0.5f));
            }

            for (int k = 0; k < 12; k++) c.Stain(rng.Next(256), rng.Next(256), 10 + rng.Next(20), p.ramp[0], 0.3f, rng);
            for (int k = 0; k < 5; k++) c.Stain(rng.Next(256), rng.Next(256), 8 + rng.Next(10), p.dirt, 0.16f, rng);
            for (int k = 0; k < 14; k++) c.Drip(rng.Next(256), 130, 10 + rng.Next(34), p.ramp[0], 0.4f);
            return c;
        }

        /// <summary>Station column: a band of the line colour, bare stained concrete, a step lighter than the wall.</summary>
        private static Canvas PaintColumn(Palette p)
        {
            var rng = new System.Random(83);
            Color32 body = Mix(p.ramp[2], p.ramp[3], 0.75f);
            var c = new Canvas(40, 128, body);
            for (int y = 0; y < 128; y++)
            {
                c.Set(0, y, p.ramp[0]);
                c.Set(1, y, Mix(p.ramp[3], p.ramp[4], 0.7f));
                c.Set(2, y, Mix(p.ramp[3], p.ramp[4], 0.3f));
                c.Set(36, y, p.ramp[2]);
                c.Set(37, y, p.ramp[2]);
                c.Set(38, y, Mix(p.ramp[1], p.ramp[2], 0.5f));
                c.Set(39, y, p.ramp[0]);
            }
            c.Box(1, 60, 38, 6, Mix(p.line, body, 0.45f));
            c.Box(1, 66, 38, 1, p.ramp[1]);
            c.Box(1, 59, 38, 1, Mix(body, p.ramp[4], 0.4f));
            c.Box(1, 127, 38, 1, p.ramp[1]);
            for (int k = 0; k < 9; k++) c.Drip(3 + rng.Next(33), 67, 10 + rng.Next(40), k % 2 == 0 ? p.dirt : p.ramp[0], 0.35f);
            for (int k = 0; k < 5; k++) c.Stain(rng.Next(40), rng.Next(128), 9 + rng.Next(8), p.ramp[0], 0.2f, rng);
            for (int k = 0; k < 14; k++) c.Set(3 + rng.Next(33), rng.Next(128), p.ramp[1]);
            return c;
        }

        /// <summary>Two service pipes on brackets, tiled along the ceiling.</summary>
        private static Canvas PaintPipeRun(Palette p)
        {
            var c = new Canvas(64, 14, Clear);
            for (int x = 0; x < 64; x++)
            {
                c.Set(x, 1, p.ramp[5]);
                c.Set(x, 2, p.ramp[4]);
                c.Set(x, 3, p.ramp[3]);
                c.Set(x, 4, p.ramp[3]);
                c.Set(x, 5, p.ramp[2]);
                c.Set(x, 6, p.ramp[0]);
                c.Set(x, 9, p.ramp[4]);
                c.Set(x, 10, p.ramp[3]);
                c.Set(x, 11, p.ramp[2]);
                c.Set(x, 12, p.ramp[0]);
            }
            c.Box(0, 0, 3, 14, p.ramp[2]);
            c.Box(0, 0, 1, 14, p.ramp[4]);
            c.Box(30, 1, 2, 6, Mix(p.dirt, p.ramp[2], 0.4f));
            return c;
        }

        /// <summary>Two cables drooping between supports. Tinted black it doubles as the foreground silhouette.</summary>
        private static Canvas PaintCableSag(Palette p)
        {
            var c = new Canvas(128, 36, Clear, false);
            for (int x = 0; x < 128; x++)
            {
                float u = (x - 63.5f) / 64f;
                int deep = Mathf.RoundToInt(2 + 28f * (1f - u * u));
                int shallow = Mathf.RoundToInt(1 + 15f * (1f - u * u));
                c.Set(x, deep, p.ramp[1]);
                c.Set(x, deep + 1, p.ramp[0]);
                c.Set(x, shallow, p.ramp[0]);
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
            c.Blend(7, 2, p.dirt, 0.6f);
            c.Blend(21, 3, p.dirt, 0.6f);
            return c;
        }

        /// <summary>Soft round light, stepped so it still reads as pixel art. Tinted by the renderer.</summary>
        private static Canvas PaintGlow()
        {
            var c = new Canvas(128, 128, Clear, false);
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

        /// <summary>Neutral roll-up shutter slats; the renderer tints them with each door's colour.</summary>
        private static Canvas PaintShutter()
        {
            var c = new Canvas(32, 32, new Color32(170, 170, 170, 255));
            for (int y = 0; y < 32; y++)
            {
                int row = y % 8;
                byte v = row == 0 ? (byte)232 : row == 1 ? (byte)196 : row == 6 ? (byte)120 : row == 7 ? (byte)70 : (byte)168;
                for (int x = 0; x < 32; x++) c.Set(x, y, new Color32(v, v, v, 255));
            }
            var rng = new System.Random(89);
            for (int n = 0; n < 26; n++) c.Blend(rng.Next(32), rng.Next(32), new Color32(60, 50, 44, 255), 0.5f);
            return c;
        }

        /// <summary>Darkness gathering under the ceiling (opaque at the top row, clear at the bottom).</summary>
        private static Canvas PaintFade()
        {
            var c = new Canvas(4, 32, Clear);
            for (int y = 0; y < 32; y++)
            {
                float t = 1f - y / 31f;
                byte alpha = (byte)(Mathf.Floor(t * t * 6f) / 6f * 190f);
                for (int x = 0; x < 4; x++) c.Set(x, y, new Color32(6, 6, 6, alpha));
            }
            return c;
        }

        /// <summary>Track bed strip: ballast, sleepers and a rusted rail, tiled along the trench.</summary>
        private static Canvas PaintTrack(Palette p)
        {
            var rng = new System.Random(101);
            var c = new Canvas(64, 12, Clear);
            for (int x = 0; x < 64; x++)
            {
                for (int y = 6; y < 12; y++)
                {
                    int roll = rng.Next(10);
                    c.Set(x, y, roll < 5 ? p.ramp[1] : roll < 8 ? p.ramp[2] : p.ramp[3]);
                }
                if (rng.Next(3) == 0) c.Set(x, 5, p.ramp[2]);
            }
            for (int x = 4; x < 64; x += 16)
            {
                c.Box(x, 3, 8, 5, Mix(p.dirt, p.ramp[2], 0.55f));
                c.Box(x, 3, 8, 1, Mix(p.dirt, p.ramp[4], 0.5f));
                c.Box(x, 7, 8, 1, p.ramp[0]);
            }
            for (int x = 0; x < 64; x++)
            {
                c.Set(x, 0, rng.Next(5) == 0 ? Mix(p.dirt, p.ramp[5], 0.4f) : p.ramp[5]);
                c.Set(x, 1, Mix(p.dirt, p.ramp[3], 0.5f));
                c.Set(x, 2, p.ramp[0]);
            }
            return c;
        }

        /// <summary>
        /// Derailed metro car resting in the track bed (9 x 3.5 units): broken windows, forced doors,
        /// the faded Line 7 stripe, rust and soot. Fits its collider exactly.
        /// </summary>
        private static Canvas PaintWagon(Palette p)
        {
            var rng = new System.Random(151);
            const int w = 288, h = 112;
            Color32 body = Mix(p.ramp[4], p.ramp[5], 0.4f);
            Color32 glass = Mix(p.ramp[0], p.ramp[1], 0.5f);
            var c = new Canvas(w, h, Clear, false);

            // Roof with equipment boxes.
            c.Box(8, 6, 272, 10, p.ramp[3]);
            c.Box(8, 6, 272, 1, p.ramp[5]);
            c.Box(40, 1, 38, 6, p.ramp[2]);
            c.Box(40, 1, 38, 1, p.ramp[4]);
            c.Box(204, 1, 46, 6, p.ramp[2]);
            c.Box(204, 1, 46, 1, p.ramp[4]);

            // Body shell.
            c.Box(2, 16, 284, 72, body);
            c.Box(2, 16, 284, 1, p.ramp[6]);
            c.Box(2, 17, 284, 2, Mix(body, p.ramp[6], 0.35f));
            c.Box(2, 16, 4, 72, p.ramp[3]);
            c.Box(282, 16, 4, 72, p.ramp[3]);
            foreach (int cx in new[] { 2, 3, 284, 285 }) c.Set(cx, 16, Clear);

            // Line 7 stripe under the windows, scratched away in places.
            for (int x = 6; x < 282; x++)
            {
                if (rng.Next(100) < 12) continue;
                c.Box(x, 60, 1, 5, Mix(p.line, body, 0.15f));
                c.Set(x, 65, Mix(p.line, p.ramp[2], 0.5f));
            }

            // Doors (three pairs) and the windows between them.
            int[] doors = { 40, 132, 224 };
            int cursor = 12;
            foreach (int door in doors)
            {
                for (int x = cursor; x + 20 <= door - 4; x += 26) PaintWagonWindow(c, p, rng, x, glass);
                cursor = door + 28;

                Color32 leaf = Mix(body, p.ramp[3], 0.45f);
                c.Box(door, 22, 24, 64, leaf);
                c.Box(door, 22, 1, 64, p.ramp[2]);
                c.Box(door + 23, 22, 1, 64, p.ramp[2]);
                c.Box(door + 11, 22, 2, 64, p.ramp[1]);
                c.Box(door + 3, 28, 7, 22, glass);
                c.Box(door + 14, 28, 7, 22, glass);
                c.Box(door, 22, 24, 1, p.ramp[5]);
            }
            for (int x = cursor; x + 20 <= 278; x += 26) PaintWagonWindow(c, p, rng, x, glass);
            // The middle door was forced half open.
            c.Box(132, 22, 9, 64, p.ramp[0]);

            // Skirt, underframe and bogies.
            c.Box(4, 88, 280, 6, p.ramp[2]);
            c.Box(4, 88, 280, 1, p.ramp[1]);
            c.Box(20, 94, 248, 8, p.ramp[1]);
            foreach (int bx in new[] { 96, 150, 182 }) c.Box(bx, 94, 22, 10, p.ramp[2]);
            foreach (int wx in new[] { 40, 70, 218, 248 })
            {
                c.Disc(wx, 102, 9, p.ramp[0]);
                c.Disc(wx, 102, 7, p.ramp[2]);
                c.Disc(wx, 102, 5, p.ramp[1]);
                c.Disc(wx, 102, 2, p.ramp[4]);
            }
            c.Box(34, 100, 42, 3, p.ramp[1]);
            c.Box(212, 100, 42, 3, p.ramp[1]);

            // Years of neglect: soot from the roof line, rust from every seam, grime along the skirt.
            for (int k = 0; k < 46; k++) c.Drip(4 + rng.Next(280), 17, 6 + rng.Next(36), k % 3 == 0 ? p.dirt : p.ramp[1], 0.42f);
            for (int k = 0; k < 14; k++) c.Stain(rng.Next(w), 30 + rng.Next(56), 6 + rng.Next(9), p.dirt, 0.3f, rng);
            for (int k = 0; k < 10; k++) c.Stain(rng.Next(w), 20 + rng.Next(60), 7 + rng.Next(10), p.ramp[1], 0.3f, rng);
            for (int x = 4; x < 284; x++)
            {
                for (int y = 78; y < 88; y++)
                {
                    if (rng.Next(100) < (y - 74) * 5) c.Blend(x, y, p.ramp[1], 0.5f);
                }
            }
            return c;
        }

        private static void PaintWagonWindow(Canvas c, Palette p, System.Random rng, int x, Color32 glass)
        {
            c.Box(x - 1, 27, 22, 26, p.ramp[2]);
            c.Box(x, 28, 20, 24, glass);
            if (rng.Next(3) == 0)
            {
                // Still glazed: a dull reflection.
                c.Line(x + 3, 46, x + 9, 31, p.ramp[2]);
                c.Line(x + 6, 48, x + 13, 31, p.ramp[2]);
            }
            else
            {
                // Smashed: shards left along the frame.
                for (int i = 0; i < 20; i += 1 + rng.Next(3))
                {
                    int tooth = 1 + rng.Next(6);
                    c.Box(x + i, 52 - tooth, 1, tooth, p.ramp[3]);
                    if (rng.Next(2) == 0) c.Box(x + i, 28, 1, 1 + rng.Next(4), p.ramp[3]);
                }
            }
        }

        /// <summary>Supply crate (1.5 units): banded planks, the kind left behind on a closed platform.</summary>
        private static Canvas PaintCrate(Palette p)
        {
            var rng = new System.Random(163);
            Color32 wood = Mix(p.dirt, p.ramp[3], 0.55f);
            var c = new Canvas(48, 48, wood, false);
            for (int x = 0; x < 48; x += 12) c.Box(x, 0, 1, 48, Mix(wood, p.ramp[0], 0.55f));
            for (int n = 0; n < 60; n++) c.Blend(rng.Next(48), rng.Next(48), p.ramp[1], 0.35f);
            foreach (int by in new[] { 0, 42 })
            {
                c.Box(0, by, 48, 6, p.ramp[3]);
                c.Box(0, by, 48, 1, p.ramp[5]);
                c.Box(0, by + 5, 48, 1, p.ramp[1]);
            }
            c.Box(0, 0, 5, 48, p.ramp[3]);
            c.Box(43, 0, 5, 48, p.ramp[3]);
            c.Box(0, 0, 1, 48, p.ramp[5]);
            c.Box(47, 0, 1, 48, p.ramp[1]);
            for (int i = 0; i < 36; i++)
            {
                c.Set(6 + i, 6 + i, p.ramp[2]);
                c.Set(7 + i, 6 + i, Mix(wood, p.ramp[4], 0.4f));
            }
            foreach (int nx in new[] { 2, 45 })
                foreach (int ny in new[] { 3, 44 })
                    c.Set(nx, ny, p.ramp[6]);
            c.Stain(34, 30, 7, p.ramp[0], 0.35f, rng);
            return c;
        }

        /// <summary>
        /// Ticket booth front (4.5 x 3.5 units): header with the line bar, cracked glass with a faint warm
        /// light behind it, counter and slatted base.
        /// </summary>
        private static Canvas PaintBooth(Palette p)
        {
            var rng = new System.Random(173);
            const int w = 144, h = 112;
            var c = new Canvas(w, h, p.ramp[3], false);
            c.Box(0, 0, w, 1, p.ramp[5]);
            c.Box(0, 0, 2, h, p.ramp[4]);
            c.Box(w - 2, 0, 2, h, p.ramp[1]);

            // Header sign.
            c.Box(4, 3, 136, 15, p.ramp[1]);
            c.Box(8, 7, 128, 3, Mix(p.line, p.ramp[2], 0.3f));
            for (int x = 10; x < 96; x += 9) c.Box(x, 12, 6, 2, p.ramp[4]);

            // Window: dark glass, a little warm light left on inside, cracks from an impact.
            Color32 glass = Mix(p.ramp[0], p.warm, 0.1f);
            c.Box(10, 23, 124, 46, p.ramp[4]);
            c.Box(12, 25, 120, 42, glass);
            for (int y = 25; y < 67; y++)
            {
                for (int x = 12; x < 132; x++)
                {
                    float d = Mathf.Sqrt((x - 84) * (x - 84) + (y - 44) * (y - 44));
                    if (d < 26f) c.Blend(x, y, p.warm, 0.16f * (1f - d / 26f));
                }
            }
            c.Box(71, 25, 2, 42, p.ramp[3]);
            foreach (var (ex, ey) in new[] { (60, 26), (74, 60), (118, 30), (126, 52), (96, 66), (40, 50) })
            {
                c.Line(98, 42, ex, ey, p.ramp[4]);
            }
            c.Disc(98, 42, 2, p.ramp[5]);
            c.Disc(40, 54, 4, p.ramp[1]);
            c.Disc(40, 54, 2, p.ramp[0]);

            // Counter and slatted base.
            c.Box(6, 70, 132, 5, p.ramp[4]);
            c.Box(6, 70, 132, 1, p.ramp[6]);
            c.Box(6, 75, 132, 1, p.ramp[1]);
            for (int y = 80; y < 106; y += 6)
            {
                c.Box(8, y, 128, 1, p.ramp[2]);
                c.Box(8, y + 1, 128, 1, Mix(p.ramp[3], p.ramp[4], 0.5f));
            }
            c.Box(0, 106, w, 6, p.ramp[1]);
            c.Box(0, 106, w, 1, p.ramp[2]);

            for (int k = 0; k < 20; k++) c.Drip(4 + rng.Next(136), 18, 6 + rng.Next(28), k % 2 == 0 ? p.dirt : p.ramp[1], 0.4f);
            for (int k = 0; k < 6; k++) c.Stain(rng.Next(w), 76 + rng.Next(30), 8, p.ramp[0], 0.3f, rng);
            return c;
        }

        /// <summary>
        /// The shelved war prototype hanging from the crane (4 x 4.7 units): a heavy armoured torso, dead
        /// visor, arms hanging, cables spilling out where the legs were never fitted.
        /// </summary>
        private static Canvas PaintCarcass(Palette p)
        {
            var rng = new System.Random(181);
            const int w = 128, h = 150;
            var c = new Canvas(w, h, Clear, false);

            c.Box(60, 0, 8, 16, p.ramp[1]);
            c.Box(56, 12, 16, 4, p.ramp[3]);

            // Head and visor.
            c.Box(50, 16, 28, 22, p.ramp[2]);
            c.Box(50, 16, 28, 1, p.ramp[5]);
            c.Box(50, 16, 2, 22, p.ramp[4]);
            c.Box(54, 24, 20, 6, p.ramp[0]);
            c.Box(66, 26, 3, 2, Mix(p.dirt, p.ramp[4], 0.4f));
            c.Box(58, 38, 12, 6, p.ramp[1]);

            // Shoulders and pauldrons.
            c.Box(18, 44, 92, 22, p.ramp[3]);
            c.Box(18, 44, 92, 1, p.ramp[6]);
            c.Box(18, 45, 92, 2, p.ramp[4]);
            c.Box(18, 65, 92, 1, p.ramp[1]);
            foreach (int px in new[] { 8, 102 })
            {
                c.Box(px, 46, 18, 28, p.ramp[2]);
                c.Box(px, 46, 18, 1, p.ramp[5]);
                c.Box(px, 46, 2, 28, p.ramp[4]);
                c.Box(px + 16, 46, 2, 28, p.ramp[1]);
            }

            // Tapering torso with a chest plate and vents.
            for (int y = 66; y < 114; y++)
            {
                int half = Mathf.RoundToInt(34 - (y - 66) * 0.28f);
                c.Box(64 - half, y, half * 2, 1, p.ramp[2]);
                c.Set(64 - half, y, p.ramp[4]);
                c.Set(64 + half - 1, y, p.ramp[0]);
            }
            c.Box(48, 72, 32, 28, p.ramp[3]);
            c.Box(48, 72, 32, 1, p.ramp[5]);
            c.Box(48, 99, 32, 1, p.ramp[1]);
            for (int y = 78; y < 96; y += 4) c.Box(53, y, 22, 2, p.ramp[0]);
            c.Box(60, 102, 8, 3, Mix(p.cool, p.ramp[1], 0.75f));

            // Arms hanging dead, three-fingered grippers.
            foreach (int ax in new[] { 10, 106 })
            {
                c.Box(ax, 74, 12, 30, p.ramp[2]);
                c.Box(ax, 74, 2, 30, p.ramp[4]);
                c.Box(ax + 1, 104, 10, 4, p.ramp[0]);
                c.Box(ax, 108, 12, 24, p.ramp[3]);
                c.Box(ax, 108, 2, 24, p.ramp[4]);
                c.Box(ax + 10, 108, 2, 24, p.ramp[1]);
                for (int f = 0; f < 3; f++) c.Box(ax + f * 5, 132, 3, 9, p.ramp[2]);
            }

            // Pelvis mount and the cables where the legs should be.
            c.Box(44, 114, 40, 12, p.ramp[1]);
            c.Box(44, 114, 40, 1, p.ramp[3]);
            for (int k = 0; k < 7; k++)
            {
                int x = 48 + k * 5;
                int length = 10 + rng.Next(14);
                for (int i = 0; i < length; i++)
                {
                    c.Set(x, 126 + i, k % 3 == 0 ? Mix(p.dirt, p.ramp[2], 0.4f) : p.ramp[0]);
                    if (rng.Next(4) == 0) x += rng.Next(3) - 1;
                }
            }

            for (int k = 0; k < 22; k++) c.Drip(10 + rng.Next(108), 46 + rng.Next(30), 8 + rng.Next(26), k % 2 == 0 ? p.dirt : p.ramp[0], 0.4f);
            for (int k = 0; k < 8; k++) c.Stain(rng.Next(w), 40 + rng.Next(90), 7, p.ramp[0], 0.3f, rng);
            return c;
        }

        /// <summary>Hanging station plate: the Line 7 roundel and bars where the name used to be.</summary>
        private static Canvas PaintSignPlate(Palette p)
        {
            var rng = new System.Random(191);
            var c = new Canvas(96, 30, Clear, false);
            c.Box(14, 0, 1, 6, p.ramp[1]);
            c.Box(81, 0, 1, 6, p.ramp[1]);
            c.Box(0, 6, 96, 24, p.ramp[1]);
            c.Box(0, 6, 96, 1, p.ramp[4]);
            c.Box(0, 29, 96, 1, p.ramp[0]);
            c.Box(0, 6, 1, 24, p.ramp[3]);
            c.Box(95, 6, 1, 24, p.ramp[0]);

            c.Disc(14, 18, 8, Mix(p.line, p.ramp[4], 0.15f));
            string[] seven = { "#####", "....#", "...#.", "..#..", "..#..", "..#..", "..#.." };
            for (int y = 0; y < seven.Length; y++)
                for (int x = 0; x < 5; x++)
                    if (seven[y][x] == '#') c.Set(12 + x, 15 + y, p.ramp[0]);

            c.Box(28, 12, 58, 4, p.ramp[5]);
            c.Box(28, 20, 36, 3, p.ramp[3]);
            for (int n = 0; n < 40; n++) c.Blend(rng.Next(96), 7 + rng.Next(22), p.ramp[1], 0.6f);
            for (int k = 0; k < 6; k++) c.Drip(rng.Next(96), 8, 8 + rng.Next(12), p.dirt, 0.4f);
            return c;
        }

        /// <summary>The resistance mark (image only): a sprayed ring with a slash through it, paint running.</summary>
        private static Canvas PaintGraffiti(Palette p)
        {
            var rng = new System.Random(197);
            var c = new Canvas(48, 56, Clear, false);
            for (int y = 0; y < 48; y++)
            {
                for (int x = 0; x < 48; x++)
                {
                    float d = Mathf.Sqrt((x - 23.5f) * (x - 23.5f) + (y - 23.5f) * (y - 23.5f));
                    if (d > 13f && d < 17.5f && rng.Next(100) < 88) c.Set(x, y, p.mark);
                }
            }
            for (int i = 0; i < 38; i++)
            {
                for (int t = 0; t < 4; t++)
                {
                    if (rng.Next(100) < 90) c.Set(5 + i + t, 42 - i, p.mark);
                }
            }
            foreach (int dx in new[] { 9, 20, 27, 37 })
            {
                int length = 5 + rng.Next(11);
                for (int i = 0; i < length; i++) c.Set(dx, 38 + i, Mix(p.mark, p.ramp[1], i / (float)length * 0.6f));
            }
            return c;
        }

        /// <summary>Platform bench, slats missing.</summary>
        private static Canvas PaintBench(Palette p)
        {
            var c = new Canvas(72, 26, Clear, false);
            foreach (int y in new[] { 0, 4 })
            {
                c.Box(2, y, 68, 3, p.ramp[3]);
                c.Box(2, y, 68, 1, p.ramp[4]);
            }
            c.Box(40, 4, 14, 3, Clear);
            c.Box(2, 12, 68, 4, p.ramp[3]);
            c.Box(2, 12, 68, 1, p.ramp[5]);
            c.Box(2, 15, 68, 1, p.ramp[1]);
            foreach (int lx in new[] { 8, 60 })
            {
                c.Box(lx, 0, 3, 26, p.ramp[2]);
                c.Box(lx, 0, 1, 26, p.ramp[3]);
                c.Box(lx - 3, 24, 9, 2, p.ramp[1]);
            }
            return c;
        }

        /// <summary>Wall vent with a bent slat.</summary>
        private static Canvas PaintVent(Palette p)
        {
            var c = new Canvas(40, 22, p.ramp[1], false);
            c.Box(0, 0, 40, 1, p.ramp[3]);
            c.Box(0, 0, 1, 22, p.ramp[3]);
            c.Box(0, 21, 40, 1, p.ramp[0]);
            c.Box(39, 0, 1, 22, p.ramp[0]);
            for (int y = 3; y < 20; y += 4)
            {
                c.Box(3, y, 34, 1, p.ramp[3]);
                c.Box(3, y + 1, 34, 2, p.ramp[0]);
            }
            c.Line(22, 11, 30, 14, p.ramp[3]);
            for (int i = 0; i < 16; i++) c.Blend(8 + i % 3, 21 - i % 2, p.dirt, 0.5f);
            return c;
        }

        /// <summary>Torn notice glued to the tiles: no readable text, just what time left of it.</summary>
        private static Canvas PaintPoster(Palette p, int seed)
        {
            var rng = new System.Random(seed);
            Color32 paper = Mix(Mix(p.ramp[5], p.dirt, 0.3f), p.ramp[3], 0.45f);
            var c = new Canvas(28, 38, paper, false);
            c.Box(3, 4, 22, 13, Mix(p.ramp[2], rng.Next(2) == 0 ? p.line : p.mark, 0.25f));
            for (int y = 21; y < 34; y += 4) c.Box(3, y, 12 + rng.Next(11), 1, p.ramp[2]);
            c.Box(0, 0, 28, 1, Mix(paper, p.ramp[6], 0.3f));

            // Torn corners and a strip ripped off the side.
            for (int i = 0; i < 12; i++) c.Box(28 - 12 + i + rng.Next(2), 38 - i - 1, 12, 1, Clear);
            for (int i = 0; i < 7; i++) c.Box(0, i, 7 - i - rng.Next(2), 1, Clear);
            int rip = 8 + rng.Next(10);
            for (int i = 0; i < 9; i++) c.Box(0, rip + i, 1 + rng.Next(3), 1, Clear);
            for (int k = 0; k < 5; k++) c.Stain(rng.Next(28), rng.Next(38), 6, p.ramp[1], 0.35f, rng);
            return c;
        }

        /// <summary>
        /// Heap of rubble: broken concrete and tile chunks piled under a rough dome, rebar sticking out,
        /// dust at the base.
        /// </summary>
        private static Canvas PaintRubble(Palette p, int w, int h, int seed)
        {
            var rng = new System.Random(seed);
            var c = new Canvas(w, h, Clear, false);
            var profile = new int[w];
            float lump = (float)rng.NextDouble() * 6f;
            for (int x = 0; x < w; x++)
            {
                float u = (x + 0.5f) / w * 2f - 1f;
                float dome = Mathf.Pow(Mathf.Clamp01(1f - u * u), 0.65f);
                float noise = 0.82f + 0.18f * Mathf.Sin(x * 0.45f + lump) * Mathf.Sin(x * 0.17f + lump * 2f);
                profile[x] = Mathf.RoundToInt((h - 2) * dome * noise);
            }

            // Dark core so gaps between chunks read as shadow.
            for (int x = 0; x < w; x++)
                for (int y = h - profile[x]; y < h; y++)
                    c.Set(x, y, p.ramp[1]);

            int chunks = w * h / 22;
            for (int n = 0; n < chunks; n++)
            {
                int cw = 3 + rng.Next(9);
                int ch = 2 + rng.Next(6);
                int x0 = rng.Next(w - cw);
                int limit = Mathf.Min(profile[x0], profile[x0 + cw - 1]);
                if (limit < ch + 1) continue;
                int y0 = h - 1 - rng.Next(limit - ch + 1) - ch + 1;

                bool tile = rng.Next(5) == 0;
                Color32 fill = tile ? Mix(p.ramp[4], p.ramp[5], (float)rng.NextDouble() * 0.6f)
                                    : Mix(p.ramp[2], p.ramp[4], (float)rng.NextDouble());
                c.Box(x0, y0, cw, ch, fill);
                c.Box(x0, y0, cw, 1, Mix(fill, p.ramp[6], 0.45f));
                c.Box(x0, y0 + ch - 1, cw, 1, Mix(fill, p.ramp[0], 0.6f));
                c.Box(x0 + cw - 1, y0, 1, ch, Mix(fill, p.ramp[0], 0.4f));
            }

            // Bent rebar.
            for (int k = 0; k < 1 + w / 48; k++)
            {
                int x = w / 5 + rng.Next(w * 3 / 5);
                int y = h - profile[x];
                int lean = rng.Next(2) == 0 ? 1 : -1;
                int length = 4 + rng.Next(5);
                for (int i = 0; i < length; i++)
                {
                    c.Set(x, y - i, Mix(p.dirt, p.ramp[3], 0.3f));
                    if (i % 2 == 1) x += lean;
                }
            }

            // Dust settling at the base and grime over everything.
            for (int x = 0; x < w; x++)
            {
                if (profile[x] > 0 && rng.Next(3) != 0) c.Set(x, h - 1, Mix(p.ramp[3], p.dirt, 0.3f));
            }
            for (int k = 0; k < w / 10; k++) c.Stain(rng.Next(w), rng.Next(h), 5, p.dirt, 0.22f, rng);
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
        public static void RegisterSolid(GameObject go, Rect worldRect, SolidKind kind, Color tint, Sprite sprite = null)
        {
            s_solids.Add(new Solid { go = go, rect = worldRect, kind = kind, tint = tint, sprite = sprite });
        }

        /// <summary>
        /// Dresses every registered solid: fill, lit cap on the parts of its top that nothing covers,
        /// edge lines on exposed sides, a shadow lip on exposed undersides, and rubble (plus the odd
        /// bench) scattered over the open floor. Solids only hide each other's edges inside the same room.
        /// </summary>
        public static void SkinSolids()
        {
            foreach (var solid in s_solids)
            {
                if (solid.go == null) continue;
                Rect r = solid.rect;
                var roomBounds = solid.go.GetComponentInParent<RoomBounds>(true);
                Rect room = RoomRect(roomBounds);
                bool lab = roomBounds != null && roomBounds.RoomId.Contains("lab");

                if (solid.kind == SolidKind.Prop)
                {
                    Fit("Art_Prop", solid.go.transform, r, solid.sprite, OrderFill);
                    continue;
                }
                if (solid.kind == SolidKind.Panel)
                {
                    Tiled("Art_Fill", solid.go.transform, r, Shutter, OrderFill, solid.tint);
                    continue;
                }

                bool mass = solid.kind == SolidKind.Mass;
                Tiled("Art_Fill", solid.go.transform, r, mass ? Mass : Block, OrderFill, Color.white);

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
                    var rng = new System.Random(Mathf.RoundToInt(r.xMin * 31f + r.yMax * 17f) + StableHash(solid.go.name));
                    foreach (var (a, b) in Exposed(r.xMin, r.xMax, others, o => o.yMin <= r.yMax + Eps && o.yMax > r.yMax + Eps, true))
                    {
                        Tiled("Art_Cap", solid.go.transform, Rect.MinMaxRect(a, r.yMax - capHeight, b, r.yMax),
                            mass && !lab ? CapPlatform : Cap, OrderTrim, Color.white, capHeight / CapHeight);
                        Scatter(solid.go.transform, a, b, r.yMax, mass, lab, room, rng);
                    }
                }

                if (r.yMin > room.yMin + Eps && r.height > CapHeight + UnderHeight)
                {
                    foreach (var (a, b) in Exposed(r.xMin, r.xMax, others, o => o.yMax >= r.yMin - Eps && o.yMin < r.yMin - Eps, true))
                    {
                        Tiled("Art_Under", solid.go.transform, Rect.MinMaxRect(a, r.yMin, b, r.yMin + UnderHeight), Under, OrderTrim, Color.white);
                    }
                }

                if (r.width <= 2f * EdgeWidth) continue;
                float sideTop = r.yMax - capHeight;
                if (sideTop - r.yMin < 0.25f) continue;

                if (r.xMin > room.xMin + Eps)
                {
                    foreach (var (a, b) in Exposed(r.yMin, sideTop, others, o => o.xMax >= r.xMin - Eps && o.xMin < r.xMin - Eps, false))
                    {
                        Tiled("Art_EdgeL", solid.go.transform, Rect.MinMaxRect(r.xMin, a, r.xMin + EdgeWidth, b), Edge, OrderTrim, Color.white);
                    }
                }

                if (r.xMax < room.xMax - Eps)
                {
                    foreach (var (a, b) in Exposed(r.yMin, sideTop, others, o => o.xMin <= r.xMax + Eps && o.xMax > r.xMax + Eps, false))
                    {
                        Tiled("Art_EdgeR", solid.go.transform, Rect.MinMaxRect(r.xMax - EdgeWidth, a, r.xMax, b), Edge, OrderTrim, Color.white).flipX = true;
                    }
                }
            }

            s_solids.Clear();
        }

        /// <summary>Rubble heaps (and a bench on long station floors) over an exposed stretch of floor.</summary>
        private static void Scatter(Transform parent, float a, float b, float topY, bool mass, bool lab, Rect room, System.Random rng)
        {
            float length = b - a;
            if (length < 2.5f) return;

            // Keep doorways and spawn points (next to the room's side walls) clear.
            float lo = Mathf.Max(a + 0.6f, room.xMin + 4.5f);
            float hi = Mathf.Min(b - 0.6f, room.xMax - 4.5f);
            if (hi - lo < 1.5f) return;

            if (mass && !lab && hi - lo >= 11f && rng.Next(100) < 65)
            {
                float x = Mathf.Lerp(lo, hi, 0.2f + 0.6f * (float)rng.NextDouble());
                Place("Art_Bench", parent, Bench, x, topY, OrderBench, false);
            }

            int heaps = length < 5f ? (rng.Next(100) < 35 ? 1 : 0) : Mathf.Min(4, 1 + Mathf.FloorToInt((hi - lo) / 8f));
            for (int i = 0; i < heaps; i++)
            {
                // Big heaps only on real floors; ledges and short stretches get the small ones.
                int variant = mass && length >= 5f ? rng.Next(3) : 1 + rng.Next(2);
                var sprite = Rubble[variant];
                float half = sprite.bounds.extents.x;
                if (hi - lo < half * 2f) continue;
                float x = Mathf.Lerp(lo + half, hi - half, (float)rng.NextDouble());
                Place($"Art_Rubble_{i}", parent, sprite, x, topY, OrderRubble, rng.Next(2) == 0);
            }
        }

        /// <summary>Art for a one-way platform: a thin grating hanging from the walkable line.</summary>
        public static void SkinOneWay(GameObject go, float x1, float x2, float topY)
        {
            Tiled("Art_OneWay", go.transform, Rect.MinMaxRect(x1, topY - OneWayHeight, x2, topY), OneWay, OrderTrim, Color.white);
        }

        /// <summary>
        /// Backdrop of a room in depth layers: the far wall with its notices, vents and station plates
        /// (slides the most), columns, pipes and cables in front of it, world-fixed hanging lamps, and
        /// black cables in the foreground that slide the other way.
        /// </summary>
        public static void AddBackdrop(Transform room, Rect rect, bool lab, float floorY)
        {
            var rng = new System.Random(StableHash(room.name));

            // Far wall.
            const float farFactor = 0.3f;
            var far = Layer("Art_Layer_FarWall", room, rect, farFactor);
            Tiled("Wall", far, Grow(rect, farFactor), lab ? BackWallLab : BackWall, OrderFarWall, Color.white);
            for (float x = rect.xMin + 3f; x < rect.xMax - 3f; x += 5f + (float)rng.NextDouble() * 5f)
            {
                int roll = rng.Next(10);
                if (lab)
                {
                    if (roll < 5) Stamp("Vent", far, Vent, new Vector2(x, rect.yMax - 3f - (float)rng.NextDouble() * 4f), OrderWallDetail);
                    continue;
                }

                float y = floorY + 2.2f + (float)rng.NextDouble() * 1.6f;
                if (y > rect.yMax - 2f) y = rect.yMax - 3f;
                if (roll < 5) Stamp("Poster", far, Posters[rng.Next(Posters.Length)], new Vector2(x, y), OrderWallDetail);
                else if (roll < 7) Stamp("Vent", far, Vent, new Vector2(x, rect.yMax - 3.4f), OrderWallDetail);
            }
            if (!lab && rect.width > 20f)
            {
                Stamp("SignPlate", far, SignPlate, new Vector2(rect.xMin + rect.width * 0.3f, Mathf.Min(floorY + 5.6f, rect.yMax - 3f)), OrderWallDetail);
            }

            // Columns, pipes and cables.
            const float midFactor = 0.12f;
            var mid = Layer("Art_Layer_Columns", room, rect, midFactor);
            Rect midRect = Grow(rect, midFactor);
            float columnWidth = Column.bounds.size.x;
            int index = 0;
            for (float x = rect.xMin + 5f; x < rect.xMax - 2f; x += 9f, index++)
            {
                Tiled($"Column_{index}", mid, Rect.MinMaxRect(x - columnWidth * 0.5f, midRect.yMin, x + columnWidth * 0.5f, midRect.yMax), Column, OrderColumn, Color.white);
                if (x + 9f < rect.xMax) Stamp($"Cable_{index}", mid, CableSag, new Vector2(x + 4.5f, rect.yMax - 2.4f), OrderColumn - 1);
            }
            float pipeHeight = PipeRun.bounds.size.y;
            Tiled("Pipes", mid, Rect.MinMaxRect(midRect.xMin, rect.yMax - 1.7f - pipeHeight, midRect.xMax, rect.yMax - 1.7f), PipeRun, OrderColumn + 1, Color.white);

            // Lamps hang from the ceiling in the playfield itself; a few are dead.
            Color lampColor = lab ? (Color)Pal.cool : (Color)Pal.warm;
            for (float x = rect.xMin + 9.5f; x < rect.xMax - 4f; x += 13f)
            {
                AddLamp($"Art_Lamp_{Mathf.RoundToInt(x)}", room, new Vector2(x, rect.yMax - 1.55f), lampColor, 9f, rng.Next(100) < 22);
            }

            float fadeHeight = Mathf.Min(5f, rect.height * 0.4f);
            var fade = NewChild("Art_CeilingFade", room, new Vector2(rect.center.x, rect.yMax - fadeHeight * 0.5f));
            fade.transform.localScale = Vector3.Scale(fade.transform.localScale, new Vector3(rect.width / (4 * Px), fadeHeight, 1f));
            var fadeRenderer = fade.AddComponent<SpriteRenderer>();
            fadeRenderer.sprite = Fade;
            fadeRenderer.sortingOrder = OrderFade;

            // Foreground: black cables hanging across the top of the screen, closer than the playfield.
            const float foreFactor = -0.22f;
            var fore = Layer("Art_Layer_Foreground", room, rect, foreFactor);
            for (float x = rect.xMin + 2f + (float)rng.NextDouble() * 6f; x < rect.xMax; x += 9f + (float)rng.NextDouble() * 8f)
            {
                var cable = Stamp("ForeCable", fore, CableSag, new Vector2(x, rect.yMax - 1.1f), OrderForeground);
                cable.color = new Color(0.02f, 0.02f, 0.02f, 1f);
                cable.transform.localScale = Vector3.Scale(cable.transform.localScale, new Vector3(1.6f, 1.6f, 1f));
            }
        }

        /// <summary>Hanging lamp plus a soft glow in the light's colour (dead lamps keep only the fixture).</summary>
        public static void AddLamp(string name, Transform parent, Vector2 worldPos, Color color, float glowDiameter, bool dead = false)
        {
            var white = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/WhiteSquare.png");

            var wire = NewChild(name + "_Wire", parent, worldPos + new Vector2(0f, 0.3f));
            wire.transform.localScale = Vector3.Scale(wire.transform.localScale, new Vector3(Px, 0.5f, 1f));
            var wireRenderer = wire.AddComponent<SpriteRenderer>();
            wireRenderer.sprite = white;
            wireRenderer.color = (Color)Pal.ramp[0];
            wireRenderer.sortingOrder = OrderLamp;

            var fixture = NewChild(name, parent, worldPos);
            fixture.transform.localScale = Vector3.Scale(fixture.transform.localScale, new Vector3(0.5f, 0.1875f, 1f));
            var fixtureRenderer = fixture.AddComponent<SpriteRenderer>();
            fixtureRenderer.sprite = white;
            fixtureRenderer.color = dead ? (Color)Pal.ramp[3] : Color.Lerp(color, Color.white, 0.35f);
            fixtureRenderer.sortingOrder = OrderLamp;

            if (!dead) AddGlow(name + "_Glow", parent, worldPos + new Vector2(0f, -0.6f), color, glowDiameter, OrderGlow);
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

        /// <summary>Sprite stretched to cover a world rectangle exactly (props drawn for their collider).</summary>
        public static SpriteRenderer Fit(string name, Transform parent, Rect worldRect, Sprite sprite, int order)
        {
            var go = NewChild(name, parent, worldRect.center);
            Vector2 size = sprite.bounds.size;
            go.transform.localScale = Vector3.Scale(go.transform.localScale, new Vector3(worldRect.width / size.x, worldRect.height / size.y, 1f));
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
            return renderer;
        }

        /// <summary>Sprite at its native size, centred on a world point.</summary>
        public static SpriteRenderer Stamp(string name, Transform parent, Sprite sprite, Vector2 worldPos, int order)
        {
            var go = NewChild(name, parent, worldPos);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
            return renderer;
        }

        /// <summary>Tint for doors and flat decor: the object's own colour pulled towards the zone ramp.</summary>
        public static Color ZoneTint(Color color)
        {
            Color pulled = Color.Lerp(color, (Color)Pal.ramp[4], 0.62f);
            pulled.a = color.a;
            return pulled;
        }

        /// <summary>Sprite standing on a floor line (its bottom edge one pixel into the cap).</summary>
        private static void Place(string name, Transform parent, Sprite sprite, float x, float floorY, int order, bool flip)
        {
            var renderer = Stamp(name, parent, sprite, new Vector2(x, floorY + sprite.bounds.extents.y - Px), order);
            renderer.flipX = flip;
        }

        private static Transform Layer(string name, Transform room, Rect rect, float factor)
        {
            var go = NewChild(name, room, rect.center);
            // Vertical parallax is halved: rooms pan mostly sideways and tall drifts look wrong.
            go.AddComponent<ParallaxLayer>().Configure(new Vector2(factor, factor * 0.5f), rect.center, go.transform.position);
            return go.transform;
        }

        /// <summary>A layer that slides by <paramref name="factor"/> must overhang the room by that share of it.</summary>
        private static Rect Grow(Rect rect, float factor)
        {
            float f = Mathf.Abs(factor);
            float mx = rect.width * 0.5f * f + 1f;
            float my = rect.height * 0.5f * f + 1f;
            return Rect.MinMaxRect(rect.xMin - mx, rect.yMin - my, rect.xMax + mx, rect.yMax + my);
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

        private static Rect RoomRect(RoomBounds room)
        {
            if (room == null) return Rect.MinMaxRect(-1e6f, -1e6f, 1e6f, 1e6f);
            var collider = room.GetComponent<BoxCollider2D>();
            Vector2 center = (Vector2)room.transform.position + collider.offset;
            return new Rect(center - collider.size * 0.5f, collider.size);
        }

        /// <summary>Same value on every run and machine (string.GetHashCode is not).</summary>
        private static int StableHash(string text)
        {
            int hash = 17;
            foreach (char ch in text) hash = hash * 31 + ch;
            return hash & 0x7FFFFFFF;
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
