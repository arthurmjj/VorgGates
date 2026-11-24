using UnityEngine;
using VorgGates.Gameplay;

namespace VorgGates.Presentation
{
    /// <summary>Adds lean, cape motion and a dodge roll to the model. It only reads gameplay state.</summary>
    public sealed class HeroPose : MonoBehaviour
    {
        private const float RollDuration = 0.25f;
        private const float RollPivotHeight = 0.95f;

        [SerializeField] private Transform visual;
        [SerializeField] private Transform body;
        [SerializeField] private Transform cape;
        [SerializeField] private WeaponUser weaponUser;
        [SerializeField] private PlayerDodge dodge;

        private Vector3 lastPosition;
        private float rollTime = -1f;
        private float lean;
        private float twist;
        private float capeLift;

        private void Awake() => lastPosition = transform.position;
        private void OnEnable() => dodge.Started += OnDodgeStarted;
        private void OnDisable() => dodge.Started -= OnDodgeStarted;

        private void OnDodgeStarted() => rollTime = 0f;

        private void LateUpdate()
        {
            var deltaTime = Time.deltaTime;
            if (deltaTime <= 0f) return;

            var delta = transform.position - lastPosition;
            delta.y = 0f;
            lastPosition = transform.position;
            var speed = Mathf.Min(delta.magnitude / deltaTime, 8f);
            var forwardness = delta.sqrMagnitude > 1e-6f ? Vector3.Dot(delta.normalized, transform.forward) : 0f;
            var targetLean = speed / 6f * 7f * forwardness;
            var targetTwist = 0f;
            switch (weaponUser.Phase)
            {
                case AttackPhase.Windup:
                    targetTwist = -22f;
                    targetLean -= 4f;
                    break;
                case AttackPhase.Active:
                    targetTwist = 26f;
                    targetLean += 10f;
                    break;
                case AttackPhase.Recovery:
                    targetTwist = 10f;
                    targetLean += 3f;
                    break;
            }

            var blend = 1f - Mathf.Exp(-20f * deltaTime);
            lean = Mathf.Lerp(lean, targetLean, blend);
            twist = Mathf.Lerp(twist, targetTwist, blend);
            body.localRotation = Quaternion.Euler(lean, twist, 0f);

            capeLift = Mathf.Lerp(capeLift, Mathf.Clamp01(speed / 6f) * 30f, 1f - Mathf.Exp(-7f * deltaTime));
            var flutter = Mathf.Sin(Time.time * (3f + speed)) * (2.5f + speed * 0.7f);
            cape.localRotation = Quaternion.Euler(capeLift + flutter, 0f, Mathf.Sin(Time.time * 2.1f) * 2.5f);

            ApplyRoll(deltaTime);
        }

        private void ApplyRoll(float deltaTime)
        {
            if (rollTime < 0f) return;
            rollTime += deltaTime;
            var progress = Mathf.Clamp01(rollTime / RollDuration);
            if (progress >= 1f)
            {
                rollTime = -1f;
                visual.localRotation = Quaternion.identity;
                visual.localPosition = Vector3.zero;
                return;
            }

            var eased = progress * progress * (3f - 2f * progress);
            var rotation = Quaternion.Euler(eased * 360f, 0f, 0f);
            var pivot = Vector3.up * RollPivotHeight;
            visual.localRotation = rotation;
            visual.localPosition = pivot - rotation * pivot;
        }
    }
}
