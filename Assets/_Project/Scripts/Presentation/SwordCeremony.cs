using UnityEngine;
using VorgGates.Gameplay;

namespace VorgGates.Presentation
{
    /// <summary>The visual climax of taking the sword. It only listens to the interaction.</summary>
    public sealed class SwordCeremony : MonoBehaviour
    {
        private const float ClimaxSeconds = 1.6f;

        [SerializeField] private LegendarySword sword;
        [SerializeField] private Light swordLight;
        [SerializeField] private Transform beam;
        [SerializeField] private Transform runeRing;
        [SerializeField] private ParticleSystem burst;

        private Vector3 beamScale;
        private float baseIntensity;
        private float takenTime = -1f;

        private void Awake()
        {
            beamScale = beam.localScale;
            baseIntensity = swordLight.intensity;
            sword.Taken += OnTaken;
        }

        private void OnDestroy() => sword.Taken -= OnTaken;

        private void OnTaken()
        {
            takenTime = 0f;
            burst.Play(true);
        }

        private void Update()
        {
            var time = Time.unscaledTime;
            runeRing.Rotate(Vector3.up, (takenTime >= 0f ? 90f : 14f) * Time.unscaledDeltaTime, Space.Self);
            var breathing = 1f + Mathf.Sin(time * 1.8f) * 0.05f;
            if (takenTime < 0f)
            {
                beam.localScale = new Vector3(beamScale.x * breathing, beamScale.y, beamScale.z * breathing);
                swordLight.intensity = baseIntensity * breathing;
                return;
            }

            takenTime += Time.unscaledDeltaTime;
            var progress = Mathf.Clamp01(takenTime / ClimaxSeconds);
            var flare = Mathf.Sin(progress * Mathf.PI * 0.5f);
            beam.localScale = new Vector3(beamScale.x * (1f + flare * 1.15f) * breathing, beamScale.y, beamScale.z * (1f + flare * 1.15f) * breathing);
            swordLight.intensity = baseIntensity * (1f + flare * 1.6f);
        }
    }
}
