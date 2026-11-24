using System;
using UnityEngine;
using VorgGates.Core;

namespace VorgGates.Gameplay
{
    [RequireComponent(typeof(WeaponUser))]
    public sealed class PlayerInventory : MonoBehaviour
    {
        [SerializeField] private WeaponDefinition[] weapons = new WeaponDefinition[InventoryState.SlotCount];

        private readonly InventoryState state = new InventoryState();
        private WeaponUser user;

        public InventoryState State => state;
        public WeaponKind Equipped => state.Equipped;

        public event Action Changed;
        public event Action WeaponGained;

        private void Awake()
        {
            user = GetComponent<WeaponUser>();
            user.Equip(Definition(state.Equipped));
        }

        public WeaponDefinition Definition(WeaponKind kind) => weapons[(int)kind];
        public bool Owns(WeaponKind kind) => state.Owns(kind);
        public bool TryGetNextMissing(out WeaponKind kind) => state.TryGetNextMissing(out kind);

        public bool Add(WeaponKind kind)
        {
            if (!state.Add(kind)) return false;
            ApplyEquipped();
            WeaponGained?.Invoke();
            return true;
        }

        public bool TrySelect(WeaponKind kind)
        {
            if (!state.TryEquip(kind)) return false;
            ApplyEquipped();
            return true;
        }

        public bool Cycle(int direction)
        {
            if (!state.Cycle(direction)) return false;
            ApplyEquipped();
            return true;
        }

        /// <summary>Applies state changed from outside, such as a checkpoint restore.</summary>
        public void Refresh() => ApplyEquipped();

        private void ApplyEquipped()
        {
            user.Equip(Definition(state.Equipped));
            Changed?.Invoke();
        }
    }
}
