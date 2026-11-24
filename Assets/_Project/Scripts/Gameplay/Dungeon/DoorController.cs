using UnityEngine;
using UnityEngine.AI;

namespace VorgGates.Gameplay
{
    public sealed class DoorController : MonoBehaviour
    {
        [SerializeField] private Transform visual;
        [SerializeField] private Collider blocker;
        [SerializeField] private NavMeshObstacle navObstacle;
        [SerializeField] private float travel = 3.2f;
        [SerializeField] private float moveTime = 0.22f;

        private float openAmount = 1f;

        public bool IsClosed { get; private set; }

        private void Awake() => SetClosed(false, true);

        public void SetClosed(bool closed, bool instant = false)
        {
            IsClosed = closed;
            blocker.enabled = closed;
            navObstacle.enabled = closed;
            if (instant) openAmount = closed ? 0f : 1f;
            ApplyVisual();
        }

        private void Update()
        {
            var target = IsClosed ? 0f : 1f;
            if (Mathf.Approximately(openAmount, target)) return;
            openAmount = Mathf.MoveTowards(openAmount, target, Time.deltaTime / moveTime);
            ApplyVisual();
        }

        private void ApplyVisual() => visual.localPosition = Vector3.down * (travel * openAmount);
    }
}
