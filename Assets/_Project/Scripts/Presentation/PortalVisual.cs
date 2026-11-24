using UnityEngine;

namespace VorgGates.Presentation
{
    public sealed class PortalVisual : MonoBehaviour
    {
        [SerializeField] private Transform swirl;
        [SerializeField] private float degreesPerSecond = 70f;

        private Vector3 baseScale;

        private void Awake() => baseScale = swirl.localScale;

        private void Update()
        {
            swirl.Rotate(Vector3.up, degreesPerSecond * Time.deltaTime, Space.Self);
            var pulse = 1f + Mathf.Sin(Time.time * 3f) * 0.04f;
            swirl.localScale = new Vector3(baseScale.x * pulse, baseScale.y, baseScale.z * pulse);
        }
    }
}
