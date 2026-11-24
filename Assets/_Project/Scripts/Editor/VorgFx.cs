using UnityEngine;
using UnityEngine.Rendering;

namespace VorgGates.EditorTools
{
    /// <summary>Light particle effects and soft ground decals. None of them affect gameplay.</summary>
    internal static class VorgFx
    {
        public static ParticleSystem Sparkles(Transform parent, string name, Material material, Vector3 position, float radius, float rate, float size, float rise, int maxParticles = 24)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localRotation = Quaternion.identity;
            var system = go.AddComponent<ParticleSystem>();
            var main = system.main;
            main.loop = true;
            main.playOnAwake = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = maxParticles;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.1f, 2.2f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(size * 0.5f, size);
            main.startColor = Color.white;

            var emission = system.emission;
            emission.rateOverTime = rate;

            var shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = radius;
            shape.rotation = new Vector3(90f, 0f, 0f);

            var velocity = system.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = new ParticleSystem.MinMaxCurve(-0.08f, 0.08f);
            velocity.z = new ParticleSystem.MinMaxCurve(-0.08f, 0.08f);
            velocity.y = new ParticleSystem.MinMaxCurve(rise * 0.5f, rise);

            ApplyFade(system, 0.95f);
            Finish(go, material);
            return system;
        }

        public static ParticleSystem Burst(Transform parent, string name, Material material, Vector3 position, int count, float speed, float size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            var system = go.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = system.main;
            main.loop = false;
            main.playOnAwake = false;
            main.duration = 0.6f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.useUnscaledTime = true;
            main.maxParticles = count;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.9f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.35f, speed);
            main.startSize = new ParticleSystem.MinMaxCurve(size * 0.4f, size);
            main.gravityModifier = -0.05f;

            var emission = system.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, count) });

            var shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.45f;

            ApplyFade(system, 1f);
            Finish(go, material);
            return system;
        }

        public static void ContactShadow(Transform parent, VorgArt art, float diameter, bool glow)
        {
            var shadow = Kit.Part(parent, PrimitiveType.Quad, "ContactShadow", Vector3.up * 0.028f, new Vector3(diameter, diameter, 1f), art.Shadow, new Vector3(90f, 0f, 0f));
            NoShadows(shadow);
            if (!glow) return;
            var halo = Kit.Part(parent, PrimitiveType.Quad, "ReadabilityGlow", Vector3.up * 0.024f, new Vector3(diameter * 1.75f, diameter * 1.75f, 1f), art.EnemyGlow, new Vector3(90f, 0f, 0f));
            NoShadows(halo);
        }

        public static void NoShadows(GameObject go)
        {
            foreach (var renderer in go.GetComponentsInChildren<Renderer>(true))
            {
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
        }

        private static void ApplyFade(ParticleSystem system, float peak)
        {
            var fade = system.colorOverLifetime;
            fade.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(peak, 0.25f), new GradientAlphaKey(0f, 1f) });
            fade.color = new ParticleSystem.MinMaxGradient(gradient);
        }

        private static void Finish(GameObject go, Material material)
        {
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }
    }
}
