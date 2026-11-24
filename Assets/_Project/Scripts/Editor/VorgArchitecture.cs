using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VorgGates.EditorTools
{
    /// <summary>
    /// Replaces the visual shell of each phase with its own CC0 modular kit. Pieces are fitted to the
    /// existing logical dimensions and never add colliders, so rooms play exactly as before.
    /// </summary>
    internal static class VorgArchitecture
    {
        public const string Root = "Assets/_Project/Art/External/M7";

        private const float RoomHalf = 9f;
        private const float RoomTile = 4.5f;
        private const float FloorDepth = 0.16f;
        private const float TallLimit = 2f;

        private static readonly string[] CryptWalls = { "Crypt/wall_large_03", "Crypt/wall_large_01", "Crypt/wall_large_02", "Crypt/wall_large_01" };
        private static readonly string[] GothicWalls = { "Cathedral/wall_window_01", "Cathedral/wall_01", "Cathedral/wall_window_large_01", "Cathedral/wall_01" };
        private static readonly Dictionary<string, (GameObject source, Bounds bounds)> Pieces = new Dictionary<string, (GameObject, Bounds)>();

        public static void Prepare()
        {
            Pieces.Clear();
            foreach (var guid in AssetDatabase.FindAssets("t:Model", new[] { Root }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("/Knight/")) continue;
                var importer = (ModelImporter)AssetImporter.GetAtPath(path);
                if (importer.isReadable && !importer.importAnimation && !importer.addCollider && !importer.importCameras && !importer.importLights) continue;
                importer.isReadable = true;
                importer.importAnimation = false;
                importer.addCollider = false;
                importer.importCameras = false;
                importer.importLights = false;
                importer.animationType = ModelImporterAnimationType.None;
                importer.SaveAndReimport();
            }
        }

        public static Texture2D Texture(string path) => AssetDatabase.LoadAssetAtPath<Texture2D>($"{Root}/{path}");

        /// <summary>Places a piece with its base center at position, scaled to the requested size.</summary>
        public static GameObject Fit(Transform parent, string model, Vector3 position, float yaw, Vector3 size, Material primary, Material secondary = null)
        {
            var (source, bounds) = Load(model);
            var wrap = Kit.Group(parent, source.name, position, new Vector3(0f, yaw, 0f));
            wrap.localScale = new Vector3(size.x / bounds.size.x, size.y / bounds.size.y, size.z / bounds.size.z);

            var instance = Object.Instantiate(source, wrap, false);
            instance.name = source.name;
            instance.transform.localRotation = source.transform.localRotation;
            instance.transform.localScale = source.transform.localScale;
            instance.transform.localPosition = -new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);

            foreach (var renderer in instance.GetComponentsInChildren<MeshRenderer>(true))
            {
                var materials = renderer.sharedMaterials;
                for (var i = 0; i < materials.Length; i++)
                {
                    var isWindow = materials[i] != null && materials[i].name.ToLowerInvariant().Contains("window");
                    materials[i] = isWindow && secondary != null ? secondary : primary;
                }

                renderer.sharedMaterials = materials;
            }

            foreach (var collider in instance.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(collider);
            return wrap.gameObject;
        }

        public static void RoomFloor(Transform parent, VorgArt art)
        {
            for (var i = 0; i < 4; i++)
            for (var j = 0; j < 4; j++)
            {
                var x = -RoomHalf + RoomTile * (i + 0.5f);
                var z = -RoomHalf + RoomTile * (j + 0.5f);
                var size = new Vector3(RoomTile, FloorDepth, RoomTile);
                Fit(parent, RoomFloorModel(art.Theme, i, j), new Vector3(x, -FloorDepth, z), 0f, size, art.ArchFloor);
            }

            if (art.Theme != VorgTheme.Forge) return;
            for (var i = 0; i < 4; i++)
            for (var j = 0; j < 4; j++)
            {
                if ((i + j) % 2 != 0) continue;
                var x = -RoomHalf + RoomTile * (i + 0.5f);
                var z = -RoomHalf + RoomTile * (j + 0.5f);
                Fit(parent, "Forge/floor_grate_cross", new Vector3(x, -0.1f, z), 0f, new Vector3(3.3f, 0.115f, 3.3f), art.ArchBeam);
            }
        }

        public static void CorridorFloor(Transform parent, VorgArt art)
        {
            var model = art.Theme == VorgTheme.Crypt ? "Crypt/floor_large_01" : art.Theme == VorgTheme.Forge ? "Crypt/floor_large_trim_01" : "Cathedral/floor_02";
            for (var x = -1f; x <= 1f; x += 2f)
            for (var z = -2f; z <= 2f; z += 2f)
                Fit(parent, model, new Vector3(x, -FloorDepth, z), 0f, new Vector3(2f, FloorDepth, 2f), art.ArchFloor);
            if (art.Theme == VorgTheme.Forge) Fit(parent, "Forge/floor_grate_straight", new Vector3(0f, -0.1f, 0f), 0f, new Vector3(2f, 0.115f, 5f), art.ArchTrim);
        }

        public static void Wall(Transform parent, Vector3 start, Vector3 end, float height, VorgArt art)
        {
            var delta = end - start;
            var length = delta.magnitude;
            var direction = delta / length;
            var center = (start + end) * 0.5f;
            var inward = -(center - direction * Vector3.Dot(center, direction));
            inward.y = 0f;
            if (inward.sqrMagnitude < 1e-4f) inward = Vector3.Cross(direction, Vector3.up);
            inward.Normalize();
            var yaw = Quaternion.LookRotation(-inward, Vector3.up).eulerAngles.y;
            var run = Kit.Group(parent, "WallRun");
            var tall = height > TallLimit;

            // The backing stays below every worn top edge, and gothic windows stay open to show their glass.
            if (!(tall && art.Theme == VorgTheme.Sanctuary))
            {
                var backing = height * 0.72f;
                Kit.Box(run, "Backing", center + Vector3.up * (backing * 0.5f), new Vector3(0.5f, backing, length), art.Abyss, Quaternion.LookRotation(direction, Vector3.up).eulerAngles);
            }

            switch (art.Theme)
            {
                case VorgTheme.Forge:
                    if (tall) ForgeWall(run, start, direction, length, height, yaw, inward, art);
                    else ForgeRail(run, start, direction, length, height, yaw, inward, art);
                    break;
                case VorgTheme.Sanctuary:
                    if (tall) Repeat(run, GothicWalls, "Cathedral/wall_rose_window_01", start, direction, length, 2.5f, height, 0.96f, yaw, art);
                    else GothicBalustrade(run, start, direction, length, height, yaw, inward, art);
                    break;
                default:
                    if (tall) Repeat(run, CryptWalls, null, start, direction, length, 4f, height, 1f, yaw, art);
                    else CryptCurb(run, start, direction, length, height, yaw, art);
                    break;
            }
        }

        public static void Pillar(Transform parent, Vector3 position, float height, VorgArt art)
        {
            var pillar = Kit.Group(parent, "Pillar", position);
            switch (art.Theme)
            {
                case VorgTheme.Forge:
                    Fit(pillar, "Forge/platform_post", Vector3.zero, 0f, new Vector3(1.25f, height, 1.25f), art.ArchBeam);
                    foreach (var yaw in new[] { 0f, 90f, 180f, 270f })
                    {
                        var outward = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward * 0.62f;
                        Fit(pillar, "Forge/platform_bracket", outward + Vector3.up * (height - 0.85f), yaw, new Vector3(0.18f, 0.8f, 0.72f), art.ArchTrim);
                    }

                    break;
                case VorgTheme.Sanctuary:
                    Fit(pillar, "Cathedral/column_small_01", Vector3.zero, 0f, new Vector3(1.3f, height + 0.3f, 1.3f), art.ArchTrim);
                    break;
                default:
                    Fit(pillar, height > TallLimit ? "Crypt/pillar_01" : "Crypt/pillar_02", Vector3.zero, 0f, new Vector3(1.42f, height + 0.12f, 1.42f), art.ArchTrim);
                    break;
            }

            VorgDressing.PillarDetail(pillar, height, art);
        }

        public static void ShortColumn(Transform parent, VorgArt art)
        {
            switch (art.Theme)
            {
                case VorgTheme.Forge:
                    Fit(parent, "Forge/platform_post", Vector3.zero, 0f, new Vector3(1.3f, 1.5f, 1.3f), art.ArchBeam);
                    break;
                case VorgTheme.Sanctuary:
                    Fit(parent, "Cathedral/column_small_01", Vector3.zero, 0f, new Vector3(1.3f, 1.6f, 1.3f), art.ArchTrim);
                    break;
                default:
                    Fit(parent, "Crypt/pillar_01", Vector3.zero, 0f, new Vector3(1.3f, 1.5f, 1.3f), art.ArchTrim);
                    break;
            }
        }

        public static void Sarcophagus(Transform parent, VorgArt art)
        {
            Fit(parent, "Crypt/coffin", Vector3.zero, 0f, new Vector3(3.05f, 0.76f, 1.55f), art.CoffinStone);
            Fit(parent, "Crypt/coffin_lid", Vector3.up * 0.72f, 0f, new Vector3(3.2f, 0.26f, 1.72f), art.CoffinStone);
        }

        private static string RoomFloorModel(VorgTheme theme, int i, int j)
        {
            var edgeX = i == 0 || i == 3;
            var edgeZ = j == 0 || j == 3;
            switch (theme)
            {
                case VorgTheme.Forge:
                    return (i + j) % 2 == 0 ? "Crypt/floor_large_trim_01" : "Crypt/floor_large_trim_02";
                case VorgTheme.Sanctuary:
                    return edgeX || edgeZ ? "Cathedral/floor_01" : "Cathedral/floor_03";
                default:
                    if (edgeX && edgeZ) return $"Crypt/floor_large_cornertrim_{(j == 3 ? "T" : "B")}{(i == 0 ? "L" : "R")}_01";
                    if (edgeX) return i == 0 ? "Crypt/floor_large_trim_L_01" : "Crypt/floor_large_trim_R_01";
                    if (edgeZ) return j == 3 ? "Crypt/floor_large_trim_T_01" : "Crypt/floor_large_trim_B_01";
                    return (i + j) % 2 == 0 ? "Crypt/floor_large_01" : "Crypt/floor_large_trim_02";
            }
        }

        private static void Repeat(Transform run, string[] models, string centerModel, Vector3 start, Vector3 direction, float length, float width, float height, float depth, float yaw, VorgArt art)
        {
            var count = Mathf.Max(1, Mathf.RoundToInt(length / width));
            if (centerModel != null && count > 3 && count % 2 == 0) count++;
            var pieceWidth = length / count;
            for (var i = 0; i < count; i++)
            {
                var model = centerModel != null && count % 2 == 1 && i == count / 2 ? centerModel : models[i % models.Length];
                Fit(run, model, start + direction * (pieceWidth * (i + 0.5f)), yaw, new Vector3(pieceWidth, height, depth), art.ArchTrim, art.ArchWindow);
            }
        }

        private static void CryptCurb(Transform run, Vector3 start, Vector3 direction, float length, float height, float yaw, VorgArt art)
        {
            var count = Mathf.Max(1, Mathf.RoundToInt(length / 2f));
            var width = length / count;
            for (var i = 0; i < count; i++)
            {
                var worn = 1f - (i * 7 % 3) * 0.07f;
                Fit(run, "Crypt/block_short_small_01", start + direction * (width * (i + 0.5f)), yaw, new Vector3(width, height * worn, 1f), art.ArchTrim);
            }
        }

        private static void GothicBalustrade(Transform run, Vector3 start, Vector3 direction, float length, float height, float yaw, Vector3 inward, VorgArt art)
        {
            var count = Mathf.Max(1, Mathf.RoundToInt(length / 2.5f));
            var width = length / count;
            for (var i = 0; i < count; i++)
            {
                var point = start + direction * (width * (i + 0.5f));
                Fit(run, "Cathedral/wall_trim_01", point, yaw, new Vector3(width, height * 0.42f, 1f), art.ArchTrim);
                Fit(run, "Cathedral/railing_01", point + Vector3.up * (height * 0.42f), yaw, new Vector3(width, height * 0.62f, 0.58f), art.ArchTrim);
            }
        }

        private static void ForgeWall(Transform run, Vector3 start, Vector3 direction, float length, float height, float yaw, Vector3 inward, VorgArt art)
        {
            const float bay = 2.5f;
            var count = Mathf.Max(1, Mathf.RoundToInt(length / bay));
            var width = length / count;
            for (var i = 0; i < count; i++)
            {
                var point = start + direction * (width * (i + 0.5f));
                Fit(run, "Forge/wall_wood", point, yaw, new Vector3(width, height, 0.9f), art.ArchTrim);
                Fit(run, "Forge/platform_beam", point + inward * 0.46f + Vector3.up * (height * 0.36f), yaw, new Vector3(width, 0.17f, 0.17f), art.ArchBeam);
                Fit(run, "Forge/platform_beam", point + inward * 0.46f + Vector3.up * (height - 0.18f), yaw, new Vector3(width, 0.24f, 0.2f), art.ArchBeam);
            }

            for (var i = 0; i <= count; i++)
            {
                var point = start + direction * (width * i) + inward * 0.42f;
                Fit(run, "Forge/platform_post", point, yaw, new Vector3(0.36f, height, 0.3f), art.ArchBeam);
                Fit(run, "Forge/platform_bracket", point + inward * 0.1f + Vector3.up * (height - 0.92f), yaw + 180f, new Vector3(0.12f, 0.72f, 0.5f), art.ArchTrim);
            }
        }

        private static void ForgeRail(Transform run, Vector3 start, Vector3 direction, float length, float height, float yaw, Vector3 inward, VorgArt art)
        {
            var count = Mathf.Max(1, Mathf.RoundToInt(length / 2f));
            var width = length / count;
            for (var i = 0; i < count; i++)
                Fit(run, "Forge/platform_rail", start + direction * (width * (i + 0.5f)), yaw, new Vector3(width, height, 0.42f), art.ArchBeam);
            for (var i = 0; i <= count; i++)
                Fit(run, "Forge/platform_post", start + direction * (width * i), yaw, new Vector3(0.34f, height + 0.08f, 0.34f), art.ArchTrim);
        }

        private static (GameObject source, Bounds bounds) Load(string model)
        {
            if (Pieces.TryGetValue(model, out var cached)) return cached;
            var source = AssetDatabase.LoadAssetAtPath<GameObject>($"{Root}/{model}.fbx") ?? AssetDatabase.LoadAssetAtPath<GameObject>($"{Root}/{model}.obj");
            if (source == null) throw new System.IO.FileNotFoundException($"Missing architecture piece: {model}");

            var toRoot = Matrix4x4.TRS(Vector3.zero, source.transform.localRotation, source.transform.localScale) * source.transform.worldToLocalMatrix;
            var min = Vector3.positiveInfinity;
            var max = Vector3.negativeInfinity;
            foreach (var filter in source.GetComponentsInChildren<MeshFilter>(true))
            {
                var matrix = toRoot * filter.transform.localToWorldMatrix;
                foreach (var vertex in filter.sharedMesh.vertices)
                {
                    var point = matrix.MultiplyPoint3x4(vertex);
                    min = Vector3.Min(min, point);
                    max = Vector3.Max(max, point);
                }
            }

            var bounds = new Bounds((min + max) * 0.5f, max - min);
            var entry = (source, bounds);
            Pieces[model] = entry;
            return entry;
        }
    }
}
