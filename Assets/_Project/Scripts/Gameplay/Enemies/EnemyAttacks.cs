using UnityEngine;
using UnityEngine.AI;

namespace VorgGates.Gameplay
{
    internal abstract class EnemyAttack
    {
        protected EnemyAttack(EnemyBrain brain) => Brain = brain;

        protected EnemyBrain Brain { get; }
        protected EnemyDefinition Definition => Brain.Definition;

        /// <summary>Called when the telegraph starts. Returns the danger area to draw, if any.</summary>
        public virtual TelegraphInfo PrepareTelegraph() => null;

        public abstract void Begin();

        /// <summary>Advances the attack and returns true when it has finished.</summary>
        public abstract bool Tick(float deltaTime);

        public static EnemyAttack Create(EnemyBrain brain)
        {
            switch (brain.Definition.Behavior)
            {
                case EnemyBehavior.Ranged: return new ProjectileAttack(brain);
                case EnemyBehavior.Charger: return new LungeAttack(brain, false);
                case EnemyBehavior.Boss: return null;
                default: return new LungeAttack(brain, true);
            }
        }
    }

    /// <summary>A straight rush. The crawler stops at the target, the brute charges past it.</summary>
    internal sealed class LungeAttack : EnemyAttack
    {
        private const float PersonalSpace = 1.15f;
        private const float MinimumFacing = 0.25f;

        private readonly bool stopsAtTarget;
        private Vector3 direction;
        private float elapsed;
        private bool landed;

        public LungeAttack(EnemyBrain brain, bool stopsAtTarget) : base(brain) => this.stopsAtTarget = stopsAtTarget;

        public override void Begin()
        {
            direction = Brain.transform.forward;
            elapsed = 0f;
            landed = false;
        }

        public override bool Tick(float deltaTime)
        {
            var step = Definition.LungeDistance / Definition.LungeDuration * deltaTime;
            var toTarget = Brain.Target.position - Brain.transform.position;
            toTarget.y = 0f;
            var distance = toTarget.magnitude;
            if (stopsAtTarget && Vector3.Dot(toTarget, direction) > 0f) step = Mathf.Min(step, Mathf.Max(0f, distance - PersonalSpace));
            Brain.Agent.Move(direction * step);

            if (!landed && distance <= Definition.HitRadius && (distance < 0.01f || Vector3.Dot(toTarget / distance, direction) > MinimumFacing))
                landed = Brain.TargetDamageable.ApplyDamage(new DamageInfo(Brain.ScaledDamage, direction, 0f));

            elapsed += deltaTime;
            return elapsed >= Definition.LungeDuration;
        }
    }

    internal sealed class ProjectileAttack : EnemyAttack
    {
        private const float MuzzleHeight = 1.15f;
        private const float MuzzleDistance = 0.75f;

        private float elapsed;

        public ProjectileAttack(EnemyBrain brain) : base(brain)
        {
        }

        public override void Begin()
        {
            elapsed = 0f;
            var origin = Brain.transform.position + Vector3.up * MuzzleHeight + Brain.transform.forward * MuzzleDistance;
            var shot = new ProjectileShot(origin, Brain.transform.forward, Definition.ProjectileSpeed, Definition.AttackRange + 5f, Brain.ScaledDamage, 0f, Brain.TargetMask, Brain.ObstacleMask);
            Brain.Projectiles.Fire(Definition.Projectile, shot);
        }

        public override bool Tick(float deltaTime)
        {
            elapsed += deltaTime;
            return elapsed >= Definition.LungeDuration;
        }
    }

    /// <summary>One data-driven boss attack. Damage only resolves inside the area shown by the telegraph.</summary>
    internal sealed class BossPatternAttack : EnemyAttack
    {
        private const float ProjectileHeight = 1.15f;
        private const float MuzzleDistance = 1.2f;
        private const float BurstSpread = 4.5f;
        private const float BlinkDistance = 2.4f;
        private const float ChargeWidth = 2.4f;

        private Vector3 center;
        private Vector3[] burstPoints = System.Array.Empty<Vector3>();
        private Vector3 direction;
        private float elapsed;
        private bool landed;

        public BossPatternAttack(EnemyBrain brain, BossPattern pattern) : base(brain) => Pattern = pattern;

        public BossPattern Pattern { get; }

