using System;
using UnityEngine;
using VorgGates.Core;

namespace VorgGates.Gameplay
{
    public sealed class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private float invulnerabilityAfterHit = 0.5f;

        private HealthModel model;
        private float invulnerableUntil;
        private bool locked;

        private HealthModel Model => model ??= CreateModel();

        public int Current => Model.Current;
        public int Max => Model.Max;
        public float Normalized => Model.Normalized;
        public bool IsDead => Model.IsDead;
        public bool IsInvulnerable => locked || Time.time < invulnerableUntil;

        public event Action<DamageInfo> Damaged;
        public event Action Died;
        public event Action Restored;
        public event Action Healed;

        public void Configure(int max)
        {
            maxHealth = max;
            model = CreateModel();
        }

        public bool ApplyDamage(in DamageInfo damage)
        {
            if (IsDead || IsInvulnerable || !Model.TakeDamage(damage.Amount)) return false;
            invulnerableUntil = Time.time + invulnerabilityAfterHit;
            Damaged?.Invoke(damage);
            if (Model.IsDead) Died?.Invoke();
            return true;
        }

        public void GrantInvulnerability(float seconds) =>
            invulnerableUntil = Mathf.Max(invulnerableUntil, Time.time + seconds);

        public void SetLocked(bool isLocked) => locked = isLocked;

        public bool Heal(int amount)
        {
            if (!Model.Heal(amount)) return false;
            Healed?.Invoke();
            return true;
        }

        public void Restore()
        {
            Model.Restore();
            invulnerableUntil = 0f;
            locked = false;
            Restored?.Invoke();
        }

        private HealthModel CreateModel() => new HealthModel(maxHealth);
    }
}
