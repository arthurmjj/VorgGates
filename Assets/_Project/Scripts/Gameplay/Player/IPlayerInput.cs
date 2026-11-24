using UnityEngine;

namespace VorgGates.Gameplay
{
    /// <summary>Player commands, independent of the device that produced them.</summary>
    public interface IPlayerInput
    {
        Vector2 Move { get; }
        Vector3 AimPoint { get; }
        bool AttackHeld { get; }
        bool ConsumeDodge();
        bool ConsumeInteract();
        bool ConsumePause();

        /// <summary>Returns the requested weapon slot (0 to 2), or -1 when none was pressed.</summary>
        int ConsumeWeaponSlot();

        /// <summary>Returns -1 or 1 when the weapon wheel moved, otherwise 0.</summary>
        int ConsumeWeaponCycle();
    }
}
