using UnityEngine;
using VorgGates.Gameplay;

namespace VorgGates.Presentation
{
    public sealed class HitFlash : MonoBehaviour
    {
        [SerializeField] private Health health;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Material flashMaterial;
        [SerializeField] private float duration = 0.09f;

        private Renderer[] renderers;
        private Material[][] originals;
        private Material[][] flashSets;
        private Material[][] overrideSets;
        private Material overrideMaterial;
        private float timer;

        private void Awake()
        {
            renderers = visualRoot.GetComponentsInChildren<Renderer>(true);
            originals = new Material[renderers.Length][];
            flashSets = new Material[renderers.Length][];
            overrideSets = new Material[renderers.Length][];
            for (var i = 0; i < renderers.Length; i++)
            {
                originals[i] = renderers[i].sharedMaterials;
                flashSets[i] = Filled(originals[i].Length, flashMaterial);
                overrideSets[i] = new Material[originals[i].Length];
            }

            health.Damaged += OnDamaged;
            health.Restored += RestoreMaterials;
        }

        private void OnDestroy()
        {
            health.Damaged -= OnDamaged;
            health.Restored -= RestoreMaterials;
        }

        /// <summary>Replaces the resting look, for example with a translucent version.</summary>
        public void SetOverride(Material material)
        {
            overrideMaterial = material;
            for (var i = 0; i < overrideSets.Length; i++)
            {
                for (var slot = 0; slot < overrideSets[i].Length; slot++) overrideSets[i][slot] = material;
            }

            if (timer <= 0f) RestoreMaterials();
        }

        private void OnDamaged(DamageInfo damage)
        {
            timer = duration;
            for (var i = 0; i < renderers.Length; i++) renderers[i].sharedMaterials = flashSets[i];
        }

        private void Update()
        {
            if (timer <= 0f) return;
            timer -= Time.deltaTime;
            if (timer <= 0f) RestoreMaterials();
        }

        private void RestoreMaterials()
        {
            timer = 0f;
            var sets = overrideMaterial != null ? overrideSets : originals;
            for (var i = 0; i < renderers.Length; i++) renderers[i].sharedMaterials = sets[i];
        }

        private static Material[] Filled(int length, Material material)
        {
            var result = new Material[length];
            for (var i = 0; i < length; i++) result[i] = material;
            return result;
        }
    }
}
