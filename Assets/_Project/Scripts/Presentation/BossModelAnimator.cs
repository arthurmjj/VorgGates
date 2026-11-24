using UnityEngine;
using VorgGates.Gameplay;

namespace VorgGates.Presentation
{
    /// <summary>Plays a rigged boss model from observed movement. It only reads gameplay state.</summary>
    public sealed class BossModelAnimator : MonoBehaviour
    {
        private const float WalkThreshold = 0.2f;
        private const float ReferenceSpeed = 2.4f;

        private static readonly int Moving = Animator.StringToHash("Moving");

        [SerializeField] private Animator animator;
        [SerializeField] private EnemyBrain brain;

        private Vector3 lastPosition;
        private float speed;

        private void Awake() => lastPosition = transform.position;

        private void LateUpdate()
        {
            var deltaTime = Time.deltaTime;
            if (deltaTime <= 0f) return;

            var delta = transform.position - lastPosition;
            delta.y = 0f;
            lastPosition = transform.position;
            var measured = Mathf.Min(delta.magnitude / deltaTime, 12f);
            speed = Mathf.Lerp(speed, measured, 1f - Mathf.Exp(-12f * deltaTime));

            var alive = brain.State != EnemyState.Dead;
            var moving = alive && speed > WalkThreshold;
            animator.SetBool(Moving, moving);
            animator.speed = !alive ? 0f : moving ? Mathf.Clamp(speed / ReferenceSpeed, 0.7f, 2.2f) : 1f;
        }
    }
}
