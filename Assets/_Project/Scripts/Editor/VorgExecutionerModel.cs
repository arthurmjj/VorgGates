using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VorgGates.Gameplay;
using VorgGates.Presentation;

namespace VorgGates.EditorTools
{
    /// <summary>
    /// Skins the static "Undead Knight [Low-Poly]" mesh by SchrisArt (CC BY 4.0) to a custom skeleton.
    /// The artist's geometry is kept. The rig is visual only and has no colliders.
    /// </summary>
    internal static class VorgExecutionerModel
    {
        private const string Folder = VorgArchitecture.Root + "/UndeadKnight";
        private const string ModelPath = Folder + "/undead_knight.obj";
        private const string SkinnedPath = Folder + "/SM_Executioner.asset";
        private const float SourceHeight = 11.35f;
        private const float SourceFloor = 5.89f;

        private const int Hips = 0, Spine = 1, Head = 2, UpperA = 3, ForeA = 4, HandA = 5, UpperB = 6, ForeB = 7, HandB = 8;
        private const int ThighA = 9, ShinA = 10, FootA = 11, ThighB = 12, ShinB = 13, FootB = 14, CapeUpper = 15, CapeLower = 16;

        // Joints measured on the source mesh: parent, start and end of each bone.
        private static readonly (string name, int parent, Vector3 start, Vector3 end)[] Bones =
        {
            ("Hips", -1, new Vector3(-0.12f, -0.75f, 0.3f), new Vector3(-0.3f, 0.5f, 0.75f)),
            ("Spine", Hips, new Vector3(-0.3f, 0.5f, 0.75f), new Vector3(-0.85f, 3.05f, 2.15f)),
            ("Head", Spine, new Vector3(-0.88f, 3.2f, 2.35f), new Vector3(-0.92f, 5.3f, 2.9f)),
            ("UpperArmA", Spine, new Vector3(-1.9f, 3.1f, 1.25f), new Vector3(-3.0f, 1.4f, 0.6f)),
            ("ForearmA", UpperA, new Vector3(-3.0f, 1.4f, 0.6f), new Vector3(-3.75f, 0.4f, 0.85f)),
            ("HandA", ForeA, new Vector3(-3.75f, 0.4f, 0.85f), new Vector3(-4.1f, -0.45f, 1f)),
            ("UpperArmB", Spine, new Vector3(0.55f, 3.2f, 2f), new Vector3(1.55f, 1.75f, 2.8f)),
            ("ForearmB", UpperB, new Vector3(1.55f, 1.75f, 2.8f), new Vector3(1.7f, 1.05f, 3.35f)),
            ("HandB", ForeB, new Vector3(1.7f, 1.05f, 3.35f), new Vector3(1.55f, 0.25f, 4.35f)),
            ("ThighA", Hips, new Vector3(-0.63f, -0.95f, -0.13f), new Vector3(-0.92f, -2.75f, -0.1f)),
            ("ShinA", ThighA, new Vector3(-0.92f, -2.75f, -0.1f), new Vector3(-0.62f, -4.45f, -2.3f)),
            ("FootA", ShinA, new Vector3(-0.62f, -4.45f, -2.3f), new Vector3(-0.8f, -5.75f, -2.7f)),
            ("ThighB", Hips, new Vector3(0.38f, -1f, 0.8f), new Vector3(0.1f, -2.9f, 1.5f)),
            ("ShinB", ThighB, new Vector3(0.1f, -2.9f, 1.5f), new Vector3(0.48f, -4.5f, 0.85f)),
            ("FootB", ShinB, new Vector3(0.48f, -4.5f, 0.85f), new Vector3(0.43f, -5.75f, 1.6f)),
            ("CapeUpper", Spine, new Vector3(-0.6f, 3.3f, 0.9f), new Vector3(1f, 0.6f, -1.6f)),
            ("CapeLower", CapeUpper, new Vector3(1f, 0.6f, -1.6f), new Vector3(3.4f, -3.2f, -5f))
        };

        public static void Prepare()
        {
            var model = (ModelImporter)AssetImporter.GetAtPath(ModelPath);
            if (!model.isReadable || model.addCollider || model.importAnimation || model.importTangents != ModelImporterTangents.CalculateMikk)
            {
                model.isReadable = true;
                model.addCollider = false;
                model.importAnimation = false;
                model.animationType = ModelImporterAnimationType.None;
                model.importTangents = ModelImporterTangents.CalculateMikk;
                model.SaveAndReimport();
            }

            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { Folder + "/Textures" }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                var isNormal = path.EndsWith("_normal.png");
                var size = path.EndsWith("_baseColor.png") ? 1024 : 512;
                var type = isNormal ? TextureImporterType.NormalMap : TextureImporterType.Default;
                if (importer.textureType == type && importer.maxTextureSize == size) continue;
                importer.textureType = type;
                importer.maxTextureSize = size;
                importer.SaveAndReimport();
            }

            BuildSkinnedMesh();
        }

