using System;
using UnityEngine;
using UnityEngine.AI;
using VorgGates.Core;

namespace VorgGates.Gameplay
{
    public enum EnemyState
    {
        Dormant,
        Chase,
        Telegraph,
        Attack,
        Recover,
        Dead
    }

    [RequireComponent(typeof(NavMeshAgent), typeof(Health))]
    public sealed class EnemyBrain : MonoBehaviour
    {
        private const float RepathInterval = 0.15f;
        private const float KnockbackDecay = 11f;
        private const float DestroyDelay = 0.45f;
        private const float BossDestroyDelay = 1.1f;
        private const float RetreatWindow = 0.9f;
        private const float RetreatDistance = 4f;
        private const float SightHeight = 1.15f;
        private const float EnrageThreshold = 0.5f;
        private const float EnrageTelegraph = 0.85f;
        private const float EnrageRecovery = 0.8f;

        [SerializeField] private EnemyDefinition definition;
        [SerializeField] private LayerMask targetMask;
        [SerializeField] private LayerMask obstacleMask;

        private NavMeshAgent agent;
        private Health health;
        private Collider bodyCollider;
        private EnemyAttack attack;
        private BossPatternAttack[] bossAttacks = Array.Empty<BossPatternAttack>();
        private int patternIndex;
        private AttackTokenPool rangedTokens;
        private Vector3 knockback;
        private float damageScale = 1f;
        private float stateTimer;
        private float repathTimer;
        private float retreatTimer;
        private bool holdsToken;

        public EnemyState State { get; private set; } = EnemyState.Dormant;
        public EnemyDefinition Definition => definition;
        public Health Health => health;
        public bool IsBoss => definition.Behavior == EnemyBehavior.Boss;
        public bool IsEnraged => IsBoss && health.Normalized < EnrageThreshold;
        public int ScaledDamage => Mathf.RoundToInt(definition.Damage * damageScale);
        public float TelegraphRemaining => State == EnemyState.Telegraph ? Mathf.Max(0f, stateTimer) : 0f;
        public BossPatternKind? CurrentPattern => IsBoss ? bossAttacks[patternIndex].Pattern.kind : (BossPatternKind?)null;

        public float CurrentTelegraph
        {
            get
            {
                if (!IsBoss) return definition.Telegraph;
                var telegraph = bossAttacks[patternIndex].Pattern.telegraph;
                return IsEnraged ? telegraph * EnrageTelegraph : telegraph;
            }
        }

        private float CurrentRecovery
        {
            get
            {
                if (!IsBoss) return definition.Recovery;
                var recovery = bossAttacks[patternIndex].Pattern.recovery;
                return IsEnraged ? recovery * EnrageRecovery : recovery;
            }
        }

        private float CurrentRange => IsBoss ? bossAttacks[patternIndex].Pattern.range : definition.AttackRange;

        internal NavMeshAgent Agent => agent;
        internal Transform Target { get; private set; }
        internal IDamageable TargetDamageable { get; private set; }
        internal ProjectileSystem Projectiles { get; private set; }
        internal LayerMask TargetMask => targetMask;
        internal LayerMask ObstacleMask => obstacleMask;

        public event Action<EnemyState> StateChanged;
        public event Action<EnemyBrain> Defeated;
        public event Action<TelegraphInfo> TelegraphStarted;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            health = GetComponent<Health>();
            bodyCollider = GetComponent<Collider>();
            health.Configure(definition.MaxHealth);
            agent.speed = definition.MoveSpeed;
            agent.updateRotation = false;
            agent.stoppingDistance = definition.Behavior == EnemyBehavior.MeleeLunge ? definition.AttackRange * 0.75f : 0.5f;
            if (IsBoss)
            {
                bossAttacks = new BossPatternAttack[definition.Patterns.Length];
                for (var i = 0; i < bossAttacks.Length; i++) bossAttacks[i] = new BossPatternAttack(this, definition.Patterns[i]);
                attack = bossAttacks[0];
            }
            else
            {
                attack = EnemyAttack.Create(this);
            }

            health.Damaged += OnDamaged;
            health.Died += OnDied;
        }

        private void OnDestroy()
        {
            ReleaseToken();
            health.Damaged -= OnDamaged;
            health.Died -= OnDied;
        }

        /// <summary>Applies the difficulty of the current phase. Bosses use their own values.</summary>
        public void ApplyDifficulty(float healthScale, float attackScale)
        {
            if (IsBoss) return;
            health.Configure(Mathf.Max(1, Mathf.RoundToInt(definition.MaxHealth * healthScale)));
            damageScale = attackScale;
        }

        public void Prepare(float spawnProtection)
        {
            if (NavMesh.SamplePosition(transform.position, out var hit, 2f, NavMesh.AllAreas)) agent.Warp(hit.position);
            health.GrantInvulnerability(spawnProtection);
            if (agent.isOnNavMesh) agent.isStopped = true;
        }

        /// <summary>Returns a misplaced enemy to a valid point, so a room can never become impossible to clear.</summary>
        public void RecoverTo(Vector3 position)
        {
            if (State == EnemyState.Dead || !agent.enabled) return;
            knockback = Vector3.zero;
            if (NavMesh.SamplePosition(position, out var hit, 2f, NavMesh.AllAreas)) agent.Warp(hit.position);
        }

