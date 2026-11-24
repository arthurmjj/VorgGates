using System.Collections.Generic;
using UnityEngine;
using VorgGates.Gameplay;

namespace VorgGates.Presentation
{
    /// <summary>Shows the equipped weapon, animates it and draws the real hit arc of melee attacks.</summary>
    public sealed class SwingVisual : MonoBehaviour
    {
        private const int ArcSegments = 18;
        private const float RestAngle = 18f;
        private const float ThrustDistance = 0.55f;

        [SerializeField] private WeaponUser weaponUser;
        [SerializeField] private Transform weaponPivot;
        [SerializeField] private GameObject[] weaponModels;
        [SerializeField] private MeshFilter slashFilter;
        [SerializeField] private MeshRenderer slashRenderer;

        private readonly Dictionary<WeaponDefinition, Mesh> arcs = new Dictionary<WeaponDefinition, Mesh>();
        private Vector3 pivotRest;

        private void Awake()
        {
            pivotRest = weaponPivot.localPosition;
            slashRenderer.enabled = false;
            weaponUser.WeaponChanged += ApplyWeapon;
        }

        private void Start() => ApplyWeapon(weaponUser.Weapon);

        private void OnDestroy()
        {
            weaponUser.WeaponChanged -= ApplyWeapon;
            foreach (var mesh in arcs.Values) Destroy(mesh);
        }

        private void ApplyWeapon(WeaponDefinition weapon)
        {
            for (var i = 0; i < weaponModels.Length; i++) weaponModels[i].SetActive(i == (int)weapon.Kind);
            if (weapon.IsRanged) return;
            if (!arcs.TryGetValue(weapon, out var mesh))
            {
                mesh = BuildArc(weapon.Range, weapon.ArcDegrees);
                arcs.Add(weapon, mesh);
            }

            slashFilter.sharedMesh = mesh;
        }

        private void Update()
        {
            var weapon = weaponUser.Weapon;
            if (weapon.IsRanged) AnimateCast();
            else AnimateSwing(weapon);
            slashRenderer.enabled = !weapon.IsRanged && weaponUser.Phase == AttackPhase.Active;
        }

        private void AnimateSwing(WeaponDefinition weapon)
        {
            var halfArc = weapon.ArcDegrees * 0.5f;
            var progress = weaponUser.PhaseProgress;
            float angle;
            switch (weaponUser.Phase)
            {
                case AttackPhase.Windup: angle = Mathf.Lerp(RestAngle, -halfArc, progress); break;
                case AttackPhase.Active: angle = Mathf.Lerp(-halfArc, halfArc, progress); break;
                case AttackPhase.Recovery: angle = Mathf.Lerp(halfArc, RestAngle, progress * progress); break;
                default: angle = RestAngle; break;
            }

            weaponPivot.localPosition = pivotRest;
            weaponPivot.localRotation = Quaternion.Euler(0f, angle, 0f);
        }

        private void AnimateCast()
        {
            var progress = weaponUser.PhaseProgress;
            float thrust;
            switch (weaponUser.Phase)
            {
                case AttackPhase.Windup: thrust = Mathf.Lerp(0f, -0.25f, progress); break;
                case AttackPhase.Active: thrust = Mathf.Lerp(-0.25f, ThrustDistance, progress); break;
                case AttackPhase.Recovery: thrust = Mathf.Lerp(ThrustDistance, 0f, progress); break;
                default: thrust = 0f; break;
            }

            weaponPivot.localRotation = Quaternion.identity;
            weaponPivot.localPosition = pivotRest + Vector3.forward * thrust;
        }

        private static Mesh BuildArc(float range, float arcDegrees)
        {
            const float innerRatio = 0.35f;
            var vertices = new Vector3[(ArcSegments + 1) * 2];
            var triangles = new int[ArcSegments * 6];
            for (var i = 0; i <= ArcSegments; i++)
            {
                var angle = Mathf.Lerp(-arcDegrees * 0.5f, arcDegrees * 0.5f, i / (float)ArcSegments) * Mathf.Deg2Rad;
                var direction = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle));
                vertices[i * 2] = direction * (range * innerRatio);
                vertices[i * 2 + 1] = direction * range;
                if (i == ArcSegments) continue;
                var t = i * 6;
                triangles[t] = i * 2;
                triangles[t + 1] = i * 2 + 1;
                triangles[t + 2] = i * 2 + 3;
                triangles[t + 3] = i * 2;
                triangles[t + 4] = i * 2 + 3;
                triangles[t + 5] = i * 2 + 2;
            }

            var mesh = new Mesh { name = "SlashArc", vertices = vertices, triangles = triangles };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
