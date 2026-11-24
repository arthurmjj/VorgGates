using System;
using System.Collections.Generic;

namespace VorgGates.Core
{
    public enum RoomKind
    {
        Entrance,
        Combat,
        Reward,
        Portal,
        SwordChamber,
        Boss
    }

    public sealed class DungeonConfig
    {
        public DungeonConfig(int mainPathRooms, int sideRooms, int combatVariants, int minEnemies, int enemyStep, int maxEnemies, int maxAttempts = 20, int enemyTier = 3, bool hasBoss = false, RoomKind exitKind = RoomKind.Portal, int rewardHostIndex = -1)
        {
            if (mainPathRooms < (hasBoss ? 4 : 3)) throw new ArgumentOutOfRangeException(nameof(mainPathRooms));
            if (sideRooms < 0) throw new ArgumentOutOfRangeException(nameof(sideRooms));
            if (combatVariants < 1) throw new ArgumentOutOfRangeException(nameof(combatVariants));
            if (minEnemies < 1 || maxEnemies < minEnemies) throw new ArgumentOutOfRangeException(nameof(minEnemies));
            if (exitKind != RoomKind.Portal && exitKind != RoomKind.SwordChamber) throw new ArgumentOutOfRangeException(nameof(exitKind));
            var combatRooms = mainPathRooms - 2 - (hasBoss ? 1 : 0);
            if (rewardHostIndex >= combatRooms) throw new ArgumentOutOfRangeException(nameof(rewardHostIndex));
            MainPathRooms = mainPathRooms;
            SideRooms = sideRooms;
            CombatVariants = combatVariants;
            MinEnemies = minEnemies;
            EnemyStep = enemyStep;
            MaxEnemies = maxEnemies;
            MaxAttempts = maxAttempts;
            EnemyTier = enemyTier;
            HasBoss = hasBoss;
            ExitKind = exitKind;
            RewardHostIndex = rewardHostIndex;
        }

        public int MainPathRooms { get; }
        public int SideRooms { get; }
        public int CombatVariants { get; }
        public int MinEnemies { get; }
        public int EnemyStep { get; }
        public int MaxEnemies { get; }
        public int MaxAttempts { get; }
        public int EnemyTier { get; }
        public bool HasBoss { get; }
        public RoomKind ExitKind { get; }

        /// <summary>Index among combat rooms that hosts the first side room, or -1 for any.</summary>
        public int RewardHostIndex { get; }

        public int CombatRooms => MainPathRooms - 2 - (HasBoss ? 1 : 0);
    }

    public sealed class RoomNode
    {
        public RoomNode(int id, GridPos position, RoomKind kind, int depth, bool onMainPath)
        {
            Id = id;
            Position = position;
            Kind = kind;
            Depth = depth;
            IsOnMainPath = onMainPath;
        }

        public int Id { get; }
        public GridPos Position { get; }
        public RoomKind Kind { get; }
        public int Depth { get; }
        public bool IsOnMainPath { get; }
        public int DoorMask { get; internal set; }
        public int InteriorVariant { get; internal set; }
        public int EnemyCount { get; internal set; }
        public EnemyKind[] Encounter { get; internal set; } = Array.Empty<EnemyKind>();

        public bool HasDoor(Side side) => (DoorMask & side.Bit()) != 0;
    }

    public readonly struct RoomLink
    {
        public RoomLink(int from, int to, Side side)
        {
            From = from;
            To = to;
            Side = side;
        }

        public int From { get; }
        public int To { get; }
        public Side Side { get; }
    }

    public sealed class DungeonLayout
    {
        internal DungeonLayout(int seed, List<RoomNode> rooms, List<RoomLink> links, bool isFallback)
        {
            Seed = seed;
            Rooms = rooms;
            Links = links;
            IsFallback = isFallback;
            Signature = ComputeSignature();
        }

        public int Seed { get; }
        public IReadOnlyList<RoomNode> Rooms { get; }
        public IReadOnlyList<RoomLink> Links { get; }
        public bool IsFallback { get; }
        public ulong Signature { get; }

        private ulong ComputeSignature()
        {
            const ulong offset = 14695981039346656037UL;
            const ulong prime = 1099511628211UL;
            var hash = offset;
            void Add(int value)
            {
                unchecked
                {
                    hash = (hash ^ (uint)value) * prime;
                }
            }

            foreach (var room in Rooms)
            {
                Add(room.Position.X);
                Add(room.Position.Y);
                Add((int)room.Kind);
                Add(room.DoorMask);
            }

            return hash;
        }
    }
}
