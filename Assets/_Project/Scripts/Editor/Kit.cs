using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VorgGates.EditorTools
{
    /// <summary>Small helpers for assembling low-poly models and wiring serialized fields.</summary>
    internal static class Kit
    {
        private const string MeshFolder = "Assets/_Project/Art/Meshes/Kit";

        public static Transform Group(Transform parent, string name, Vector3 position = default, Vector3 euler = default)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localEulerAngles = euler;
            return go.transform;
        }

        public static GameObject Part(Transform parent, PrimitiveType type, string name, Vector3 position, Vector3 scale, Material material, Vector3 euler = default)
        {
            var go = GameObject.CreatePrimitive(type);
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localEulerAngles = euler;
            go.transform.localScale = scale;
            go.GetComponent<MeshRenderer>().sharedMaterial = material;

            // Transparent warning shapes keep exact primitive outlines. Solid parts use faceted meshes.
            if (material.renderQueue < (int)UnityEngine.Rendering.RenderQueue.Transparent)
            {
                var faceted = VorgMeshLibrary.For(type, name, position);
                if (faceted != null) go.GetComponent<MeshFilter>().sharedMesh = faceted;
            }

            return go;
        }

        public static GameObject Box(Transform parent, string name, Vector3 position, Vector3 scale, Material material, Vector3 euler = default) =>
            Part(parent, PrimitiveType.Cube, name, position, scale, material, euler);

        public static GameObject Ball(Transform parent, string name, Vector3 position, Vector3 scale, Material material, Vector3 euler = default) =>
            Part(parent, PrimitiveType.Sphere, name, position, scale, material, euler);

        public static GameObject Cylinder(Transform parent, string name, Vector3 position, Vector3 scale, Material material, Vector3 euler = default) =>
            Part(parent, PrimitiveType.Cylinder, name, position, scale, material, euler);

        public static GameObject Capsule(Transform parent, string name, Vector3 position, Vector3 scale, Material material, Vector3 euler = default) =>
            Part(parent, PrimitiveType.Capsule, name, position, scale, material, euler);

        public static BoxCollider Blocker(Transform parent, string name, Vector3 center, Vector3 size)
        {
            var go = new GameObject(name) { layer = VorgProjectSetup.WorldLayer };
            go.transform.SetParent(parent, false);
            go.transform.localPosition = center;
            var box = go.AddComponent<BoxCollider>();
            box.size = size;
            return box;
        }

        public static void SetLayer(GameObject root, int layer)
        {
            foreach (var child in root.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = layer;
        }

        public static void Wire(Object target, params (string name, object value)[] values)
        {
            var serialized = new SerializedObject(target);
            foreach (var (name, value) in values)
            {
                var property = serialized.FindProperty(name);
                if (property == null) throw new System.ArgumentException($"{target.GetType().Name} has no serialized field '{name}'.");
                switch (value)
                {
                    case Object[] array:
                        property.arraySize = array.Length;
                        for (var i = 0; i < array.Length; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = array[i];
                        break;
                    case Object reference: property.objectReferenceValue = reference; break;
                    case LayerMask mask: property.intValue = mask.value; break;
                    case float number: property.floatValue = number; break;
                    case int integer:
                        if (property.propertyType == SerializedPropertyType.Boolean) property.boolValue = integer != 0;
                        else property.intValue = integer;
                        break;
                    case string text: property.stringValue = text; break;
                    case Bounds bounds: property.boundsValue = bounds; break;
                    default: throw new System.ArgumentException($"Unsupported value for '{name}'.");
                }
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Merges every renderer under root into one mesh per material, saved as an asset.</summary>
        public static void Combine(Transform root, string assetName)
        {
            var renderers = root.GetComponentsInChildren<MeshRenderer>(true);
            var toLocal = root.worldToLocalMatrix;
            var groups = new Dictionary<Material, List<CombineInstance>>();
            foreach (var renderer in renderers)
            {
                var source = renderer.GetComponent<MeshFilter>().sharedMesh;
                var materials = renderer.sharedMaterials;
                for (var subMesh = 0; subMesh < source.subMeshCount; subMesh++)
                {
                    var material = materials[Mathf.Min(subMesh, materials.Length - 1)];
                    if (!groups.TryGetValue(material, out var list)) groups[material] = list = new List<CombineInstance>();
                    list.Add(new CombineInstance { mesh = source, subMeshIndex = subMesh, transform = toLocal * renderer.transform.localToWorldMatrix });
                }
            }

            foreach (var group in groups)
            {
                var mesh = new Mesh { name = $"{assetName}_{group.Key.name}", indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
                mesh.CombineMeshes(group.Value.ToArray(), true, true);
                if (UsesWorldMapping(group.Key)) ApplyWorldMapping(mesh);
                mesh.RecalculateBounds();
                mesh.Optimize();
                AssetDatabase.CreateAsset(mesh, $"{MeshFolder}/{mesh.name}.asset");

                var combined = new GameObject(group.Key.name);
                combined.transform.SetParent(root, false);
                combined.AddComponent<MeshFilter>().sharedMesh = mesh;
                combined.AddComponent<MeshRenderer>().sharedMaterial = group.Key;
            }

            foreach (var renderer in renderers)
            {
                if (renderer != null) Object.DestroyImmediate(renderer.gameObject);
            }

            RemoveEmptyChildren(root);
        }

        private static bool UsesWorldMapping(Material material)
        {
            if (!material.HasProperty("_BaseMap")) return false;
            var map = material.GetTexture("_BaseMap");
            return map != null && map.name.StartsWith("T_Vorg");
        }

        // Continuous surface mapping keeps cracks and grain at one density across scaled pieces.
        private static void ApplyWorldMapping(Mesh mesh)
        {
            const float density = 0.34f;
            var vertices = mesh.vertices;
            var normals = mesh.normals;
            var uv = new Vector2[vertices.Length];
            for (var i = 0; i < vertices.Length; i++)
            {
                var n = normals[i];
                var absolute = new Vector3(Mathf.Abs(n.x), Mathf.Abs(n.y), Mathf.Abs(n.z));
                var p = vertices[i];
                if (absolute.y >= absolute.x && absolute.y >= absolute.z) uv[i] = new Vector2(p.x, p.z) * density;
                else if (absolute.x >= absolute.z) uv[i] = new Vector2(p.z, p.y) * density;
                else uv[i] = new Vector2(p.x, p.y) * density;
            }

            mesh.uv = uv;
        }

        private static void RemoveEmptyChildren(Transform root)
        {
            var transforms = root.GetComponentsInChildren<Transform>(true);
            for (var i = transforms.Length - 1; i >= 0; i--)
            {
                var candidate = transforms[i];
                if (candidate == root || candidate.childCount > 0 || candidate.GetComponents<Component>().Length > 1) continue;
                Object.DestroyImmediate(candidate.gameObject);
            }
        }

        public static void ResetMeshFolder()
        {
            if (AssetDatabase.IsValidFolder(MeshFolder)) AssetDatabase.DeleteAsset(MeshFolder);
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Art/Meshes")) AssetDatabase.CreateFolder("Assets/_Project/Art", "Meshes");
            AssetDatabase.CreateFolder("Assets/_Project/Art/Meshes", "Kit");
        }

        public static T SaveAsPrefab<T>(GameObject root, string path) where T : Object
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return typeof(T) == typeof(GameObject) ? prefab as T : prefab.GetComponent<T>();
        }
    }
}
