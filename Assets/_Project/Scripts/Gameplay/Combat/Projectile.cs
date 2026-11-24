using UnityEngine;

namespace VorgGates.Gameplay
{
    public readonly struct ProjectileShot
    {
        public ProjectileShot(Vector3 origin, Vector3 direction, float speed, float range, int damage, float knockback, LayerMask targetMask, LayerMask obstacleMask)
        {
            Origin = origin;
            Direction = direction;
            Speed = speed;
            Range = range;
            Damage = damage;
            Knockback = knockback;
            TargetMask = targetMask;
            ObstacleMask = obstacleMask;
        }

        public Vector3 Origin { get; }
        public Vector3 Direction { get; }
        public float Speed { get; }
        public float Range { get; }
        public int Damage { get; }
        public float Knockback { get; }
        public LayerMask TargetMask { get; }
        public LayerMask ObstacleMask { get; }
    }

    /// <summary>Pooled projectile moved by sphere casts, so it cannot tunnel through thin walls.</summary>
    public sealed class Projectile : MonoBehaviour
    {
        [SerializeField] private float radius = 0.18f;

        private readonly Collider[] overlap = new Collider[6];
        private ProjectileSystem owner;
        private ProjectileShot shot;
        private float travelled;

        internal Projectile SourcePrefab { get; private set; }

        internal void Launch(ProjectileSystem system, Projectile prefab, in ProjectileShot newShot)
        {
            owner = system;
            SourcePrefab = prefab;
            shot = newShot;
            travelled = 0f;
            transform.SetPositionAndRotation(shot.Origin, Quaternion.LookRotation(shot.Direction, Vector3.up));
        }

        private void Update()
        {
            if (TryHitOverlappingTarget()) return;

            var step = shot.Speed * Time.deltaTime;
            var mask = shot.TargetMask | shot.ObstacleMask;
            if (Physics.SphereCast(transform.position, radius, shot.Direction, out var hit, step, mask, QueryTriggerInteraction.Collide))
            {
                var isTarget = (shot.TargetMask.value & (1 << hit.collider.gameObject.layer)) != 0;
                if (!isTarget)
                {
                    owner.Release(this);
                    return;
                }

                if (TryDamage(hit.collider)) return;
            }

            transform.position += shot.Direction * step;
            travelled += step;
            if (travelled >= shot.Range) owner.Release(this);
        }

        private bool TryHitOverlappingTarget()
        {
            var count = Physics.OverlapSphereNonAlloc(transform.position, radius, overlap, shot.TargetMask, QueryTriggerInteraction.Collide);
            for (var i = 0; i < count; i++)
            {
                if (TryDamage(overlap[i])) return true;
            }

            return false;
        }

        // A target that is dodging takes no damage and the projectile keeps flying.
        private bool TryDamage(Collider target)
        {
            var damageable = target.GetComponentInParent<IDamageable>();
            if (damageable == null || !damageable.ApplyDamage(new DamageInfo(shot.Damage, shot.Direction, shot.Knockback))) return false;
            owner.Release(this);
            return true;
        }
    }
}
