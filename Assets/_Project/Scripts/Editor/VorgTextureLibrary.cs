using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VorgGates.EditorTools
{
    /// <summary>Small tileable surface textures generated for the project.</summary>
    internal static class VorgTextureLibrary
    {
        private const string Folder = "Assets/_Project/Art/Textures";
        private const int Size = 256;

        public static Texture2D Stone { get; private set; }
        public static Texture2D Floor { get; private set; }
        public static Texture2D Wood { get; private set; }
        public static Texture2D Metal { get; private set; }
        public static Texture2D Plate { get; private set; }
        public static Texture2D Soft { get; private set; }
        public static Texture2D Beam { get; private set; }

        public static void Prepare()
        {
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/_Project/Art", "Textures");
            Stone = Ensure("T_VorgStone", (u, v) => Gray(0.8f + 0.24f * (Fbm(u, v, 5, 4, 11) - 0.5f) * 2f - Crack(u, v, 5, 21, 0.018f) * 0.22f), true);
            Floor = Ensure("T_VorgFloor", (u, v) => Gray(0.82f + 0.16f * (Fbm(u, v, 6, 4, 31) - 0.5f) * 2f - Crack(u, v, 3, 41, 0.009f) * 0.26f), true);
            Wood = Ensure("T_VorgWood", (u, v) =>
            {
                var grain = Mathf.Sin((v * 14f + Fbm(u, v, 3, 3, 51) * 2.4f) * Mathf.PI * 2f) * 0.5f + 0.5f;
                return Gray(0.74f + grain * 0.2f + (Fbm(u, v, 8, 2, 61) - 0.5f) * 0.14f);
            }, true);
            Metal = Ensure("T_VorgMetal", (u, v) => Gray(0.84f + (Fbm(u, v, 7, 3, 71) - 0.5f) * 0.26f - Crack(u, v, 9, 81, 0.008f) * 0.12f), true);
            Plate = Ensure("T_VorgPlate", (u, v) =>
            {
                var px = Mathf.Repeat(u * 2f, 1f);
                var py = Mathf.Repeat(v * 2f, 1f);
                var edge = Mathf.Min(Mathf.Min(px, 1f - px), Mathf.Min(py, 1f - py));
                var seam = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(edge / 0.022f));
                var bevel = edge < 0.05f ? 0.1f : 0f;
                var cx = Mathf.Min(px, 1f - px) - 0.09f;
                var cy = Mathf.Min(py, 1f - py) - 0.09f;
                var rivet = 1f - Mathf.SmoothStep(0.022f, 0.036f, Mathf.Sqrt(cx * cx + cy * cy));
                var wear = (Fbm(u, v, 6, 3, 91) - 0.5f) * 0.3f;
                return Gray(0.74f + wear + bevel + rivet * 0.3f - seam * 0.62f);
            }, true);
            Soft = Ensure("T_VorgSoft", (u, v) =>
            {
                var distance = Mathf.Clamp01(Vector2.Distance(new Vector2(u, v), new Vector2(0.5f, 0.5f)) * 2f);
                var alpha = 1f - distance;
                return new Color(1f, 1f, 1f, alpha * alpha * (3f - 2f * alpha));
            }, false);
            Beam = Ensure("T_VorgBeam", (u, v) =>
            {
                var vertical = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(v * 5f)) * Mathf.Pow(1f - v, 1.3f);
                var streak = 0.72f + 0.28f * Mathf.Sin(u * Mathf.PI * 2f * 7f) * Mathf.Sin(u * Mathf.PI * 2f * 3f);
                return new Color(1f, 1f, 1f, vertical * streak);
            }, false);
        }

        private static Color Gray(float value)
        {
            value = Mathf.Clamp01(value);
            return new Color(value, value, value, 1f);
        }

        private static Texture2D Ensure(string name, Func<float, float, Color> shade, bool repeat)
        {
            var path = $"{Folder}/{name}.png";
            if (!File.Exists(path))
            {
                var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
                for (var y = 0; y < Size; y++)
                for (var x = 0; x < Size; x++)
                    texture.SetPixel(x, y, shade(x / (float)Size, y / (float)Size));
                File.WriteAllBytes(path, texture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            }

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            var wrap = repeat ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
            if (importer.wrapMode != wrap || importer.maxTextureSize != Size || importer.alphaIsTransparency == repeat)
            {
                importer.wrapMode = wrap;
                importer.maxTextureSize = Size;
                importer.alphaIsTransparency = !repeat;
                importer.mipmapEnabled = true;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static float Hash(int x, int y, int seed)
        {
            unchecked
            {
                var h = x * 374761393 + y * 668265263 + seed * 2147483647;
                h = (h ^ (h >> 13)) * 1274126177;
                return ((h ^ (h >> 16)) & 0x7fffffff) / (float)int.MaxValue;
            }
        }

        private static float Value(float u, float v, int period, int seed)
        {
            var x = u * period;
            var y = v * period;
            var x0 = Mathf.FloorToInt(x);
            var y0 = Mathf.FloorToInt(y);
            var fx = x - x0;
            var fy = y - y0;
            fx = fx * fx * (3f - 2f * fx);
            fy = fy * fy * (3f - 2f * fy);
            float At(int i, int j) => Hash(Wrap(i, period), Wrap(j, period), seed);
            return Mathf.Lerp(Mathf.Lerp(At(x0, y0), At(x0 + 1, y0), fx), Mathf.Lerp(At(x0, y0 + 1), At(x0 + 1, y0 + 1), fx), fy);
        }

        private static float Fbm(float u, float v, int period, int octaves, int seed)
        {
            var total = 0f;
            var amplitude = 0.5f;
            var weight = 0f;
            for (var i = 0; i < octaves; i++)
            {
                total += Value(u, v, period << i, seed + i) * amplitude;
                weight += amplitude;
                amplitude *= 0.5f;
            }

            return total / weight;
        }

        // Thin lines along the borders of a wrapped cell pattern look like cracked stone.
        private static float Crack(float u, float v, int cells, int seed, float width)
        {
            var x = u * cells;
            var y = v * cells;
            var cx = Mathf.FloorToInt(x);
            var cy = Mathf.FloorToInt(y);
            var first = float.MaxValue;
            var second = float.MaxValue;
            for (var j = -1; j <= 1; j++)
            for (var i = -1; i <= 1; i++)
            {
                var px = cx + i + Hash(Wrap(cx + i, cells), Wrap(cy + j, cells), seed);
                var py = cy + j + Hash(Wrap(cx + i, cells), Wrap(cy + j, cells), seed + 7);
                var distance = Mathf.Sqrt((px - x) * (px - x) + (py - y) * (py - y));
                if (distance < first)
                {
                    second = first;
                    first = distance;
                }
                else if (distance < second)
                {
                    second = distance;
                }
            }

            return 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((second - first) / (width * cells)));
        }

        private static int Wrap(int value, int period) => ((value % period) + period) % period;
    }
}
