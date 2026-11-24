using UnityEngine;

namespace VorgGates.Presentation
{
    public sealed class FlameFlicker : MonoBehaviour
    {
        [SerializeField] private Light flameLight;
        [SerializeField] private Transform flame;
        [SerializeField] private float amount = 0.18f;
        [SerializeField] private float speed = 7f;

        private float baseIntensity;
        private Vector3 baseScale;
        private float seed;

        private void Awake()
        {
            baseIntensity = flameLight.intensity;
            baseScale = flame.localScale;
            seed = transform.position.x * 13.7f + transform.position.z * 7.3f;
        }

        private void Update()
        {
            var noise = Mathf.PerlinNoise(seed, Time.time * speed) * 2f - 1f;
            flameLight.intensity = baseIntensity * (1f + noise * amount);
            flame.localScale = new Vector3(baseScale.x * (1f - noise * 0.08f), baseScale.y * (1f + noise * 0.2f), baseScale.z * (1f - noise * 0.08f));
        }
    }
}
