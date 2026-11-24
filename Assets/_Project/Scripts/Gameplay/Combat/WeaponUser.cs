using System;
using System.Collections.Generic;
using UnityEngine;

namespace VorgGates.Gameplay
{
    public enum AttackPhase
    {
        None,
        Windup,
        Active,
        Recovery
    }

    /// <summary>Runs attack timing and resolves melee queries or projectile shots.</summary>
    public sealed class WeaponUser : MonoBehaviour
    {
        private const float OriginHeight = 1f;
        private const float SwingHeight = 1.3f;
        private const float MuzzleHeight = 1.15f;
        private const float MuzzleDistance = 0.8f;
        private const float ComboWindow = 0.75f;

        [SerializeField] private WeaponDefinition weapon;
        [SerializeField] private LayerMask targetMask;
        [SerializeField] private LayerMask obstacleMask;

        private readonly Collider[] overlapBuffer = new Collider[24];
        private readonly HashSet<IDamageable> hitThisSwing = new HashSet<IDamageable>();
        private ProjectileSystem projectiles;
        private float phaseTime;
        private float lastSwingTime = float.NegativeInfinity;
        private int comboCount;
        private int swingDamage;
        private bool firedThisSwing;

        public WeaponDefinition Weapon => weapon;
        public AttackPhase Phase { get; private set; }
        public bool IsAttacking => Phase != AttackPhase.None;
        public bool CanSwitchWeapon => Phase == AttackPhase.None || Phase == AttackPhase.Recovery;
        public float PhaseProgress => Phase == AttackPhase.None ? 0f : Mathf.Clamp01(phaseTime / CurrentPhaseDuration);

        public event Action SwingStarted;
        public event Action TargetHit;
        public event Action<WeaponDefinition> WeaponChanged;

        private float CurrentPhaseDuration
        {
            get
            {
                switch (Phase)
                {
                    case AttackPhase.Windup: return weapon.Windup;
                    case AttackPhase.Active: return weapon.Active;
                    case AttackPhase.Recovery: return weapon.Recovery;
                    default: return 1f;
                }
            }
        }

        public void Initialize(ProjectileSystem projectileSystem) => projectiles = projectileSystem;

        public void Equip(WeaponDefinition definition)
        {
            CancelAttack();
            comboCount = 0;
            if (weapon == definition) return;
            weapon = definition;
            WeaponChanged?.Invoke(weapon);
        }

        public bool TryAttack()
        {
            if (IsAttacking || weapon == null) return false;
            if (Time.time - lastSwingTime > ComboWindow) comboCount = 0;
            comboCount++;
            lastSwingTime = Time.time;
            swingDamage = weapon.ComboEvery > 0 && comboCount % weapon.ComboEvery == 0 ? weapon.ComboDamage : weapon.Damage;
            hitThisSwing.Clear();
            firedThisSwing = false;
            Phase = AttackPhase.Windup;
            phaseTime = 0f;
            SwingStarted?.Invoke();
            return true;
        }

        public void CancelAttack()
        {
            Phase = AttackPhase.None;
            phaseTime = 0f;
        }

        public void Tick(float deltaTime)
        {
            if (!IsAttacking) return;
            phaseTime += deltaTime;
            while (IsAttacking && phaseTime >= CurrentPhaseDuration)
            {
                phaseTime -= CurrentPhaseDuration;
                if (Phase == AttackPhase.Active) ResolveAttack();
                Phase = Phase == AttackPhase.Recovery ? AttackPhase.None : Phase + 1;
                if (Phase == AttackPhase.Active) ResolveAttack();
            }

            if (Phase == AttackPhase.Active) ResolveAttack();
            if (!IsAttacking) phaseTime = 0f;
        }

        private void ResolveAttack()
        {
            if (weapon.IsRanged) FireProjectile();
            else ResolveMeleeHits();
        }

        private void FireProjectile()
        {
            if (firedThisSwing || projectiles == null) return;
            firedThisSwing = true;
            var center = transform.position + Vector3.up * MuzzleHeight;
            var muzzle = center + transform.forward * MuzzleDistance;
            if (Physics.Linecast(center, muzzle, obstacleMask, QueryTriggerInteraction.Ignore)) return;
            projectiles.Fire(weapon.Projectile, new ProjectileShot(muzzle, transform.forward, weapon.ProjectileSpeed, weapon.Range, swingDamage, weapon.Knockback, targetMask, obstacleMask));
            TargetHit?.Invoke();
        }

        private void ResolveMeleeHits()
        {
            var origin = transform.position + Vector3.up * OriginHeight;
            var count = Physics.OverlapSphereNonAlloc(origin, weapon.Range, overlapBuffer, targetMask, QueryTriggerInteraction.Collide);
            for (var i = 0; i < count; i++)
            {
                var target = overlapBuffer[i];
                var damageable = target.GetComponentInParent<IDamageable>();
                if (damageable == null || damageable.IsDead || hitThisSwing.Contains(damageable)) continue;

                var center = target.bounds.center;
                var toTarget = center - origin;
                toTarget.y = 0f;
                if (toTarget.sqrMagnitude > 0.01f && Vector3.Angle(transform.forward, toTarget) > weapon.ArcDegrees * 0.5f) continue;

                // Swings pass over waist-high props, so a target across a low obstacle can still be hit.
                var swingFrom = new Vector3(origin.x, transform.position.y + SwingHeight, origin.z);
                var swingTo = new Vector3(center.x, swingFrom.y, center.z);
                if (Physics.Linecast(swingFrom, swingTo, obstacleMask, QueryTriggerInteraction.Ignore)) continue;

                hitThisSwing.Add(damageable);
                var direction = toTarget.sqrMagnitude > 0.01f ? toTarget.normalized : transform.forward;
                if (damageable.ApplyDamage(new DamageInfo(swingDamage, direction, weapon.Knockback))) TargetHit?.Invoke();
            }
        }
    }
}
