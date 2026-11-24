using UnityEngine;
using UnityEngine.Rendering;

namespace VorgGates.Presentation
{
    /// <summary>Applies the light, ambient, post-processing and particles of the current phase.</summary>
    public sealed class ThemeDirector : MonoBehaviour
    {
        [SerializeField] private DungeonAtmosphere[] atmospheres;
        [SerializeField] private Light mainLight;
        [SerializeField] private Camera gameCamera;
        [SerializeField] private Volume volume;
        [SerializeField] private Transform follow;

        private GameObject[] particles;
        private Transform activeParticles;

        public int AppliedIndex { get; private set; } = -1;

        private void Awake()
        {
            particles = new GameObject[atmospheres.Length];
            for (var i = 0; i < particles.Length; i++)
            {
                if (atmospheres[i].Particles == null) continue;
                particles[i] = Instantiate(atmospheres[i].Particles, transform);
                particles[i].SetActive(false);
            }
        }

        public void Apply(int index)
        {
            var atmosphere = atmospheres[index];
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = atmosphere.AmbientSky;
            RenderSettings.ambientEquatorColor = atmosphere.AmbientEquator;
            RenderSettings.ambientGroundColor = atmosphere.AmbientGround;
            mainLight.color = atmosphere.LightColor;
            mainLight.intensity = atmosphere.LightIntensity;
            mainLight.transform.rotation = Quaternion.Euler(atmosphere.LightEuler);
            gameCamera.backgroundColor = atmosphere.Background;
            volume.sharedProfile = atmosphere.Volume;

            for (var i = 0; i < particles.Length; i++)
            {
                if (particles[i] != null) particles[i].SetActive(i == index);
            }

            activeParticles = particles[index] != null ? particles[index].transform : null;
            AppliedIndex = index;
            LateUpdate();
        }

        private void LateUpdate()
        {
            if (activeParticles != null) activeParticles.position = follow.position + Vector3.up * 2f;
        }
    }
}
