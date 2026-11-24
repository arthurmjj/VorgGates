using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VorgGates.EditorTools
{
    /// <summary>Faceted low-poly replacements with the same dimensions as Unity's primitives.</summary>
    internal static class VorgMeshLibrary
    {
        private const string Folder = "Assets/_Project/Art/Meshes/Library";

        private static readonly Mesh[] RoughBoxes = new Mesh[4];
        private static Mesh bevelBox;
        private static Mesh sphere;
        private static Mesh cylinder;
        private static Mesh capsule;

        public static void Prepare()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Art/Meshes")) AssetDatabase.CreateFolder("Assets/_Project/Art", "Meshes");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/_Project/Art/Meshes", "Library");
            bevelBox = GetOrCreate("LM_BevelBox", () => BuildBox(0.075f, 0f, 1));
            for (var i = 0; i < RoughBoxes.Length; i++)
            {
                var seed = 100 + i;
                RoughBoxes[i] = GetOrCreate($"LM_RoughBox{i}", () => BuildBox(0.11f, 0.055f, seed));
            }

            sphere = GetOrCreate("LM_FacetSphere", () => BuildSphere(5, 9));
            cylinder = GetOrCreate("LM_FacetCylinder", () => BuildCylinder(12));
            capsule = GetOrCreate("LM_FacetCapsule", () => BuildCapsule(9));
        }

        /// <summary>Returns the faceted mesh for a primitive, or null to keep Unity's mesh.</summary>
        public static Mesh For(PrimitiveType type, string name, Vector3 position)
        {
            switch (type)
            {
                case PrimitiveType.Cube:
                    if (!IsRough(name)) return bevelBox;
                    var hash = Mathf.Abs(Mathf.RoundToInt(position.x * 13f + position.y * 7f + position.z * 17f));
                    return RoughBoxes[hash % RoughBoxes.Length];
                case PrimitiveType.Sphere: return sphere;
                case PrimitiveType.Cylinder: return cylinder;
                case PrimitiveType.Capsule: return capsule;
                default: return null;
            }
        }

        private static bool IsRough(string name) =>
            name.Contains("Block") || name.Contains("Rubble") || name.Contains("Break") || name.Contains("Stone") || name.Contains("Slab") || name.Contains("Rock");

        private static Mesh GetOrCreate(string name, Func<Mesh> build)
        {
            var path = $"{Folder}/{name}.asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh != null) return mesh;
            mesh = build();
            mesh.name = name;
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }

        private static Mesh BuildBox(float bevel, float jitter, int seed)
        {
            var random = new System.Random(seed);
            var inner = 0.5f - bevel;
            var offsets = new Dictionary<int, Vector3>();
            Vector3 Corner(int sx, int sy, int sz)
            {
                var key = (sx + 1) * 9 + (sy + 1) * 3 + (sz + 1);
                if (offsets.TryGetValue(key, out var offset)) return offset;
                offset = new Vector3(Next(random) * jitter, Next(random) * jitter, Next(random) * jitter);
                offset = -Vector3.Scale(new Vector3(sx, sy, sz), new Vector3(Mathf.Abs(offset.x), Mathf.Abs(offset.y), Mathf.Abs(offset.z)));
                offsets[key] = offset;
                return offset;
            }

            Vector3 X(int sx, int sy, int sz) => new Vector3(sx * 0.5f, sy * inner, sz * inner) + Corner(sx, sy, sz);
            Vector3 Y(int sx, int sy, int sz) => new Vector3(sx * inner, sy * 0.5f, sz * inner) + Corner(sx, sy, sz);
            Vector3 Z(int sx, int sy, int sz) => new Vector3(sx * inner, sy * inner, sz * 0.5f) + Corner(sx, sy, sz);

            var builder = new FlatBuilder();
            foreach (var s in new[] { -1, 1 })
            {
                builder.Polygon(X(s, -1, -1), X(s, 1, -1), X(s, 1, 1), X(s, -1, 1));
                builder.Polygon(Y(-1, s, -1), Y(1, s, -1), Y(1, s, 1), Y(-1, s, 1));
                builder.Polygon(Z(-1, -1, s), Z(1, -1, s), Z(1, 1, s), Z(-1, 1, s));
                foreach (var t in new[] { -1, 1 })
                {
                    builder.Polygon(X(s, t, -1), X(s, t, 1), Y(s, t, 1), Y(s, t, -1));
                    builder.Polygon(X(s, -1, t), X(s, 1, t), Z(s, 1, t), Z(s, -1, t));
                    builder.Polygon(Y(-1, s, t), Y(1, s, t), Z(1, s, t), Z(-1, s, t));
                    foreach (var u in new[] { -1, 1 }) builder.Polygon(X(s, t, u), Y(s, t, u), Z(s, t, u));
                }
            }

            return builder.ToMesh();
        }

        private static Mesh BuildSphere(int rings, int segments)
        {
            var builder = new FlatBuilder();
            Vector3 Point(int ring, int segment)
            {
                var polar = Mathf.PI * ring / rings;
                var azimuth = Mathf.PI * 2f * (segment + (ring % 2) * 0.5f) / segments;
                return new Vector3(Mathf.Sin(polar) * Mathf.Cos(azimuth), Mathf.Cos(polar), Mathf.Sin(polar) * Mathf.Sin(azimuth)) * 0.5f;
            }

            for (var ring = 0; ring < rings; ring++)
            for (var segment = 0; segment < segments; segment++)
            {
                var a = Point(ring, segment);
                var b = Point(ring, segment + 1);
                var c = Point(ring + 1, segment + 1);
                var d = Point(ring + 1, segment);
                if (ring > 0) builder.Polygon(a, b, d);
                if (ring < rings - 1) builder.Polygon(b, c, d);
            }

            return builder.ToMesh();
        }

        private static Mesh BuildCylinder(int segments)
        {
            var heights = new[] { -1f, -0.94f, 0.94f, 1f };
            var radii = new[] { 0.44f, 0.5f, 0.5f, 0.44f };
            return BuildLathe(heights, radii, segments);
        }

        private static Mesh BuildCapsule(int segments)
        {
            var heights = new[] { -1f, -0.9f, -0.68f, -0.5f, 0.5f, 0.68f, 0.9f, 1f };
            var radii = new[] { 0.12f, 0.3f, 0.45f, 0.5f, 0.5f, 0.45f, 0.3f, 0.12f };
            return BuildLathe(heights, radii, segments);
        }

        private static Mesh BuildLathe(float[] heights, float[] radii, int segments)
        {
            var builder = new FlatBuilder();
            Vector3 Point(int ring, int segment)
            {
                var angle = Mathf.PI * 2f * segment / segments;
                return new Vector3(Mathf.Cos(angle) * radii[ring], heights[ring], Mathf.Sin(angle) * radii[ring]);
            }

            for (var ring = 0; ring < heights.Length - 1; ring++)
            for (var segment = 0; segment < segments; segment++)
                builder.Polygon(Point(ring, segment), Point(ring, segment + 1), Point(ring + 1, segment + 1), Point(ring + 1, segment));

            foreach (var ring in new[] { 0, heights.Length - 1 })
            {
                var center = new Vector3(0f, heights[ring], 0f);
                for (var segment = 0; segment < segments; segment++) builder.Polygon(center, Point(ring, segment), Point(ring, segment + 1));
            }

            return builder.ToMesh();
        }

        private static float Next(System.Random random) => (float)random.NextDouble();

        private sealed class FlatBuilder
        {
            private readonly List<Vector3> vertices = new List<Vector3>();
            private readonly List<Vector3> normals = new List<Vector3>();
            private readonly List<Vector2> uvs = new List<Vector2>();
            private readonly List<int> triangles = new List<int>();

            public void Polygon(params Vector3[] points)
            {
                var center = Vector3.zero;
                foreach (var point in points) center += point;
                center /= points.Length;
                var normal = Vector3.Cross(points[1] - points[0], points[2] - points[0]);
                if (normal.sqrMagnitude < 1e-10f) return;
                normal.Normalize();
                if (Vector3.Dot(normal, center) < 0f)
                {
                    Array.Reverse(points);
                    normal = -normal;
                }

                var start = vertices.Count;
                foreach (var point in points)
                {
                    vertices.Add(point);
                    normals.Add(normal);
                    uvs.Add(Project(point, normal));
                }

                for (var i = 1; i < points.Length - 1; i++)
                {
                    triangles.Add(start);
                    triangles.Add(start + i);
                    triangles.Add(start + i + 1);
                }
            }

            public Mesh ToMesh()
            {
                var mesh = new Mesh();
                mesh.SetVertices(vertices);
                mesh.SetNormals(normals);
                mesh.SetUVs(0, uvs);
                mesh.SetTriangles(triangles, 0);
                mesh.RecalculateBounds();
                return mesh;
            }

            private static Vector2 Project(Vector3 point, Vector3 normal)
            {
                var absolute = new Vector3(Mathf.Abs(normal.x), Mathf.Abs(normal.y), Mathf.Abs(normal.z));
                if (absolute.y >= absolute.x && absolute.y >= absolute.z) return new Vector2(point.x + 0.5f, point.z + 0.5f);
                return absolute.x >= absolute.z ? new Vector2(point.z + 0.5f, point.y + 0.5f) : new Vector2(point.x + 0.5f, point.y + 0.5f);
            }
        }
    }
}
