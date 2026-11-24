using System;
using System.Linq;
using UnityEngine;
using VorgGates.Core;

namespace VorgGates.Gameplay
{
    /// <summary>Owns the current dungeon: generates, builds, tracks progress and tears it down.</summary>
    public sealed class DungeonDirector : MonoBehaviour
    {
        [SerializeField] private DungeonDefinition[] campaign;
        [SerializeField] private DungeonBuilder builder;
        [SerializeField] private RuntimeNavMesh navMesh;
        [SerializeField] private ProjectileSystem projectiles;

        private readonly System.Random seedSource = new System.Random();
        private PlayerStateMachine player;
        private ulong previousSignature;

        public DungeonDefinition Definition { get; private set; }
        public int DungeonIndex { get; private set; }
        public DungeonLayout Layout { get; private set; }
        public BuiltDungeon Current { get; private set; }
        public int CombatRoomsRemaining { get; private set; }
        public RoomController ActiveRoom { get; private set; }
        public float LastBuildMilliseconds { get; private set; }
        public ProjectileSystem Projectiles => projectiles;
        public int CampaignLength => campaign.Length;

        /// <summary>Makes the next build fail once, to verify that a failed transition keeps the checkpoint.</summary>
        public bool FailNextBuild { get; set; }

        public EnemyBrain ActiveBoss
        {
            get
            {
                if (ActiveRoom == null) return null;
                var enemies = ActiveRoom.Enemies;
                for (var i = 0; i < enemies.Count; i++)
                {
                    if (enemies[i] != null && enemies[i].IsBoss && !enemies[i].Health.IsDead) return enemies[i];
                }

                return null;
            }
        }

        public event Action PortalUsed;
        public event Action SwordTaken;
        public event Action RoomStarted;
        public event Action RoomFinished;
        public event Action<EnemyBrain> EnemySpawned;

        public DungeonDefinition DefinitionAt(int index) => campaign[index];

        public void Initialize(PlayerStateMachine playerState)
        {
            player = playerState;
            player.Weapon.Initialize(projectiles);
        }

        public bool Rebuild(int dungeonIndex, int? explicitSeed = null)
        {
            if (FailNextBuild)
            {
                FailNextBuild = false;
                return false;
            }

            var timer = System.Diagnostics.Stopwatch.StartNew();
            Teardown();
            projectiles.ClearAll();

            if (dungeonIndex != DungeonIndex) previousSignature = 0UL;
            DungeonIndex = dungeonIndex;
            Definition = campaign[dungeonIndex];
            var config = Definition.ToConfig(Definition.Kit.CombatInteriors.Length);
            Layout = explicitSeed.HasValue
                ? DungeonGenerator.Generate(config, explicitSeed.Value)
                : DungeonGenerator.GenerateDifferent(config, seedSource.Next(), previousSignature);
            if (!LayoutValidator.Validate(Layout, config, out var error))
            {
                Debug.LogError($"Vorg'Gates: invalid layout for seed {Layout.Seed}: {error}");
                return false;
            }

            previousSignature = Layout.Signature;
            Current = builder.Build(Definition, Layout, player, projectiles);
            if (!navMesh.Build(Current.Bounds)) return false;

            CombatRoomsRemaining = Layout.Rooms.Count(r => r.Kind == RoomKind.Combat || r.Kind == RoomKind.Boss);
            foreach (var room in Current.Rooms)
            {
                room.Activated += OnRoomActivated;
                room.Cleared += OnRoomCleared;
                room.EnemySpawned += OnEnemySpawned;
            }

            if (Current.Portal != null)
            {
                Current.Portal.SetAvailable(false);
                Current.Portal.Used += OnPortalUsed;
            }

            if (Current.Sword != null)
            {
                Current.Sword.SetAvailable(false);
                Current.Sword.Taken += OnSwordTaken;
            }

            LastBuildMilliseconds = (float)timer.Elapsed.TotalMilliseconds;
            return true;
        }

        private void OnDestroy() => Teardown();

        private void Teardown()
        {
            ActiveRoom = null;
            if (Current == null) return;
            foreach (var room in Current.Rooms)
            {
                room.Activated -= OnRoomActivated;
                room.Cleared -= OnRoomCleared;
                room.EnemySpawned -= OnEnemySpawned;
            }

            if (Current.Portal != null) Current.Portal.Used -= OnPortalUsed;
            if (Current.Sword != null) Current.Sword.Taken -= OnSwordTaken;

            // The root is already gone when the whole scene is unloading.
            if (Current.Root != null)
            {
                // Inactive objects leave physics at once, so the next NavMesh never collects old colliders.
                Current.Root.SetActive(false);
                Destroy(Current.Root);
            }

            Current = null;
        }

        private void OnRoomActivated(RoomController room)
        {
            ActiveRoom = room;
            RoomStarted?.Invoke();
        }

        private void OnEnemySpawned(EnemyBrain enemy) => EnemySpawned?.Invoke(enemy);

        private void OnRoomCleared(RoomController room)
        {
            if (ActiveRoom == room) ActiveRoom = null;
            RoomFinished?.Invoke();
            CombatRoomsRemaining--;
            if (CombatRoomsRemaining > 0) return;
            if (Current.Portal != null) Current.Portal.SetAvailable(true);
            if (Current.Sword != null) Current.Sword.SetAvailable(true);
        }

        private void OnPortalUsed() => PortalUsed?.Invoke();

        private void OnSwordTaken() => SwordTaken?.Invoke();
    }
}
