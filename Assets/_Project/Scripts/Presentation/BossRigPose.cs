using UnityEngine;
using VorgGates.Gameplay;

namespace VorgGates.Presentation
{
    /// <summary>Animates a skinned boss from observed state. It reads gameplay and never changes it.</summary>
    public sealed class BossRigPose : MonoBehaviour
    {
        private const float StrideDegrees = 36f;
        private const float KneeDegrees = 26f;
        private const float StepRadiansPerMeter = Mathf.PI;
        private const float StrikeSeconds = 0.2f;
        private const float RaisedArm = -132f;

        [SerializeField] private EnemyBrain brain;
        [SerializeField] private Transform hips;
        [SerializeField] private Transform spine;
        [SerializeField] private Transform head;
        [SerializeField] private Transform upperArmA;
        [SerializeField] private Transform forearmA;
        [SerializeField] private Transform upperArmB;
        [SerializeField] private Transform forearmB;
        [SerializeField] private Transform thighA;
        [SerializeField] private Transform shinA;
        [SerializeField] private Transform thighB;
        [SerializeField] private Transform shinB;
        [SerializeField] private Transform capeUpper;
        [SerializeField] private Transform capeLower;

        private Vector3 lastPosition;
        private Vector3 hipsRest;
        private EnemyState lastState;
        private float stateTime;
        private float phase;
        private float moveBlend;
        private float arm;
        private float fore;
        private float bend;
        private float twist;

        private void Awake()
        {
            lastPosition = transform.position;
            hipsRest = hips.localPosition;
            lastState = brain.State;
        }

        private void LateUpdate()
        {
            var deltaTime = Time.deltaTime;
            if (deltaTime <= 0f) return;

            if (brain.State != lastState)
            {
                lastState = brain.State;
                stateTime = 0f;
            }

            stateTime += deltaTime;
            var delta = transform.position - lastPosition;
            delta.y = 0f;
            lastPosition = transform.position;
            var speed = Mathf.Min(delta.magnitude / deltaTime, 12f);
            var walking = speed > 0.2f && brain.State != EnemyState.Dead;
            moveBlend = Mathf.MoveTowards(moveBlend, walking ? 1f : 0f, deltaTime * 7f);
            phase = Mathf.Repeat(phase + speed * StepRadiansPerMeter * deltaTime, Mathf.PI * 2f);

            ApplyLegs();
            ApplyUpperBody(deltaTime);
        }

        private void ApplyLegs()
        {
            var stride = (1f - Mathf.Cos(phase)) * 0.5f * moveBlend;
            var lift = Mathf.Sin(phase);
            thighA.localRotation = Quaternion.Euler(-StrideDegrees * stride, 0f, 0f);
            thighB.localRotation = Quaternion.Euler(StrideDegrees * stride, 0f, 0f);
            shinA.localRotation = Quaternion.Euler(Mathf.Max(0f, lift) * KneeDegrees * moveBlend, 0f, 0f);
            shinB.localRotation = Quaternion.Euler(Mathf.Max(0f, -lift) * KneeDegrees * moveBlend, 0f, 0f);
            hips.localPosition = hipsRest + Vector3.up * (Mathf.Abs(lift) * 0.14f * moveBlend);
        }

        private void ApplyUpperBody(float deltaTime)
        {
            var targetArm = 0f;
            var targetFore = 0f;
            var targetBend = 0f;
            var targetTwist = 0f;
            var rate = 10f;
            switch (brain.State)
            {
                case EnemyState.Telegraph:
                    var windup = Mathf.Clamp01(stateTime / Mathf.Max(0.05f, brain.CurrentTelegraph));
                    windup = windup * windup * (3f - 2f * windup);
                    targetArm = RaisedArm * windup;
                    targetFore = -24f * windup;
                    targetBend = -11f * windup;
                    targetTwist = 20f * windup;
                    rate = 18f;
                    break;
                case EnemyState.Attack:
                    var strike = Mathf.Clamp01(stateTime / StrikeSeconds);
                    targetArm = Mathf.Lerp(RaisedArm, 24f, strike);
                    targetFore = Mathf.Lerp(-24f, -4f, strike);
                    targetBend = Mathf.Lerp(-11f, 17f, strike);
                    targetTwist = Mathf.Lerp(20f, -24f, strike);
                    rate = 34f;
                    break;
                case EnemyState.Dead:
                    targetArm = 18f;
                    targetBend = 24f;
                    rate = 6f;
                    break;
            }

            var blend = 1f - Mathf.Exp(-rate * deltaTime);
            arm = Mathf.Lerp(arm, targetArm, blend);
            fore = Mathf.Lerp(fore, targetFore, blend);
            bend = Mathf.Lerp(bend, targetBend, blend);
            twist = Mathf.Lerp(twist, targetTwist, blend);

            var breath = Mathf.Sin(Time.time * 2.1f);
            var swing = Mathf.Sin(phase) * moveBlend;
            spine.localRotation = Quaternion.Euler(bend + breath * 1.4f, twist + swing * 6f, 0f);
            head.localRotation = Quaternion.Euler(-bend * 0.45f, -twist * 0.45f, 0f);
            upperArmA.localRotation = Quaternion.Euler(arm + swing * 10f, 0f, breath * 1.2f);
            forearmA.localRotation = Quaternion.Euler(fore, 0f, 0f);
            upperArmB.localRotation = Quaternion.Euler(-swing * 16f + breath * 1.5f, 0f, -breath * 1.2f);
            forearmB.localRotation = Quaternion.Euler(-Mathf.Abs(swing) * 8f, 0f, 0f);

            var flutter = Mathf.Sin(Time.time * (2.6f + moveBlend * 2.4f));
            capeUpper.localRotation = Quaternion.Euler(flutter * (2f + moveBlend * 3f) - moveBlend * 5f, 0f, flutter * 1.4f);
            capeLower.localRotation = Quaternion.Euler(Mathf.Sin(Time.time * 3.3f + 0.8f) * (4f + moveBlend * 5f), 0f, flutter * 2.4f);
        }
    }
}
