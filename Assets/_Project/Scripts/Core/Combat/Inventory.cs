using System;

namespace VorgGates.Core
{
    public enum WeaponKind
    {
        Sword,
        Axe,
        Staff
    }

    public readonly struct InventorySnapshot
    {
        internal InventorySnapshot(int ownedMask, WeaponKind equipped)
        {
            OwnedMask = ownedMask;
            Equipped = equipped;
        }

        internal int OwnedMask { get; }
        internal WeaponKind Equipped { get; }
    }

    /// <summary>Three weapon slots. Snapshots are values, so a failed attempt can never alter a checkpoint.</summary>
    public sealed class InventoryState
    {
        public const int SlotCount = 3;

        private int ownedMask = 1 << (int)WeaponKind.Sword;

        public WeaponKind Equipped { get; private set; } = WeaponKind.Sword;

        public bool Owns(WeaponKind kind) => (ownedMask & (1 << (int)kind)) != 0;

        public bool Add(WeaponKind kind)
        {
            if (Owns(kind)) return false;
            ownedMask |= 1 << (int)kind;
            Equipped = kind;
            return true;
        }

        public bool TryEquip(WeaponKind kind)
        {
            if (!Owns(kind) || Equipped == kind) return false;
            Equipped = kind;
            return true;
        }

        public bool Cycle(int direction)
        {
            if (direction == 0) return false;
            var step = Math.Sign(direction);
            var index = (int)Equipped;
            for (var i = 1; i < SlotCount; i++)
            {
                index = (index + step + SlotCount) % SlotCount;
                if (TryEquip((WeaponKind)index)) return true;
            }

            return false;
        }

        public bool TryGetNextMissing(out WeaponKind kind)
        {
            for (var i = 0; i < SlotCount; i++)
            {
                kind = (WeaponKind)i;
                if (!Owns(kind)) return true;
            }

            kind = WeaponKind.Sword;
            return false;
        }

        public void Reset()
        {
            ownedMask = 1 << (int)WeaponKind.Sword;
            Equipped = WeaponKind.Sword;
        }

        public InventorySnapshot Capture() => new InventorySnapshot(ownedMask, Equipped);

        public void Restore(InventorySnapshot snapshot)
        {
            ownedMask = snapshot.OwnedMask;
            Equipped = snapshot.Equipped;
        }
    }
}