        /// <summary>Adds the skinned model under body and the pose driver on the boss root.</summary>
        public static System.Action<EnemyBrain> Place(GameObject root, Transform body, float height, VorgArt art)
        {
            var scale = height / SourceHeight;
            var model = Kit.Group(body, "ExecutionerModel", new Vector3(0.12f * scale, SourceFloor * scale, -0.3f * scale));
            model.localScale = new Vector3(scale * 1.2f, scale, scale * 1.14f);

            var bones = new Transform[Bones.Length];
            for (var i = 0; i < Bones.Length; i++)
            {
                var bone = Bones[i];
                var parent = bone.parent < 0 ? model : bones[bone.parent];
                var parentStart = bone.parent < 0 ? Vector3.zero : Bones[bone.parent].start;
                bones[i] = Kit.Group(parent, bone.name, bone.start - parentStart);
            }

            var skin = new GameObject("ExecutionerSkin");
            skin.transform.SetParent(model, false);
            var renderer = skin.AddComponent<SkinnedMeshRenderer>();
            renderer.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(SkinnedPath);
            renderer.bones = bones;
            renderer.rootBone = bones[Hips];
            renderer.quality = SkinQuality.Bone4;
            renderer.localBounds = new Bounds(Vector3.up * 0.2f, new Vector3(18f, 16f, 18f));
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath).GetComponentInChildren<MeshRenderer>().sharedMaterials;
            var materials = new Material[source.Length];
            for (var i = 0; i < materials.Length; i++) materials[i] = art.ExecutionerMaterial(source[i].name);
            renderer.sharedMaterials = materials;

            var aura = Kit.Group(root.transform, "ForgeAura");
            VorgFx.Sparkles(aura, "RisingEmbers", art.MagicSpark, Vector3.up * 0.25f, 1.15f, 13f, 0.17f, 1.35f, 30);
            var light = new GameObject("ForgeHeartLight").AddComponent<Light>();
            light.transform.SetParent(aura, false);
            light.transform.localPosition = new Vector3(0f, 2.1f, 0.85f);
            light.type = LightType.Point;
            light.color = VorgArt.Hex("#FF6A24");
            light.range = 5.5f;
            light.intensity = 2.1f;
            light.shadows = LightShadows.None;

            return brain => Kit.Wire(root.AddComponent<BossRigPose>(),
                ("brain", brain), ("hips", bones[Hips]), ("spine", bones[Spine]), ("head", bones[Head]),
                ("upperArmA", bones[UpperA]), ("forearmA", bones[ForeA]), ("upperArmB", bones[UpperB]), ("forearmB", bones[ForeB]),
                ("thighA", bones[ThighA]), ("shinA", bones[ShinA]), ("thighB", bones[ThighB]), ("shinB", bones[ShinB]),
                ("capeUpper", bones[CapeUpper]), ("capeLower", bones[CapeLower]));
        }

        private static void BuildSkinnedMesh()
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath).GetComponentInChildren<MeshFilter>().sharedMesh;
            var vertices = source.vertices;
            var weights = new BoneWeight[vertices.Length];
            var names = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath).GetComponentInChildren<MeshRenderer>().sharedMaterials;
            for (var s = 0; s < source.subMeshCount; s++)
            {
                var part = names[s].name;
                foreach (var index in source.GetTriangles(s))
                    weights[index] = Weigh(vertices[index], part);
            }

            var mesh = Object.Instantiate(source);
            mesh.name = "SM_Executioner";
            mesh.boneWeights = weights;
            var binds = new Matrix4x4[Bones.Length];
            for (var i = 0; i < binds.Length; i++) binds[i] = Matrix4x4.Translate(-Bones[i].start);
            mesh.bindposes = binds;
            mesh.RecalculateBounds();
            if (AssetDatabase.LoadAssetAtPath<Mesh>(SkinnedPath) != null) AssetDatabase.DeleteAsset(SkinnedPath);
            AssetDatabase.CreateAsset(mesh, SkinnedPath);
        }

        private static BoneWeight Weigh(Vector3 point, string part)
        {
            if (part.StartsWith("Claymore")) return new BoneWeight { boneIndex0 = HandA, weight0 = 1f };
            if (part == "Cape") return WeighCape(point);

            var candidates = new List<(int bone, float score)>(Bones.Length);
            for (var i = 0; i < CapeUpper; i++)
            {
                var distance = SegmentDistance(point, Bones[i].start, Bones[i].end);
                candidates.Add((i, 1f / (Mathf.Pow(distance + 0.08f, 6f) + 1e-5f)));
            }

            candidates.Sort((a, b) => b.score.CompareTo(a.score));
            var total = candidates[0].score + candidates[1].score + candidates[2].score;
            return new BoneWeight
            {
                boneIndex0 = candidates[0].bone,
                boneIndex1 = candidates[1].bone,
                boneIndex2 = candidates[2].bone,
                weight0 = candidates[0].score / total,
                weight1 = candidates[1].score / total,
                weight2 = candidates[2].score / total
            };
        }

        private static BoneWeight WeighCape(Vector3 point)
        {
            var collar = Bones[CapeUpper].start;
            var reach = Mathf.Clamp01(Vector3.Distance(point, collar) / 8.5f);
            var spine = Mathf.Clamp01(1f - reach * 3.4f);
            var lower = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.32f, 0.82f, reach));
            var upper = Mathf.Max(0f, 1f - spine - lower);
            return new BoneWeight { boneIndex0 = Spine, weight0 = spine, boneIndex1 = CapeUpper, weight1 = upper, boneIndex2 = CapeLower, weight2 = lower };
        }

        private static float SegmentDistance(Vector3 point, Vector3 a, Vector3 b)
        {
            var ab = b - a;
            var t = Mathf.Clamp01(Vector3.Dot(point - a, ab) / ab.sqrMagnitude);
            return Vector3.Distance(point, a + ab * t);
        }
    }
}