        public override TelegraphInfo PrepareTelegraph()
        {
            var info = new TelegraphInfo { Duration = Brain.CurrentTelegraph, Radius = Pattern.radius };
            switch (Pattern.kind)
            {
                case BossPatternKind.Slam:
                case BossPatternKind.Nova:
                    center = Brain.transform.position;
                    info.Shape = TelegraphShape.Circle;
                    info.Points = new[] { center };
                    break;
                case BossPatternKind.BlinkSlam:
                    center = PointNearTarget(BlinkDistance);
                    info.Shape = TelegraphShape.Circle;
                    info.Points = new[] { center };
                    break;
                case BossPatternKind.Cone:
                case BossPatternKind.Volley:
                    info.Shape = TelegraphShape.Cone;
                    info.ArcDegrees = Pattern.arcDegrees;
                    break;
                case BossPatternKind.Charge:
                    info.Shape = TelegraphShape.Line;
                    info.Length = Pattern.distance;
                    info.Width = ChargeWidth;
                    break;
                case BossPatternKind.Bursts:
                    burstPoints = new Vector3[Pattern.count];
                    burstPoints[0] = PointNearTarget(0f);
                    for (var i = 1; i < burstPoints.Length; i++) burstPoints[i] = PointNearTarget(BurstSpread);
                    info.Shape = TelegraphShape.Bursts;
                    info.Points = burstPoints;
                    break;
            }

            return info;
        }

        public override void Begin()
        {
            elapsed = 0f;
            landed = false;
            direction = Brain.transform.forward;
            switch (Pattern.kind)
            {
                case BossPatternKind.BlinkSlam:
                    Brain.Agent.Warp(center);
                    HitIfInsideCircle(center);
                    break;
                case BossPatternKind.Slam:
                    HitIfInsideCircle(center);
                    break;
                case BossPatternKind.Cone:
                    HitIfInsideCone();
                    break;
                case BossPatternKind.Volley:
                    FireFan(Pattern.arcDegrees);
                    break;
                case BossPatternKind.Nova:
                    FireFan(360f);
                    break;
                case BossPatternKind.Bursts:
                    foreach (var point in burstPoints) HitIfInsideCircle(point);
                    break;
            }
        }

        public override bool Tick(float deltaTime)
        {
            if (Pattern.kind == BossPatternKind.Charge) TickCharge(deltaTime);
            elapsed += deltaTime;
            return elapsed >= Pattern.duration;
        }

        private void TickCharge(float deltaTime)
        {
            Brain.Agent.Move(direction * (Pattern.distance / Pattern.duration * deltaTime));
            if (landed) return;
            var toTarget = Brain.Target.position - Brain.transform.position;
            toTarget.y = 0f;
            if (toTarget.magnitude <= ChargeWidth * 0.75f) landed = Damage();
        }

        private void HitIfInsideCircle(Vector3 point)
        {
            if (landed) return;
            var delta = Brain.Target.position - point;
            delta.y = 0f;
            if (delta.magnitude <= Pattern.radius) landed = Damage();
        }

        private void HitIfInsideCone()
        {
            var toTarget = Brain.Target.position - Brain.transform.position;
            toTarget.y = 0f;
            if (toTarget.magnitude > Pattern.radius) return;
            if (toTarget.sqrMagnitude < 0.01f || Vector3.Angle(direction, toTarget) <= Pattern.arcDegrees * 0.5f) landed = Damage();
        }

        private void FireFan(float arc)
        {
            var isFullCircle = arc >= 359f;
            var steps = isFullCircle ? Pattern.count : Mathf.Max(1, Pattern.count - 1);
            var start = isFullCircle ? Random.value * 360f / Pattern.count : -arc * 0.5f;
            for (var i = 0; i < Pattern.count; i++)
            {
                var shotDirection = Quaternion.Euler(0f, start + arc * i / steps, 0f) * direction;
                var origin = Brain.transform.position + Vector3.up * ProjectileHeight + shotDirection * MuzzleDistance;
                Brain.Projectiles.Fire(Definition.Projectile, new ProjectileShot(origin, shotDirection, Pattern.projectileSpeed, Pattern.range + 8f, Pattern.damage, 0f, Brain.TargetMask, Brain.ObstacleMask));
            }
        }

        private bool Damage() => Brain.TargetDamageable.ApplyDamage(new DamageInfo(Pattern.damage, direction, 0f));

        private Vector3 PointNearTarget(float spread)
        {
            var offset = Random.insideUnitCircle * spread;
            var candidate = Brain.Target.position + new Vector3(offset.x, 0f, offset.y);
            return NavMesh.SamplePosition(candidate, out var hit, 3f, NavMesh.AllAreas) ? hit.position : Brain.Target.position;
        }
    }
}
