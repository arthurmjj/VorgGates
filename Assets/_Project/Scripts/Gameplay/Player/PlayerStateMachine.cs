using UnityEngine;
using VorgGates.Core;

namespace VorgGates.Gameplay
{
    /// <summary>Decides which player action may run each frame.</summary>
    [RequireComponent(typeof(PlayerMotor), typeof(PlayerAim), typeof(PlayerDodge))]
    public sealed class PlayerStateMachine : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float attackMoveMultiplier = 0.45f;

        private PlayerMotor motor;
        private PlayerAim aim;
        private PlayerDodge dodge;
        private WeaponUser weapon;
        private PlayerInventory inventory;
        private Interactor interactor;
        private Health health;
        private bool controlsEnabled;
        private bool suppressAttack;

        public IPlayerInput Input { get; private set; }
        public PlayerMotor Motor => motor;
        public PlayerDodge Dodge => dodge;
        public WeaponUser Weapon => weapon;
        public PlayerInventory Inventory => inventory;
        public Interactor Interactor => interactor;
        public Health Health => health;

        private void Awake()
        {
            motor = GetComponent<PlayerMotor>();
            aim = GetComponent<PlayerAim>();
            dodge = GetComponent<PlayerDodge>();
            weapon = GetComponent<WeaponUser>();
            inventory = GetComponent<PlayerInventory>();
            interactor = GetComponent<Interactor>();
            health = GetComponent<Health>();
            Input = GetComponent<IPlayerInput>();
        }

        public void SetInput(IPlayerInput input) => Input = input;

        public void SetControlsEnabled(bool isEnabled)
        {
            controlsEnabled = isEnabled;
            if (!isEnabled) weapon.CancelAttack();

            // A click that confirmed a menu must not also swing the weapon.
            else suppressAttack = Input.AttackHeld;
        }

        public void ResetTo(Transform spawn)
        {
            dodge.ResetState();
            weapon.CancelAttack();
            motor.Teleport(spawn.position, spawn.rotation);
            health.Restore();
        }

        private void Update()
        {
            var deltaTime = Time.deltaTime;
            if (!controlsEnabled)
            {
                motor.MoveBy(Vector3.zero, deltaTime);
                return;
            }

            aim.FaceTowards(Input.AimPoint);
            HandleWeaponSelection();
            var moveDirection = motor.ToWorldDirection(Input.Move);

            if (!dodge.IsDodging && Input.ConsumeDodge())
            {
                var dodgeDirection = moveDirection.sqrMagnitude > 0.01f ? moveDirection : transform.forward;
                if (dodge.TryStart(dodgeDirection)) weapon.CancelAttack();
            }

            if (dodge.IsDodging)
            {
                dodge.Tick(deltaTime);
                return;
            }

            if (!Input.AttackHeld) suppressAttack = false;
            if (Input.AttackHeld && !suppressAttack) weapon.TryAttack();
            weapon.Tick(deltaTime);

            var speed = motor.MoveSpeed * (weapon.IsAttacking ? attackMoveMultiplier : 1f);
            motor.MoveBy(moveDirection * (speed * deltaTime), deltaTime);

            if (Input.ConsumeInteract()) interactor.TryInteract();
        }

        private void HandleWeaponSelection()
        {
            var slot = Input.ConsumeWeaponSlot();
            var cycle = Input.ConsumeWeaponCycle();
            if (!weapon.CanSwitchWeapon) return;
            if (slot >= 0) inventory.TrySelect((WeaponKind)slot);
            else if (cycle != 0) inventory.Cycle(cycle);
        }
    }
}