        public bool IsOnNavMesh => agent.enabled && agent.isOnNavMesh;
        public Transform CurrentTarget => Target;

        public void Activate(Transform newTarget, IDamageable damageable, ProjectileSystem projectiles, AttackTokenPool tokens)
        {
            if (State != EnemyState.Dormant) return;
            Target = newTarget;
            TargetDamageable = damageable;
            Projectiles = projectiles;
            rangedTokens = tokens;
            repathTimer = UnityEngine.Random.value * RepathInterval;
            Enter(EnemyState.Chase, 0f);
        }

        private void Update()
        {
            if (State == EnemyState.Dormant || State == EnemyState.Dead || !agent.isOnNavMesh) return;

            var deltaTime = Time.deltaTime;
            ApplyKnockback(deltaTime);
            if (TargetDamageable == null || TargetDamageable.IsDead)
            {
                agent.isStopped = true;
                return;
            }

            stateTimer -= deltaTime;
            switch (State)
            {
                case EnemyState.Chase:
                    TickChase(deltaTime);
                    break;
                case EnemyState.Telegraph:
                    TickTelegraph();
                    break;
                case EnemyState.Attack:
                    if (attack.Tick(deltaTime)) FinishAttack();
                    break;
                case EnemyState.Recover:
                    if (stateTimer <= 0f)
                    {
                        retreatTimer = RetreatWindow;
                        Enter(EnemyState.Chase, 0f);
                    }

                    break;
            }
        }

        private void FinishAttack()
        {
            ReleaseToken();
            var recovery = CurrentRecovery;
            if (IsBoss)
            {
                patternIndex = (patternIndex + 1) % bossAttacks.Length;
                attack = bossAttacks[patternIndex];
            }

            Enter(EnemyState.Recover, recovery);
        }

        private void TickChase(float deltaTime)
        {
            agent.isStopped = false;
            FaceTarget();
            var distance = FlatDistanceToTarget();
            repathTimer -= deltaTime;
            retreatTimer -= deltaTime;

            if (definition.Behavior == EnemyBehavior.Ranged && distance < definition.PreferredMinRange && retreatTimer > 0f)
            {
                if (repathTimer <= 0f)
                {
                    repathTimer = RepathInterval;
                    Retreat();
                }

                return;
            }

            if (repathTimer <= 0f)
            {
                repathTimer = RepathInterval;
                agent.SetDestination(Target.position);
            }

            if (!CanStartAttack(distance)) return;
            agent.isStopped = true;
            var info = attack.PrepareTelegraph();
            Enter(EnemyState.Telegraph, CurrentTelegraph);
            if (info != null) TelegraphStarted?.Invoke(info);
        }

        private bool CanStartAttack(float distance)
        {
            if (distance > CurrentRange) return false;
            if (definition.Behavior == EnemyBehavior.MeleeLunge) return true;
            if (!HasLineOfSight()) return false;
            if (definition.Behavior != EnemyBehavior.Ranged) return true;
            if (!rangedTokens.TryAcquire()) return false;
            holdsToken = true;
            return true;
        }

        private void Retreat()
        {
            var away = transform.position - Target.position;
            away.y = 0f;
            var destination = transform.position + away.normalized * RetreatDistance;
            if (NavMesh.SamplePosition(destination, out var hit, 2f, NavMesh.AllAreas)) agent.SetDestination(hit.position);
        }

        private void TickTelegraph()
        {
            if (stateTimer > CurrentTelegraph * 0.3f) FaceTarget();
            if (stateTimer > 0f) return;
            Enter(EnemyState.Attack, 0f);
            attack.Begin();
        }

        private bool HasLineOfSight()
        {
            var from = transform.position + Vector3.up * SightHeight;
            var to = Target.position + Vector3.up * SightHeight;
            return !Physics.Linecast(from, to, obstacleMask, QueryTriggerInteraction.Ignore);
        }

        private void ApplyKnockback(float deltaTime)
        {
            if (knockback.sqrMagnitude < 0.01f) return;
            agent.Move(knockback * deltaTime);
            knockback = Vector3.Lerp(knockback, Vector3.zero, KnockbackDecay * deltaTime);
        }

        private void FaceTarget()
        {
            var direction = Target.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        private float FlatDistanceToTarget()
        {
            var delta = Target.position - transform.position;
            delta.y = 0f;
            return delta.magnitude;
        }

        private void OnDamaged(DamageInfo damage) =>
            knockback = damage.Direction * (damage.Knockback * (1f - definition.KnockbackResistance));

        private void OnDied()
        {
            ReleaseToken();
            Enter(EnemyState.Dead, 0f);
            if (bodyCollider != null) bodyCollider.enabled = false;
            agent.enabled = false;
            Defeated?.Invoke(this);
            Destroy(gameObject, IsBoss ? BossDestroyDelay : DestroyDelay);
        }

        private void ReleaseToken()
        {
            if (!holdsToken) return;
            holdsToken = false;
            rangedTokens.Release();
        }

        private void Enter(EnemyState next, float duration)
        {
            State = next;
            stateTimer = duration;
            StateChanged?.Invoke(next);
        }
    }
}
