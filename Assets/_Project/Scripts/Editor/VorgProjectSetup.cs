using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace VorgGates.EditorTools
{
    /// <summary>Idempotent project configuration for layers, quality and URP.</summary>
    internal static class VorgProjectSetup
    {
        public const int WorldLayer = 6;
        public const int PlayerLayer = 7;
        public const int EnemyLayer = 8;
        public const int PlayerHitboxLayer = 9;
        public const int EnemyProjectileLayer = 10;
        public const int InteractableLayer = 11;

        private static readonly string[] LayerNames = { "World", "Player", "Enemy", "PlayerHitbox", "EnemyProjectile", "Interactable" };

        public static void Apply()
        {
            ConfigureLayers();
            ConfigureCollisionMatrix();
            ConfigurePipeline();
            PlayerSettings.productName = "Vorg'Gates";
            QualitySettings.vSyncCount = 1;
            AssetDatabase.SaveAssets();
        }

        private static void ConfigureLayers()
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tagManager.FindProperty("layers");
            for (var i = 0; i < LayerNames.Length; i++)
            {
                var layer = layers.GetArrayElementAtIndex(WorldLayer + i);
                if (!string.IsNullOrEmpty(layer.stringValue) && layer.stringValue != LayerNames[i])
                    throw new System.InvalidOperationException($"Layer {WorldLayer + i} is already used by '{layer.stringValue}'.");
                layer.stringValue = LayerNames[i];
            }

            tagManager.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureCollisionMatrix()
        {
            for (var a = WorldLayer; a <= InteractableLayer; a++)
            for (var b = a; b <= InteractableLayer; b++)
                Physics.IgnoreLayerCollision(a, b, true);

            Physics.IgnoreLayerCollision(WorldLayer, PlayerLayer, false);
            Physics.IgnoreLayerCollision(WorldLayer, EnemyLayer, false);
            Physics.IgnoreLayerCollision(WorldLayer, EnemyProjectileLayer, false);
            Physics.IgnoreLayerCollision(PlayerLayer, EnemyProjectileLayer, false);
            Physics.IgnoreLayerCollision(EnemyLayer, PlayerHitboxLayer, false);
        }

        private static void ConfigurePipeline()
        {
            var asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/PC_RPAsset.asset");
            asset.shadowDistance = 26f;
            asset.shadowCascadeCount = 2;
            asset.msaaSampleCount = 2;
            asset.supportsCameraDepthTexture = false;
            asset.supportsCameraOpaqueTexture = false;

            var serialized = new SerializedObject(asset);
            var additionalShadows = serialized.FindProperty("m_AdditionalLightShadowsSupported");
            if (additionalShadows != null) additionalShadows.boolValue = false;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);

            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/Settings/PC_Renderer.asset");
            RemoveAmbientOcclusion(renderer);
            EditorUtility.SetDirty(renderer);
        }

        // A disabled SSAO feature still needs resources that the player build strips, so it must be removed.
        private static void RemoveAmbientOcclusion(UniversalRendererData renderer)
        {
            var serialized = new SerializedObject(renderer);
            var features = serialized.FindProperty("m_RendererFeatures");
            var map = serialized.FindProperty("m_RendererFeatureMap");
            for (var i = features.arraySize - 1; i >= 0; i--)
            {
                var feature = features.GetArrayElementAtIndex(i).objectReferenceValue;
                if (feature == null || !feature.GetType().Name.Contains("ScreenSpaceAmbientOcclusion")) continue;

                var size = features.arraySize;
                features.DeleteArrayElementAtIndex(i);
                if (features.arraySize == size) features.DeleteArrayElementAtIndex(i);
                map.DeleteArrayElementAtIndex(i);
                serialized.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.RemoveObjectFromAsset(feature);
                Object.DestroyImmediate(feature, true);
            }
        }
    }
}
