using System.Collections.Generic;

namespace VorgGates.Core
{
    /// <summary>Generates a room graph: a guaranteed main path plus optional side rooms.</summary>
    public static class DungeonGenerator
    {
        private const int AttemptSeedStep = 104729;
        private const int MaxDifferentAttempts = 64;
        private static readonly Side[] AllSides = { Side.North, Side.East, Side.South, Side.West };

        public static DungeonLayout Generate(DungeonConfig config, int seed)
        {
            for (var attempt = 0; attempt < config.MaxAttempts; attempt++)
            {
                var random = new SeededRandom(unchecked(seed + attempt * AttemptSeedStep));
                var layout = TryBuild(config, seed, random);
                if (layout != null && LayoutValidator.Validate(layout, config, out _)) return layout;
            }

            return BuildFallback(config, seed);
        }

        /// <summary>Generates a layout whose shape differs from the previous attempt.</summary>
        public static DungeonLayout GenerateDifferent(DungeonConfig config, int seed, ulong previousSignature)
        {
            var candidateSeed = seed;
            var layout = Generate(config, candidateSeed);
            for (var i = 0; i < MaxDifferentAttempts && layout.Signature == previousSignature; i++)
            {
                candidateSeed = unchecked(candidateSeed * 31 + 7919);
                layout = Generate(config, candidateSeed);
            }

            return layout;
        }

        private static RoomKind KindAt(DungeonConfig config, int index)
        {
            if (index == 0) return RoomKind.Entrance;
            if (index == config.MainPathRooms - 1) return config.ExitKind;
            return config.HasBoss && index == config.MainPathRooms - 2 ? RoomKind.Boss : RoomKind.Combat;
        }

        private static DungeonLayout TryBuild(DungeonConfig config, int seed, SeededRandom random)
        {
            var path = new List<GridPos> { new GridPos(0, 0) };
            var occupied = new HashSet<GridPos> { path[0] };
            if (!ExtendPath(path, occupied, config.MainPathRooms, random)) return null;

            var rooms = new List<RoomNode>(config.MainPathRooms + config.SideRooms);
            var links = new List<RoomLink>(config.MainPathRooms + config.SideRooms);
            for (var i = 0; i < path.Count; i++)
            {
                rooms.Add(new RoomNode(i, path[i], KindAt(config, i), i, true));
                if (i > 0) Connect(rooms, links, i - 1, i, SideBetween(path[i - 1], path[i]));
            }

            if (!AddSideRooms(config, rooms, links, occupied, random)) return null;
            AssignEncounters(config, rooms, random);
            return new DungeonLayout(seed, rooms, links, false);
        }

        private static bool ExtendPath(List<GridPos> path, HashSet<GridPos> occupied, int target, SeededRandom random)
        {
            if (path.Count == target) return true;
            var sides = (Side[])AllSides.Clone();
            random.Shuffle(sides);
            foreach (var side in sides)
            {
                var next = path[path.Count - 1].Step(side);
                if (!occupied.Add(next)) continue;
                path.Add(next);
                if (ExtendPath(path, occupied, target, random)) return true;
                path.RemoveAt(path.Count - 1);
                occupied.Remove(next);
            }

            return false;
        }

        private static bool AddSideRooms(DungeonConfig config, List<RoomNode> rooms, List<RoomLink> links, HashSet<GridPos> occupied, SeededRandom random)
        {
            var combatRooms = new List<RoomNode>();
            foreach (var room in rooms)
            {
                if (room.Kind == RoomKind.Combat) combatRooms.Add(room);
            }

            for (var added = 0; added < config.SideRooms; added++)
            {
                var hosts = new List<RoomNode>(combatRooms);
                if (added == 0 && config.RewardHostIndex >= 0)
                {
                    hosts.Clear();
                    hosts.Add(combatRooms[config.RewardHostIndex]);
                }
                else
                {
                    random.Shuffle(hosts);
                }

                var placed = false;
                foreach (var host in hosts)
                {
                    var sides = (Side[])AllSides.Clone();
                    random.Shuffle(sides);
                    foreach (var side in sides)
                    {
                        var position = host.Position.Step(side);
                        if (!occupied.Add(position)) continue;
                        var id = rooms.Count;
                        rooms.Add(new RoomNode(id, position, RoomKind.Reward, host.Depth + 1, false));
                        Connect(rooms, links, host.Id, id, side);
                        placed = true;
                        break;
                    }

                    if (placed) break;
                }

                if (!placed) return false;
            }

            return true;
        }

        private static void AssignEncounters(DungeonConfig config, List<RoomNode> rooms, SeededRandom random)
        {
            var combatIndex = 0;
            var previousVariant = -1;
            foreach (var room in rooms)
            {
                if (room.Kind == RoomKind.Boss)
                {
                    room.EnemyCount = 1;
                    room.Encounter = new[] { EnemyKind.Boss };
                    continue;
                }

                if (room.Kind != RoomKind.Combat) continue;
                var enemies = config.MinEnemies + combatIndex * config.EnemyStep;
                room.EnemyCount = enemies > config.MaxEnemies ? config.MaxEnemies : enemies;
                room.Encounter = EncounterPlanner.Plan(combatIndex, room.EnemyCount, config.EnemyTier);
                var variant = random.Next(config.CombatVariants);
                if (config.CombatVariants > 1 && variant == previousVariant) variant = (variant + 1) % config.CombatVariants;
                room.InteriorVariant = variant;
                previousVariant = variant;
                combatIndex++;
            }
        }

        private static DungeonLayout BuildFallback(DungeonConfig config, int seed)
        {
            var rooms = new List<RoomNode>();
            var links = new List<RoomLink>();
            for (var i = 0; i < config.MainPathRooms; i++)
            {
                rooms.Add(new RoomNode(i, new GridPos(0, i), KindAt(config, i), i, true));
                if (i > 0) Connect(rooms, links, i - 1, i, Side.North);
            }

            var combatRooms = config.CombatRooms;
            var firstHost = config.RewardHostIndex >= 0 ? config.RewardHostIndex : 0;
            for (var i = 0; i < config.SideRooms; i++)
            {
                var host = 1 + (i + firstHost) % combatRooms;
                var layer = i / combatRooms;
                var side = layer % 2 == 0 ? Side.East : Side.West;
                var distance = layer / 2 + 1;
                var position = new GridPos(side == Side.East ? distance : -distance, host);
                var id = rooms.Count;
                rooms.Add(new RoomNode(id, position, RoomKind.Reward, host + distance, false));
                Connect(rooms, links, distance == 1 ? host : id - 2 * combatRooms, id, side);
            }

            AssignEncounters(config, rooms, new SeededRandom(seed));
            return new DungeonLayout(seed, rooms, links, true);
        }

        private static void Connect(List<RoomNode> rooms, List<RoomLink> links, int from, int to, Side side)
        {
            rooms[from].DoorMask |= side.Bit();
            rooms[to].DoorMask |= side.Opposite().Bit();
            links.Add(new RoomLink(from, to, side));
        }

        private static Side SideBetween(GridPos from, GridPos to)
        {
            foreach (var side in AllSides)
            {
                if (from.Step(side).Equals(to)) return side;
            }

            throw new System.InvalidOperationException($"Rooms {from} and {to} are not adjacent.");
        }
    }
}
