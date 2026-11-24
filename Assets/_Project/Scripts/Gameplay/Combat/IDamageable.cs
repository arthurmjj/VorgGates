using UnityEngine;

namespace VorgGates.Gameplay
{
    public readonly struct DamageInfo
    {
        public DamageInfo(int amount, Vector3 direction, float knockback)
        {
            Amount = amount;
            Direction = direction;
            Knockback = knockback;
        }

        public int Amount { get; }
        public Vector3 Direction { get; }
        public float Knockback { get; }
    }

    public interface IDamageable
    {
        bool IsDead { get; }
        bool ApplyDamage(in DamageInfo damage);
    }
}
