using UnityEngine;

namespace VorgGates.Gameplay
{
    public sealed class HealOrb : MonoBehaviour
    {
        private const float PickupDistance = 1.35f;

        [SerializeField] private int healing = 15;

        private Transform player;
        private Health playerHealth;
        private Vector3 restPosition;

        public void Configure(Transform playerTransform, Health health)
        {
            player = playerTransform;
            playerHealth = health;
            restPosition = transform.position;
        }

        private void Update()
        {
            transform.position = restPosition + Vector3.up * (Mathf.Sin(Time.time * 3f) * 0.12f);
            var delta = player.position - restPosition;
            delta.y = 0f;
            if (delta.magnitude <= PickupDistance && playerHealth.Heal(healing)) Destroy(gameObject);
        }
    }
}
