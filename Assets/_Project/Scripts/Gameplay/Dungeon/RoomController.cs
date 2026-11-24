using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VorgGates.Core;

namespace VorgGates.Gameplay
{
    public enum RoomState
    {
        Dormant,
        Active,
        Cleared
    }

    /// <summary>Runs one combat encounter. Doors stay closed until every enemy is defeated.</summary>
    public sealed class RoomController : MonoBehaviour
    {
        private const float HalfRoom = 9f;
        private const float ThresholdDepth = 1.1f;
        private const float ThresholdHalfWidth = 2.7f;
        private const int MaxRangedAttackers = 3;
        private const float OrbChance = 0.35f;
        private const float EnemyCheckInterval = 1f;
        private const int ClearHealing = 30;

        [SerializeField] private float spawnDelay = 0.6f;

        private readonly List<EnemyBrain> enemies = new List<EnemyBrain>();
        private readonly AttackTokenPool rangedTokens = new AttackTokenPool(MaxRangedAttackers);
        private DoorController[] doors = Array.Empty<DoorController>();
        private Side[] doorSides = Array.Empty<Side>();
        private Transform[] spawnPoints = Array.Empty<Transform>();
        private EnemyBrain[] encounter = Array.Empty<EnemyBrain>();
        private Transform player;
        private Health playerHealth;
        private ProjectileSystem projectiles;
        private HealOrb orbPrefab;
        private Coroutine spawnRoutine;
        private float healthScale = 1f;
        private float damageScale = 1f;
        private float nextEnemyCheck;

        public RoomState State { get; private set; } = RoomState.Cleared;
        public bool HasEncounter { get; private set; }
        public int EnemyCount => encounter.Length;
        public int AliveEnemies { get; private set; }
        public IReadOnlyList<EnemyBrain> Enemies => enemies;
        public IReadOnlyList<DoorController> Doors => doors;
        public IReadOnlyList<Transform> SpawnPoints => spawnPoints;

        public event Action<RoomController> Activated;
        public event Action<RoomController> Cleared;
        public event Action<EnemyBrain> EnemySpawned;

        public void Configure(Transform playerTransform, Health health, EnemyBrain[] encounterPrefabs, Transform[] spawns, DoorController[] roomDoors, Side[] sides, ProjectileSystem projectileSystem, HealOrb orb, float enemyHealthScale, float enemyDamageScale)
        {
            healthScale = enemyHealthScale;
            damageScale = enemyDamageScale;
            player = playerTransform;
            playerHealth = health;
            encounter = encounterPrefabs.Take(spawns.Length).ToArray();
            spawnPoints = spawns;
            doors = roomDoors;
            doorSides = sides;
            projectiles = projectileSystem;
            orbPrefab = orb;
            HasEncounter = true;
            State = RoomState.Dormant;
        }

        public void ConfigureSafe()
        {
            HasEncounter = false;
            State = RoomState.Cleared;
            enabled = false;
        }

        public void ResetRoom()
        {
            if (!HasEncounter) return;
            if (spawnRoutine != null)
            {
                StopCoroutine(spawnRoutine);
                spawnRoutine = null;
            }

            foreach (var enemy in enemies)
            {
                if (enemy == null) continue;
                enemy.Defeated -= OnEnemyDefeated;
                enemy.gameObject.SetActive(false);
                Destroy(enemy.gameObject);
            }

            enemies.Clear();
            AliveEnemies = 0;
            State = RoomState.Dormant;
            foreach (var door in doors) door.SetClosed(false, true);
        }

        private void Update()
        {
            if (player == null) return;
            if (State == RoomState.Active)
            {
                KeepEnemiesInsideRoom();
                return;
            }

            if (State == RoomState.Dormant && IsPastThreshold(transform.InverseTransformPoint(player.position))) Begin();
        }

        private void KeepEnemiesInsideRoom()
        {
            if (Time.time < nextEnemyCheck) return;
            nextEnemyCheck = Time.time + EnemyCheckInterval;
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || enemy.Health.IsDead || enemy.State == EnemyState.Dormant) continue;
                var local = transform.InverseTransformPoint(enemy.transform.position);
                if (enemy.IsOnNavMesh && Mathf.Abs(local.x) < HalfRoom && Mathf.Abs(local.z) < HalfRoom) continue;
                enemy.RecoverTo(spawnPoints[i % spawnPoints.Length].position);
            }
        }

        // The whole floor counts as inside, except the strip at each door, so walls cannot be used to skip a fight.
        private bool IsPastThreshold(Vector3 local)
        {
            if (Mathf.Abs(local.x) >= HalfRoom || Mathf.Abs(local.z) >= HalfRoom) return false;
            foreach (var side in doorSides)
            {
                switch (side)
                {
                    case Side.North:
                        if (local.z > HalfRoom - ThresholdDepth && Mathf.Abs(local.x) < ThresholdHalfWidth) return false;
                        break;
                    case Side.South:
                        if (local.z < -HalfRoom + ThresholdDepth && Mathf.Abs(local.x) < ThresholdHalfWidth) return false;
                        break;
                    case Side.East:
                        if (local.x > HalfRoom - ThresholdDepth && Mathf.Abs(local.z) < ThresholdHalfWidth) return false;
                        break;
                    default:
                        if (local.x < -HalfRoom + ThresholdDepth && Mathf.Abs(local.z) < ThresholdHalfWidth) return false;
                        break;
                }
            }

            return true;
        }

        private void Begin()
        {
            State = RoomState.Active;
            foreach (var door in doors) door.SetClosed(true);
            Activated?.Invoke(this);
            spawnRoutine = StartCoroutine(SpawnEncounter());
        }

        private IEnumerator SpawnEncounter()
        {
            var points = spawnPoints.OrderByDescending(p => (p.position - player.position).sqrMagnitude).Take(encounter.Length).ToArray();
            for (var i = 0; i < points.Length; i++)
            {
                var enemy = Instantiate(encounter[i], points[i].position, points[i].rotation, transform);
                enemy.ApplyDifficulty(healthScale, damageScale);
                enemy.Prepare(spawnDelay);
                enemy.Defeated += OnEnemyDefeated;
                enemies.Add(enemy);
                AliveEnemies++;
                EnemySpawned?.Invoke(enemy);
            }

            yield return new WaitForSeconds(spawnDelay);
            spawnRoutine = null;
            foreach (var enemy in enemies)
            {
                if (enemy != null) enemy.Activate(player, playerHealth, projectiles, rangedTokens);
            }
        }

        private void OnEnemyDefeated(EnemyBrain enemy)
        {
            enemy.Defeated -= OnEnemyDefeated;
            if (orbPrefab != null && UnityEngine.Random.value < OrbChance)
            {
                var orb = Instantiate(orbPrefab, enemy.transform.position + Vector3.up * 0.7f, Quaternion.identity, transform);
                orb.Configure(player, playerHealth);
            }

            AliveEnemies--;
            if (AliveEnemies > 0 || State != RoomState.Active) return;
            State = RoomState.Cleared;
            playerHealth.Heal(ClearHealing);
            foreach (var door in doors) door.SetClosed(false);
            Cleared?.Invoke(this);
        }
    }
}
