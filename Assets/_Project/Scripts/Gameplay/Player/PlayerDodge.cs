using UnityEngine;

namespace VorgGates.Gameplay
{
    [RequireComponent(typeof(PlayerMotor), typeof(Health))]
    public sealed class PlayerDodge : MonoBehaviour
    {
        [SerializeField] private float distance = 5f;
        [SerializeField] private float duration = 0.25f;
        [SerializeField] private float invulnerability = 0.25f;
        [SerializeField] private float cooldown = 0.8f;

        private PlayerMotor motor;
        private Health health;
        private Vector3 direction;
        private float remaining;
        private float readyAt;

        public event System.Action Started;

        public bool IsDodging { get; private set; }
        public float Distance => distance;
        public float Cooldown => cooldown;
        public float CooldownProgress => cooldown <= 0f ? 1f : 1f - Mathf.Clamp01((readyAt - Time.time) / (cooldown + duration));

        private void Awake()
        {
            motor = GetComponent<PlayerMotor>();
            health = GetComponent<Health>();
        }

        public bool TryStart(Vector3 worldDirection)
        {
            if (IsDodging || Time.time < readyAt || worldDirection.sqrMagnitude < 0.0001f) return false;
            direction = worldDirection.normalized;
            remaining = distance;
            IsDodging = true;
            readyAt = Time.time + duration + cooldown;
            health.GrantInvulnerability(invulnerability);
            Started?.Invoke();
            return true;
        }

        public void Tick(float deltaTime)
        {
            if (!IsDodging) return;
            var step = Mathf.Min(distance / duration * deltaTime, remaining);
            motor.MoveBy(direction * step, deltaTime);
            remaining -= step;
            if (remaining <= 0.0001f) IsDodging = false;
        }

        public void ResetState()
        {
            IsDodging = false;
            remaining = 0f;
            readyAt = 0f;
        }
    }
}
