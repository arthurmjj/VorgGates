using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VorgGates.EditorTools
{
    /// <summary>Draws the three inventory icons as small original PNG files.</summary>
    internal static class VorgIconLibrary
    {
        private const string Folder = "Assets/_Project/UI/Icons";
        private const int Size = 128;
        private const int Samples = 3;

        private static readonly Color Steel = VorgArt.Hex("#D5DAE6");
        private static readonly Color SteelDark = VorgArt.Hex("#8C92A4");
        private static readonly Color Gold = VorgArt.Hex("#F2C14E");
        private static readonly Color Wood = VorgArt.Hex("#8A5E3E");
        private static readonly Color Ether = VorgArt.Hex("#6FF0E8");
        private static readonly Color Outline = VorgArt.Hex("#14121C");

        public static void Prepare()
        {
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/_Project/UI", "Icons");
            Ensure("Icon_Sword", 45f, new[]
            {
                Layer(Steel, P(.46f, .30f), P(.54f, .30f), P(.54f, .82f), P(.5f, .93f), P(.46f, .82f)),
                Layer(SteelDark, P(.49f, .31f), P(.51f, .31f), P(.51f, .8f), P(.49f, .8f)),
                Layer(Wood, P(.465f, .12f), P(.535f, .12f), P(.535f, .27f), P(.465f, .27f)),
                Layer(Gold, P(.31f, .25f), P(.69f, .25f), P(.65f, .31f), P(.35f, .31f)),
                Circle(Gold, .5f, .1f, .05f)
            });
            Ensure("Icon_Axe", 35f, new[]
            {
                Layer(Wood, P(.465f, .05f), P(.535f, .05f), P(.535f, .92f), P(.465f, .92f)),
                Layer(Steel, P(.53f, .82f), P(.63f, .86f), P(.82f, .94f), P(.89f, .72f), P(.82f, .5f), P(.63f, .59f), P(.53f, .65f)),
                Layer(SteelDark, P(.53f, .79f), P(.62f, .8f), P(.62f, .64f), P(.53f, .68f)),
                Layer(Steel, P(.47f, .81f), P(.28f, .74f), P(.47f, .67f)),
                Layer(Gold, P(.45f, .6f), P(.55f, .6f), P(.55f, .64f), P(.45f, .64f)),
                Circle(Gold, .5f, .05f, .04f)
            });
            Ensure("Icon_Staff", 35f, new[]
            {
                Layer(Wood, P(.47f, .05f), P(.53f, .05f), P(.53f, .72f), P(.47f, .72f)),
                Layer(Gold, P(.44f, .66f), P(.56f, .66f), P(.56f, .7f), P(.44f, .7f)),
                Layer(Gold, P(.47f, .7f), P(.35f, .78f), P(.37f, .92f), P(.42f, .8f), P(.5f, .74f)),
                Layer(Gold, P(.53f, .7f), P(.65f, .78f), P(.63f, .92f), P(.58f, .8f), P(.5f, .74f)),
                Circle(Ether, .5f, .83f, .105f),
                Circle(Color.white, .47f, .86f, .03f)
            });
        }

        private static Vector2 P(float x, float y) => new Vector2(x, y);

        private static (Color color, Vector2[] points) Layer(Color color, params Vector2[] points) => (color, points);

        private static (Color color, Vector2[] points) Circle(Color color, float x, float y, float radius)
        {
            var points = new Vector2[20];
            for (var i = 0; i < points.Length; i++)
            {
                var angle = i * Mathf.PI * 2f / points.Length;
                points[i] = new Vector2(x + Mathf.Cos(angle) * radius, y + Mathf.Sin(angle) * radius);
            }

            return (color, points);
        }

        private static void Ensure(string name, float tilt, IReadOnlyList<(Color color, Vector2[] points)> layers)
        {
            var path = $"{Folder}/{name}.png";
            if (!File.Exists(path))
            {
                var pixels = Render(layers, tilt);
                var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
                texture.SetPixels(pixels);
                File.WriteAllBytes(path, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            }

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer.alphaIsTransparency && importer.wrapMode == TextureWrapMode.Clamp && !importer.mipmapEnabled) return;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = Size;
            importer.SaveAndReimport();
        }

        private static Color[] Render(IReadOnlyList<(Color color, Vector2[] points)> layers, float tilt)
        {
            var radians = tilt * Mathf.Deg2Rad;
            var cos = Mathf.Cos(radians);
            var sin = Mathf.Sin(radians);
            var result = new Color[Size * Size];
            var coverage = new float[Size * Size];
            for (var y = 0; y < Size; y++)
            for (var x = 0; x < Size; x++)
            {
                var sum = Color.clear;
                for (var sy = 0; sy < Samples; sy++)
                for (var sx = 0; sx < Samples; sx++)
                {
                    var u = (x + (sx + .5f) / Samples) / Size - .5f;
                    var v = (y + (sy + .5f) / Samples) / Size - .5f;
                    var point = new Vector2(u * cos - v * sin + .5f, u * sin + v * cos + .5f);
                    for (var i = layers.Count - 1; i >= 0; i--)
                    {
                        if (!Inside(point, layers[i].points)) continue;
                        sum += layers[i].color;
                        coverage[y * Size + x] += 1f / (Samples * Samples);
                        break;
                    }
                }

                var alpha = coverage[y * Size + x];
                result[y * Size + x] = alpha > 0f ? new Color(sum.r / (alpha * Samples * Samples), sum.g / (alpha * Samples * Samples), sum.b / (alpha * Samples * Samples), alpha) : Color.clear;
            }

            const int outline = 4;
            var final = new Color[result.Length];
            for (var y = 0; y < Size; y++)
            for (var x = 0; x < Size; x++)
            {
                var near = 0f;
                for (var dy = -outline; dy <= outline; dy++)
                for (var dx = -outline; dx <= outline; dx++)
                {
                    if (dx * dx + dy * dy > outline * outline) continue;
                    var nx = x + dx;
                    var ny = y + dy;
                    if (nx < 0 || ny < 0 || nx >= Size || ny >= Size) continue;
                    near = Mathf.Max(near, coverage[ny * Size + nx]);
                }

                var color = result[y * Size + x];
                var mixed = Color.Lerp(Outline, color, color.a);
                mixed.a = Mathf.Max(near, color.a);
                final[y * Size + x] = mixed;
            }

            return final;
        }

        private static bool Inside(Vector2 point, Vector2[] polygon)
        {
            var inside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                var a = polygon[i];
                var b = polygon[j];
                if (a.y > point.y != b.y > point.y && point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x) inside = !inside;
            }

            return inside;
        }
    }
}
