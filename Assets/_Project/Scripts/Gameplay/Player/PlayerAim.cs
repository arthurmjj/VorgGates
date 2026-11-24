using UnityEngine;

namespace VorgGates.Gameplay
{
    public sealed class PlayerAim : MonoBehaviour
    {
        public void FaceTowards(Vector3 worldPoint)
        {
            var direction = worldPoint - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.01f) return;
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }
    }
}
