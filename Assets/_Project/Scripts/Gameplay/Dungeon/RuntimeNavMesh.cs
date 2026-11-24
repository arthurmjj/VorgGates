using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace VorgGates.Gameplay
{
    /// <summary>Builds a NavMesh at runtime from world colliders, without extra packages.</summary>
    public sealed class RuntimeNavMesh : MonoBehaviour
    {
        private const float VoxelSize = 0.2f;
        private const float MaxClimb = 0.2f;
        private const float MinRegionArea = 8f;
        private const int NotWalkableArea = 1;

        [SerializeField] private LayerMask sourceMask;

        private readonly List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>(512);
        private readonly List<NavMeshBuildMarkup> markups = new List<NavMeshBuildMarkup>();
        private NavMeshData data;
        private NavMeshDataInstance instance;

        public bool Build(Bounds bounds)
        {
            Clear();
            Physics.SyncTransforms();
            sources.Clear();
            NavMeshBuilder.CollectSources(bounds, sourceMask, NavMeshCollectGeometry.PhysicsColliders, 0, markups, sources);

            // Only the floor sits below zero. Everything above it blocks walking, including its top face.
            for (var i = 0; i < sources.Count; i++)
            {
                var source = sources[i];
                if (source.transform.GetColumn(3).y <= 0f) continue;
                source.area = NotWalkableArea;
                sources[i] = source;
            }

            var settings = NavMesh.GetSettingsByID(0);
            settings.overrideVoxelSize = true;
            settings.voxelSize = VoxelSize;

            // Props are not walkable: no climbing onto them and no small islands on their tops.
            settings.agentClimb = MaxClimb;
            settings.minRegionArea = MinRegionArea;
            data = NavMeshBuilder.BuildNavMeshData(settings, sources, bounds, Vector3.zero, Quaternion.identity);
            if (data == null)
            {
                Debug.LogError("Vorg'Gates: failed to build the dungeon NavMesh.");
                return false;
            }

            instance = NavMesh.AddNavMeshData(data);
            return instance.valid;
        }

        public void Clear()
        {
            if (instance.valid) instance.Remove();
            if (data != null) Destroy(data);
            data = null;
        }

        private void OnDestroy() => Clear();
    }
}
