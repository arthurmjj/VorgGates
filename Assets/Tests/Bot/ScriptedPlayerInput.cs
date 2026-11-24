using UnityEngine;
using VorgGates.Gameplay;

namespace VorgGates.Tests
{
    public sealed class ScriptedPlayerInput : IPlayerInput
    {
        public bool Dodge;
        public bool Interact;
        public int WeaponSlot = -1;
        public int WeaponCycle;

        public Vector2 Move { get; set; }
        public Vector3 AimPoint { get; set; }
        public bool AttackHeld { get; set; }

        public bool ConsumeDodge()
        {
            var value = Dodge;
            Dodge = false;
            return value;
        }

        public bool ConsumeInteract()
        {
            var value = Interact;
            Interact = false;
            return value;
        }

        public bool Pause;

        public bool ConsumePause()
        {
            var value = Pause;
            Pause = false;
            return value;
        }

        public int ConsumeWeaponSlot()
        {
            var value = WeaponSlot;
            WeaponSlot = -1;
            return value;
        }

        public int ConsumeWeaponCycle()
        {
            var value = WeaponCycle;
            WeaponCycle = 0;
            return value;
        }
    }
}
