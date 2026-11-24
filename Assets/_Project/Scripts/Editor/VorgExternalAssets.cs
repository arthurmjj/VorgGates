using System.IO;
using UnityEditor;
using UnityEngine;

namespace VorgGates.EditorTools
{
    /// <summary>Places CC0 Kenney models as visual-only pieces using each phase's URP materials.</summary>
    internal static class VorgExternalAssets
    {
        public const string Root = "Assets/_Project/Art/External/Kenney";

        public static void Prepare()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Model", new[] { Root }))
            {
                var importer = (ModelImporter)AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid));
                if (importer.isReadable && !importer.importAnimation && !importer.addCollider && !importer.importCameras && !importer.importLights) continue;
                importer.isReadable = true;
                importer.importAnimation = false;
                importer.addCollider = false;
                importer.importCameras = false;
                importer.importLights = false;
                importer.animationType = ModelImporterAnimationType.None;
                importer.SaveAndReimport();
            }

            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { Root }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                var isParticle = path.Contains("/Particles/");
                var size = isParticle ? 128 : 256;
                if (importer.maxTextureSize == size) continue;
                importer.maxTextureSize = size;
                if (isParticle) importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }
        }

        public static Texture2D Colormap(string kit) => AssetDatabase.LoadAssetAtPath<Texture2D>($"{Root}/{kit}/Textures/colormap.png");

        public static Texture2D Particle(string name) => AssetDatabase.LoadAssetAtPath<Texture2D>($"{Root}/Particles/{name}.png");

        /// <summary>Instantiates a model without colliders. The model name is "Kit/file".</summary>
        public static GameObject Place(Transform parent, string model, Vector3 position, float yaw, float scale, VorgArt art)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>($"{Root}/{model}.fbx");
            if (source == null) throw new FileNotFoundException($"Missing external model: {model}");

            var instance = Object.Instantiate(source, parent, false);
            instance.name = Path.GetFileName(model);
            instance.transform.localPosition = position;
            instance.transform.localRotation = Quaternion.Euler(0f, yaw, 0f) * source.transform.localRotation;
            instance.transform.localScale = source.transform.localScale * scale;

            var kit = model.Substring(0, model.IndexOf('/'));
            foreach (var renderer in instance.GetComponentsInChildren<MeshRenderer>(true))
            {
                var materials = renderer.sharedMaterials;
                for (var i = 0; i < materials.Length; i++)
                    materials[i] = kit == "Nature" ? art.NatureMaterial(materials[i] != null ? materials[i].name : string.Empty) : art.KitMaterial(kit);
                renderer.sharedMaterials = materials;
            }

            foreach (var collider in instance.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(collider);
            return instance;
        }
    }
}
