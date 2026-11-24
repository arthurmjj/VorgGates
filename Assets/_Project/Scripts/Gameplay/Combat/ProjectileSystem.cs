using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace VorgGates.Gameplay
{
    public sealed class ProjectileSystem : MonoBehaviour
    {
        private const int DefaultCapacity = 16;
        private const int MaxPooled = 40;

        private readonly Dictionary<Projectile, ObjectPool<Projectile>> pools = new Dictionary<Projectile, ObjectPool<Projectile>>();
        private readonly List<Projectile> active = new List<Projectile>(MaxPooled);

        public int ActiveCount => active.Count;

        public void Fire(Projectile prefab, in ProjectileShot shot)
        {
            if (!pools.TryGetValue(prefab, out var pool))
            {
                pool = new ObjectPool<Projectile>(
                    () => Instantiate(prefab, transform),
                    item => item.gameObject.SetActive(true),
                    item => item.gameObject.SetActive(false),
                    item =>
                    {
                        // Unity clears pools on exit, when the projectiles may already be gone.
                        if (item != null) Destroy(item.gameObject);
                    },
                    false, DefaultCapacity, MaxPooled);
                pools.Add(prefab, pool);
            }

            var projectile = pool.Get();
            projectile.Launch(this, prefab, shot);
            active.Add(projectile);
        }

        internal void Release(Projectile projectile)
        {
            if (!active.Remove(projectile)) return;
            pools[projectile.SourcePrefab].Release(projectile);
        }

        public void ClearAll()
        {
            for (var i = active.Count - 1; i >= 0; i--) Release(active[i]);
        }
    }
}
