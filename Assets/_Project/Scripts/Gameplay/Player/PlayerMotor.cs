using UnityEngine;

namespace VorgGates.Gameplay
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        private const float Gravity = -20f;
        private const float GroundedVelocity = -2f;

        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float cameraYaw = 45f;

        private CharacterController controller;
        private float verticalVelocity;

        public float MoveSpeed => moveSpeed;
        public Vector3 Velocity { get; private set; }

        private void Awake() => controller = GetComponent<CharacterController>();

        public Vector3 ToWorldDirection(Vector2 input) => IsoMovement.ToWorld(input, cameraYaw);

        public void MoveBy(Vector3 horizontalDelta, float deltaTime)
        {
            verticalVelocity = controller.isGrounded ? GroundedVelocity : verticalVelocity + Gravity * deltaTime;
            var before = transform.position;
            controller.Move(horizontalDelta + Vector3.up * (verticalVelocity * deltaTime));
            Velocity = deltaTime > 0f ? (transform.position - before) / deltaTime : Vector3.zero;
        }

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            controller.enabled = false;
            transform.SetPositionAndRotation(position, rotation);
            controller.enabled = true;
            verticalVelocity = GroundedVelocity;
            Velocity = Vector3.zero;
            Physics.SyncTransforms();
        }
    }
}
