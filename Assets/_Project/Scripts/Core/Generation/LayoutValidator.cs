using System.Collections.Generic;

namespace VorgGates.Core
{
    public static class LayoutValidator
    {
        public static bool Validate(DungeonLayout layout, DungeonConfig config, out string error)
        {
            error = FindError(layout, config);
            return error == null;
        }

        private static string FindError(DungeonLayout layout, DungeonConfig config)
        {
            var rooms = layout.Rooms;
            if (rooms.Count != config.MainPathRooms + config.SideRooms) return $"Expected {config.MainPathRooms + config.SideRooms} rooms, found {rooms.Count}.";
            if (layout.Links.Count != rooms.Count - 1) return "The room graph is not a tree.";

            var positions = new HashSet<GridPos>();
            var entrance = -1;
            var exit = -1;
            var boss = -1;
            var rewards = 0;
            foreach (var room in rooms)
            {
                if (!positions.Add(room.Position)) return $"Two rooms overlap at {room.Position}.";
                switch (room.Kind)
                {
                    case RoomKind.Entrance:
                        if (entrance >= 0) return "More than one entrance.";
                        entrance = room.Id;
                        break;
                    case RoomKind.Portal:
                    case RoomKind.SwordChamber:
                        if (room.Kind != config.ExitKind) return "Wrong exit room.";
                        if (exit >= 0) return "More than one exit.";
                        exit = room.Id;
                        break;
                    case RoomKind.Boss:
                        if (boss >= 0) return "More than one boss.";
                        if (room.Encounter.Length != 1 || room.Encounter[0] != EnemyKind.Boss) return "The boss room has no boss.";
                        boss = room.Id;
                        break;
                    case RoomKind.Reward:
                        rewards++;
                        break;
                    case RoomKind.Combat:
                        if (room.EnemyCount < config.MinEnemies || room.EnemyCount > config.MaxEnemies) return $"Room {room.Id} has an invalid encounter.";
                        if (room.Encounter.Length != room.EnemyCount) return $"Room {room.Id} has a mismatched encounter.";
                        if (room.InteriorVariant < 0 || room.InteriorVariant >= config.CombatVariants) return $"Room {room.Id} has an invalid interior.";
                        break;
                }
            }

            if (entrance < 0 || exit < 0) return "Entrance or exit is missing.";
            if (rewards != config.SideRooms) return "Wrong number of side rooms.";
            if (config.HasBoss != (boss >= 0)) return "Boss room does not match the configuration.";

            var neighbors = new List<int>[rooms.Count];
            var expectedMasks = new int[rooms.Count];
            for (var i = 0; i < neighbors.Length; i++) neighbors[i] = new List<int>();
            foreach (var link in layout.Links)
            {
                if (!rooms[link.From].Position.Step(link.Side).Equals(rooms[link.To].Position)) return $"Link {link.From}-{link.To} does not join adjacent rooms.";
                neighbors[link.From].Add(link.To);
                neighbors[link.To].Add(link.From);
                expectedMasks[link.From] |= link.Side.Bit();
                expectedMasks[link.To] |= link.Side.Opposite().Bit();
            }

            for (var i = 0; i < rooms.Count; i++)
            {
                if (rooms[i].DoorMask != expectedMasks[i]) return $"Room {i} doors do not match its links.";
            }

            if (config.SideRooms > 0 && config.RewardHostIndex >= 0)
            {
                var firstReward = config.MainPathRooms;
                var expectedHost = config.RewardHostIndex + 1;
                if (!neighbors[firstReward].Contains(expectedHost)) return "The reward is not attached to the required combat room.";
            }

            var previous = new int[rooms.Count];
            for (var i = 0; i < previous.Length; i++) previous[i] = -1;
            var visited = new bool[rooms.Count];
            var queue = new Queue<int>();
            queue.Enqueue(entrance);
            visited[entrance] = true;
            var reached = 0;
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                reached++;
                foreach (var next in neighbors[current])
                {
                    if (visited[next]) continue;
                    visited[next] = true;
                    previous[next] = current;
                    queue.Enqueue(next);
                }
            }

            if (reached != rooms.Count) return "Some rooms are unreachable.";
            if (config.HasBoss && previous[exit] != boss) return "The exit must be behind the boss.";

            var pathLength = 1;
            for (var room = exit; room != entrance; room = previous[room])
            {
                if (room != exit && rooms[room].Kind != RoomKind.Combat && rooms[room].Kind != RoomKind.Boss) return "The path to the exit crosses a non-combat room.";
                pathLength++;
            }

            return pathLength == config.MainPathRooms ? null : "The main path has the wrong length.";
        }
    }
}
