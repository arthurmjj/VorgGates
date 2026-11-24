using UnityEngine;

namespace VorgGates.Gameplay
{
    public static class IsoMovement
    {
        /// <summary>Converts screen-relative input to a world direction for a camera with this yaw.</summary>
        public static Vector3 ToWorld(Vector2 input, float yawDegrees)
        {
            if (input.sqrMagnitude > 1f) input.Normalize();
            var radians = yawDegrees * Mathf.Deg2Rad;
            var sin = Mathf.Sin(radians);
            var cos = Mathf.Cos(radians);
            return new Vector3(input.x * cos + input.y * sin, 0f, -input.x * sin + input.y * cos);
        }
    }
}
