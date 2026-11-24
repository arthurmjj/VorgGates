namespace VorgGates.Core
{
    /// <summary>Limits how many enemies may run a ranged attack at the same time.</summary>
    public sealed class AttackTokenPool
    {
        public AttackTokenPool(int capacity) => Capacity = capacity;

        public int Capacity { get; }
        public int InUse { get; private set; }

        public bool TryAcquire()
        {
            if (InUse >= Capacity) return false;
            InUse++;
            return true;
        }

        public void Release()
        {
            if (InUse > 0) InUse--;
        }
    }
}
