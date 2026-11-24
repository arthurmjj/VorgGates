using UnityEngine;

namespace VorgGates.Gameplay
{
    public sealed class Interactor : MonoBehaviour
    {
        [SerializeField] private float range = 2.6f;
        [SerializeField] private LayerMask interactableMask;

        private readonly Collider[] buffer = new Collider[8];

        public IInteractable Current { get; private set; }

        private void Update()
        {
            Current = null;
            var bestDistance = float.MaxValue;
            var count = Physics.OverlapSphereNonAlloc(transform.position, range, buffer, interactableMask, QueryTriggerInteraction.Collide);
            for (var i = 0; i < count; i++)
            {
                var candidate = buffer[i].GetComponentInParent<IInteractable>();
                if (candidate == null || !candidate.CanInteract) continue;
                var distance = (buffer[i].transform.position - transform.position).sqrMagnitude;
                if (distance >= bestDistance) continue;
                bestDistance = distance;
                Current = candidate;
            }
        }

        public bool TryInteract()
        {
            if (Current == null || !Current.CanInteract) return false;
            Current.Interact();
            return true;
        }
    }
}
