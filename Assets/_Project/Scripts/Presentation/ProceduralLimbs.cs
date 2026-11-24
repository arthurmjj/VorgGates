using UnityEngine;

namespace VorgGates.Presentation
{
    /// <summary>Lightweight walk and idle motion for characters built from primitives.</summary>
    public sealed class ProceduralLimbs : MonoBehaviour
    {
        [SerializeField] private Transform body;
        [SerializeField] private Transform[] groupA;
        [SerializeField] private Transform[] groupB;
        [SerializeField] private float stepsPerMeter = 1.5f;
        [SerializeField] private float swingDegrees = 34f;
        [SerializeField] private float bobHeight = 0.05f;

        private Vector3 lastPosition;
        private Vector3 bodyRest;
        private float phase;
        private float moveBlend;

        private void Awake()
        {
            lastPosition = transform.position;
            bodyRest = body.localPosition;
        }

        private void LateUpdate()
        {
            var deltaTime = Time.deltaTime;
            if (deltaTime <= 0f) return;

            var delta = transform.position - lastPosition;
            delta.y = 0f;
            lastPosition = transform.position;
            var speed = Mathf.Min(delta.magnitude / deltaTime, 8f);
            moveBlend = Mathf.MoveTowards(moveBlend, speed > 0.2f ? 1f : 0f, deltaTime * 8f);
            phase += speed * stepsPerMeter * Mathf.PI * deltaTime;

            var swing = Mathf.Sin(phase) * swingDegrees * moveBlend;
            foreach (var limb in groupA) limb.localRotation = Quaternion.Euler(swing, 0f, 0f);
            foreach (var limb in groupB) limb.localRotation = Quaternion.Euler(-swing, 0f, 0f);

            var idle = Mathf.Sin(Time.time * 2.4f) * bobHeight * 0.4f * (1f - moveBlend);
            var walk = Mathf.Abs(Mathf.Sin(phase)) * bobHeight * moveBlend;
            body.localPosition = bodyRest + Vector3.up * (idle + walk);
        }
    }
}
