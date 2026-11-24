using System;

namespace VorgGates.Core
{
    /// <summary>Engine-free health rules. Death fires exactly once until restored.</summary>
    public sealed class HealthModel
    {
        public HealthModel(int max)
        {
            if (max <= 0) throw new ArgumentOutOfRangeException(nameof(max));
            Max = max;
            Current = max;
        }

        public int Max { get; }
        public int Current { get; private set; }
        public bool IsDead => Current <= 0;
        public float Normalized => (float)Current / Max;

        public event Action Died;

        public bool TakeDamage(int amount)
        {
            if (amount <= 0 || IsDead) return false;
            Current = Math.Max(0, Current - amount);
            if (IsDead) Died?.Invoke();
            return true;
        }

        public bool Heal(int amount)
        {
            if (amount <= 0 || IsDead || Current >= Max) return false;
            Current = Math.Min(Max, Current + amount);
            return true;
        }

        public void Restore() => Current = Max;
    }
}
