using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VorgGates.Core;

namespace VorgGates.Gameplay
{
    public sealed class BuiltDungeon
    {
        public GameObject Root;
        public RoomController[] Rooms;
        public PrototypePortal Portal;
        public LegendarySword Sword;
        public WeaponPickup Reward;
        public Transform PlayerStart;
        public Bounds Bounds;
    }

    /// <summary>Turns a generated layout into prefab instances of the phase's kit.</summary>
    public sealed class DungeonBuilder : MonoBehaviour
    {
        public const float RoomPitch = 24f;
        private const float RoomExtent = 11f;

        [SerializeField] private WeaponPickup rewardPrefab;
        [SerializeField] private HealOrb healOrbPrefab;
        [SerializeField] private LegendarySword swordPrefab;

        public static Vector3 ToWorld(GridPos position) => new Vector3(position.X * RoomPitch, 0f, position.Y * RoomPitch);

        public BuiltDungeon Build(DungeonDefinition definition, DungeonLayout layout, PlayerStateMachine player, ProjectileSystem projectiles)
        {
            var root = new GameObject($"Dungeon_{definition.name}_{layout.Seed}");
            var built = new BuiltDungeon { Root = root, Rooms = new RoomController[layout.Rooms.Count] };
            var bounds = new Bounds(ToWorld(layout.Rooms[0].Position), Vector3.zero);

            foreach (var node in layout.Rooms)
            {
                built.Rooms[node.Id] = BuildRoom(definition, node, root.transform, built, player, projectiles);
                var center = ToWorld(node.Position);
                bounds.Encapsulate(center + new Vector3(RoomExtent, 4f, RoomExtent));
                bounds.Encapsulate(center - new Vector3(RoomExtent, 2f, RoomExtent));
            }

            foreach (var link in layout.Links)
            {
                var from = ToWorld(layout.Rooms[link.From].Position);
                var to = ToWorld(layout.Rooms[link.To].Position);
                var yaw = link.Side == Side.North || link.Side == Side.South ? 0f : 90f;
                Instantiate(definition.Kit.CorridorPrefab, (from + to) * 0.5f, Quaternion.Euler(0f, yaw, 0f), root.transform);
            }

            built.Bounds = bounds;
            return built;
        }

        private RoomController BuildRoom(DungeonDefinition definition, RoomNode node, Transform parent, BuiltDungeon built, PlayerStateMachine player, ProjectileSystem projectiles)
        {
            var kit = definition.Kit;
            var view = Instantiate(kit.RoomPrefab, ToWorld(node.Position), Quaternion.identity, parent);
            view.name = $"Room_{node.Id}_{node.Kind}";
            view.ApplyDoors(node.DoorMask);

            var interior = Instantiate(SelectInterior(kit, node), view.transform);
            var controller = view.GetComponent<RoomController>();

            if (node.Kind == RoomKind.Combat || node.Kind == RoomKind.Boss)
            {
                var doors = new List<DoorController>();
                var sides = new List<Side>();
                for (var i = 0; i < SideUtil.Count; i++)
                {
                    var side = (Side)i;
                    if (!node.HasDoor(side)) continue;
                    var prefab = side == Side.North || side == Side.East ? kit.TallDoor : kit.LowDoor;
                    var anchor = view.DoorAnchor(side);
                    doors.Add(Instantiate(prefab, anchor.position, anchor.rotation, view.transform));
                    sides.Add(side);
                }

                var encounter = node.Encounter.Select(kind => kit.EnemyPrefabs[(int)kind]).ToArray();
                controller.Configure(player.transform, player.Health, encounter, interior.SpawnPoints, doors.ToArray(), sides.ToArray(), projectiles, healOrbPrefab, definition.HealthScale, definition.DamageScale);
            }
            else
            {
                controller.ConfigureSafe();
            }

            switch (node.Kind)
            {
                case RoomKind.Entrance:
                    built.PlayerStart = interior.Anchor;
                    break;
                case RoomKind.Portal:
                    built.Portal = Instantiate(kit.PortalPrefab, interior.Anchor.position, interior.Anchor.rotation, view.transform);
                    break;
                case RoomKind.SwordChamber:
                    built.Sword = Instantiate(swordPrefab, interior.Anchor.position, interior.Anchor.rotation, view.transform);
                    break;
                case RoomKind.Reward:
                    built.Reward = Instantiate(rewardPrefab, interior.Anchor.position, interior.Anchor.rotation, view.transform);
                    built.Reward.Configure(player, definition.RewardWeapon);
                    break;
            }

            return controller;
        }

        private static RoomInterior SelectInterior(DungeonKit kit, RoomNode node)
        {
            switch (node.Kind)
            {
                case RoomKind.Entrance: return kit.EntranceInterior;
                case RoomKind.Portal:
                case RoomKind.SwordChamber: return kit.ExitInterior;
                case RoomKind.Reward: return kit.RewardInterior;
                case RoomKind.Boss: return kit.BossInterior;
                default: return kit.CombatInteriors[node.InteriorVariant];
            }
        }
    }
}
