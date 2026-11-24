using UnityEngine;
using UnityEngine.Rendering;

namespace VorgGates.Presentation
{
    [CreateAssetMenu(menuName = "Vorg'Gates/Atmosphere", fileName = "Atmosphere")]
    public sealed class DungeonAtmosphere : ScriptableObject
    {
        [SerializeField] private Color ambientSky;
        [SerializeField] private Color ambientEquator;
        [SerializeField] private Color ambientGround;
        [SerializeField] private Color lightColor;
        [SerializeField] private float lightIntensity = 1f;
        [SerializeField] private Vector3 lightEuler = new Vector3(58f, 118f, 0f);
        [SerializeField] private Color background;
        [SerializeField] private VolumeProfile volume;
        [SerializeField] private GameObject particles;

        public Color AmbientSky => ambientSky;
        public Color AmbientEquator => ambientEquator;
        public Color AmbientGround => ambientGround;
        public Color LightColor => lightColor;
        public float LightIntensity => lightIntensity;
        public Vector3 LightEuler => lightEuler;
        public Color Background => background;
        public VolumeProfile Volume => volume;
        public GameObject Particles => particles;

#if UNITY_EDITOR
        public void EditorConfigure(Color sky, Color equator, Color ground, Color light, float intensity, Vector3 euler, Color backgroundColor, VolumeProfile profile, GameObject particlePrefab)
        {
            ambientSky = sky;
            ambientEquator = equator;
            ambientGround = ground;
            lightColor = light;
            lightIntensity = intensity;
            lightEuler = euler;
            background = backgroundColor;
            volume = profile;
            particles = particlePrefab;
        }
#endif
    }
}
