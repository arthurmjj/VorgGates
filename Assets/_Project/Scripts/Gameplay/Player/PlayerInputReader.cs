using UnityEngine;
using UnityEngine.InputSystem;

namespace VorgGates.Gameplay
{
    public sealed class PlayerInputReader : MonoBehaviour, IPlayerInput
    {
        [SerializeField] private InputActionAsset actions;
        [SerializeField] private Camera aimCamera;

        private InputActionAsset runtimeActions;
        private InputActionMap map;
        private InputAction move;
        private InputAction aim;
        private InputAction attack;
        private InputAction dodge;
        private InputAction interact;
        private InputAction pause;
        private InputAction weapon1;
        private InputAction weapon2;
        private InputAction weapon3;
        private InputAction weaponCycle;

        public Vector2 Move => move.ReadValue<Vector2>();
        public bool AttackHeld => attack.IsPressed();

        public Vector3 AimPoint
        {
            get
            {
                var ray = aimCamera.ScreenPointToRay(aim.ReadValue<Vector2>());
                var plane = new Plane(Vector3.up, transform.position + Vector3.up);
                return plane.Raycast(ray, out var distance) ? ray.GetPoint(distance) : transform.position + transform.forward;
            }
        }

        public bool ConsumeDodge() => dodge.WasPressedThisFrame();
        public bool ConsumeInteract() => interact.WasPressedThisFrame();
        public bool ConsumePause() => pause.WasPressedThisFrame();

        public int ConsumeWeaponSlot()
        {
            if (weapon1.WasPressedThisFrame()) return 0;
            if (weapon2.WasPressedThisFrame()) return 1;
            return weapon3.WasPressedThisFrame() ? 2 : -1;
        }

        public int ConsumeWeaponCycle()
        {
            if (!weaponCycle.WasPerformedThisFrame()) return 0;
            var value = weaponCycle.ReadValue<float>();
            return value > 0f ? -1 : value < 0f ? 1 : 0;
        }

        private void Awake()
        {
            // A private copy keeps an old reader from disabling shared actions during a scene change.
            runtimeActions = Instantiate(actions);
            map = runtimeActions.FindActionMap("Player", true);
            move = map.FindAction("Move", true);
            aim = map.FindAction("Aim", true);
            attack = map.FindAction("Attack", true);
            dodge = map.FindAction("Dodge", true);
            interact = map.FindAction("Interact", true);
            pause = map.FindAction("Pause", true);
            weapon1 = map.FindAction("Weapon1", true);
            weapon2 = map.FindAction("Weapon2", true);
            weapon3 = map.FindAction("Weapon3", true);
            weaponCycle = map.FindAction("WeaponCycle", true);
        }

        private void OnEnable() => map.Enable();
        private void OnDisable() => map.Disable();
        private void OnDestroy() => Destroy(runtimeActions);
    }
}
