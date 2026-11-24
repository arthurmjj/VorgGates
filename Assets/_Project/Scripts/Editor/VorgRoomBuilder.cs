using UnityEditor;
using UnityEngine;
using VorgGates.Core;
using VorgGates.Gameplay;

namespace VorgGates.EditorTools
{
    internal sealed class RoomKit
    {
        public RoomView Room;
        public GameObject Corridor;
        public RoomInterior Entrance;
        public RoomInterior Exit;
        public RoomInterior Reward;
        public RoomInterior Boss;
        public RoomInterior[] Combat;
    }

    /// <summary>
    /// Builds the modular room kit. North and east walls are tall, south and west are low. Next to a
    /// doorway the tall wall steps down, so it can never cover the player inside a corridor.
    /// </summary>
    internal static class VorgRoomBuilder
    {
        private const string Folder = "Assets/_Project/Prefabs/Rooms";
        private const float HalfRoom = 9f;
        private const float Edge = 9.5f;
        private const float WallEnd = 10f;
        private const float DoorHalf = 2.5f;
        private const float StepDown = 6f;
        private const float TallHeight = 3.3f;
        private const float LowHeight = 0.8f;
        private const float BlockerHeight = 6f;

        private static readonly Vector2[] SpawnLayout =
        {
            new Vector2(-6.5f, -1f), new Vector2(6.5f, 1f), new Vector2(-1f, 6.5f),
            new Vector2(1f, -6.5f), new Vector2(-2.4f, 2.4f), new Vector2(2.4f, -2.4f)
        };

        public static RoomKit Build(VorgArt art, GameObject torch)
        {
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Rooms");
            var kit = new RoomKit { Room = BuildShell(art, torch), Corridor = BuildCorridor(art) };
            var start = new Vector3(0f, 0.05f, 0f);
            var portalSpot = new Vector3(5f, 0f, 5f);
            var bossSpawn = new[] { new Vector2(0f, 1.5f) };

            switch (art.Theme)
            {
                case VorgTheme.Forge:
                    kit.Entrance = BuildInterior("Interior_Entrance", art, 111, VorgThemeDecor.ForgeEntrance, start, 45f);
                    kit.Exit = BuildInterior("Interior_Exit", art, 123, VorgThemeDecor.ForgeExit, portalSpot, 0f);
                    kit.Reward = BuildInterior("Interior_Reward", art, 137, VorgThemeDecor.ForgeReward, Vector3.zero, 0f);
                    kit.Boss = BuildInterior("Interior_Boss", art, 149, VorgThemeDecor.ForgeBoss, Vector3.zero, 0f, bossSpawn);
                    kit.Combat = new[]
                    {
                        BuildInterior("Interior_Foundry", art, 141, VorgThemeDecor.ForgeFoundry, Vector3.zero, 0f),
                        BuildInterior("Interior_Anvils", art, 153, VorgThemeDecor.ForgeAnvils, Vector3.zero, 0f),
                        BuildInterior("Interior_Channels", art, 167, VorgThemeDecor.ForgeChannels, Vector3.zero, 0f)
                    };
                    break;
                case VorgTheme.Sanctuary:
                    kit.Entrance = BuildInterior("Interior_Entrance", art, 211, VorgThemeDecor.SanctuaryEntrance, start, 45f);
                    kit.Exit = BuildInterior("Interior_Exit", art, 223, VorgThemeDecor.SanctuaryExit, Vector3.zero, 0f);
                    kit.Reward = BuildInterior("Interior_Reward", art, 237, VorgThemeDecor.SanctuaryReward, Vector3.zero, 0f);
                    kit.Boss = BuildInterior("Interior_Boss", art, 249, VorgThemeDecor.SanctuaryBoss, Vector3.zero, 0f, bossSpawn);
                    kit.Combat = new[]
                    {
                        BuildInterior("Interior_Altars", art, 241, VorgThemeDecor.SanctuaryAltars, Vector3.zero, 0f),
                        BuildInterior("Interior_Obelisks", art, 253, VorgThemeDecor.SanctuaryObelisks, Vector3.zero, 0f),
                        BuildInterior("Interior_Ruins", art, 267, VorgThemeDecor.SanctuaryRuins, Vector3.zero, 0f)
                    };
                    break;
                default:
                    kit.Entrance = BuildInterior("Interior_Entrance", art, 11, BuildEntranceDecor, start, 45f);
                    kit.Exit = BuildInterior("Interior_Portal", art, 23, VorgThemeDecor.CryptExit, portalSpot, 0f);
                    kit.Reward = BuildInterior("Interior_Reward", art, 37, BuildRewardDecor, Vector3.zero, 0f);
                    kit.Boss = BuildInterior("Interior_Boss", art, 49, VorgThemeDecor.CryptBoss, Vector3.zero, 0f, bossSpawn);
                    kit.Combat = new[]
                    {
                        BuildInterior("Interior_Arena", art, 41, BuildArenaDecor, Vector3.zero, 0f),
                        BuildInterior("Interior_Columns", art, 53, BuildColumnsDecor, Vector3.zero, 0f),
                        BuildInterior("Interior_Crypt", art, 67, BuildCryptDecor, Vector3.zero, 0f)
                    };
                    break;
            }

            return kit;
        }

        private static RoomView BuildShell(VorgArt art, GameObject torch)
        {
            var random = new System.Random(4127);
            var root = new GameObject("Room_Shell");

            var floor = Kit.Group(root.transform, "Floor");
            Kit.Box(floor, "Base", new Vector3(0f, -0.2f, 0f), new Vector3(20f, 0.3f, 20f), art.Grout);
            VorgArchitecture.RoomFloor(floor, art);
            Kit.Combine(floor, $"RoomFloor{art.Suffix}");
            Kit.Blocker(root.transform, "FloorCollider", Vector3.down * 0.25f, new Vector3(21f, 0.5f, 21f));

            var corners = Kit.Group(root.transform, "Corners");
            Pillar(corners, new Vector3(Edge, 0f, Edge), TallHeight + 0.5f, art);
            Pillar(corners, new Vector3(-Edge, 0f, Edge), TallHeight + 0.5f, art);
            Pillar(corners, new Vector3(Edge, 0f, -Edge), TallHeight + 0.5f, art);
            Pillar(corners, new Vector3(-Edge, 0f, -Edge), LowHeight + 0.45f, art);
            Kit.Combine(corners, "RoomCorners");

            var closed = new Object[SideUtil.Count];
            var open = new Object[SideUtil.Count];
            var anchors = new Object[SideUtil.Count];
            for (var i = 0; i < SideUtil.Count; i++)
            {
                var side = (Side)i;
                var group = Kit.Group(root.transform, $"Side_{side}");
                closed[i] = BuildClosedSide(group, side, art, torch, random);
                open[i] = BuildOpenSide(group, side, art, torch, random);
                var yaw = side == Side.North || side == Side.South ? 0f : 90f;
                anchors[i] = Kit.Group(group, "DoorAnchor", SidePoint(side, 0f, 0f, Edge), new Vector3(0f, yaw, 0f));
            }

            var view = root.AddComponent<RoomView>();
            Kit.Wire(view, ("closedSides", closed), ("openSides", open), ("doorAnchors", anchors));
            root.AddComponent<RoomController>();
            Kit.SetLayer(root, VorgProjectSetup.WorldLayer);
            return Kit.SaveAsPrefab<RoomView>(root, $"{Folder}/Room_Shell{art.Suffix}.prefab");
        }

        private static GameObject BuildClosedSide(Transform parent, Side side, VorgArt art, GameObject torch, System.Random random)
        {
            var group = Kit.Group(parent, "Closed");
            var visual = Kit.Group(group, "Visual");
            var tall = IsTallSide(side);
            WallRun(visual, SidePoint(side, -WallEnd, 0f, Edge), SidePoint(side, WallEnd, 0f, Edge), tall ? TallHeight : LowHeight, art, random);
            if (tall)
            {
                VorgThemeDecor.WallDressing(visual, SidePoint(side, 0f, 2.95f, HalfRoom - 0.04f), BannerYaw(side), art);
                VorgDressing.WallFeatures(visual, side, -WallEnd, WallEnd, art, random);
            }

            Kit.Combine(visual, $"Wall_{side}_Closed");
            SideBlocker(group, side, 0f, WallEnd * 2f + 1f);
            if (tall)
            {
                Torch(group, torch, side, -5.5f);
                Torch(group, torch, side, 5.5f);
            }

            return group.gameObject;
        }

        private static GameObject BuildOpenSide(Transform parent, Side side, VorgArt art, GameObject torch, System.Random random)
        {
            var group = Kit.Group(parent, "Open");
            var visual = Kit.Group(group, "Visual");
            var tall = IsTallSide(side);
            if (tall)
            {
                WallRun(visual, SidePoint(side, -WallEnd, 0f, Edge), SidePoint(side, -StepDown, 0f, Edge), TallHeight, art, random);
                WallRun(visual, SidePoint(side, -StepDown, 0f, Edge), SidePoint(side, -DoorHalf, 0f, Edge), LowHeight, art, random);
                WallRun(visual, SidePoint(side, DoorHalf, 0f, Edge), SidePoint(side, WallEnd, 0f, Edge), TallHeight, art, random);
                Pillar(visual, SidePoint(side, -DoorHalf, 0f, Edge), LowHeight + 0.45f, art);
                Pillar(visual, SidePoint(side, DoorHalf, 0f, Edge), TallHeight + 0.5f, art);
                VorgThemeDecor.WallDressing(visual, SidePoint(side, 7.9f, 2.95f, HalfRoom - 0.04f), BannerYaw(side), art);
                VorgDressing.WallFeatures(visual, side, DoorHalf, WallEnd, art, random);
            }
            else
            {
                WallRun(visual, SidePoint(side, -WallEnd, 0f, Edge), SidePoint(side, -DoorHalf, 0f, Edge), LowHeight, art, random);
                WallRun(visual, SidePoint(side, DoorHalf, 0f, Edge), SidePoint(side, WallEnd, 0f, Edge), LowHeight, art, random);
                Pillar(visual, SidePoint(side, -DoorHalf, 0f, Edge), LowHeight + 0.45f, art);
                Pillar(visual, SidePoint(side, DoorHalf, 0f, Edge), LowHeight + 0.45f, art);
            }

            Kit.Combine(visual, $"Wall_{side}_Open");
            const float length = 8.5f;
            SideBlocker(group, side, -6.25f, length);
            SideBlocker(group, side, 6.25f, length);
            if (tall)
            {
                Torch(group, torch, side, -8f);
                Torch(group, torch, side, 5.4f);
            }

            return group.gameObject;
        }

        private static GameObject BuildCorridor(VorgArt art)
        {
            var random = new System.Random(991);
            var root = new GameObject("Corridor");
            var visual = Kit.Group(root.transform, "Visual");
            Kit.Box(visual, "Base", new Vector3(0f, -0.2f, 0f), new Vector3(6f, 0.3f, 6f), art.Grout);
            VorgArchitecture.CorridorFloor(visual, art);

            foreach (var x in new[] { -DoorHalf, DoorHalf })
                WallRun(visual, new Vector3(x, 0f, -2f), new Vector3(x, 0f, 2f), LowHeight, art, random);
            if (art.Theme == VorgTheme.Forge)
            {
                foreach (var x in new[] { -DoorHalf, DoorHalf }) VorgThemeDecor.Chain(visual, new Vector3(x, 0.72f, 0f), 3, art);
            }

            Kit.Combine(visual, $"Corridor{art.Suffix}");

            Kit.Blocker(root.transform, "Floor", Vector3.down * 0.25f, new Vector3(6f, 0.5f, 7f));
            foreach (var x in new[] { -DoorHalf, DoorHalf })
                Kit.Blocker(root.transform, "Wall", new Vector3(x, BlockerHeight * 0.5f, 0f), new Vector3(1f, BlockerHeight, 7f));

            Kit.SetLayer(root, VorgProjectSetup.WorldLayer);
            return Kit.SaveAsPrefab<GameObject>(root, $"{Folder}/Corridor{art.Suffix}.prefab");
        }

        private static RoomInterior BuildInterior(string baseName, VorgArt art, int seed, System.Action<Transform, VorgArt, System.Random> decorate, Vector3 anchorPosition, float anchorYaw, Vector2[] spawnLayout = null)
        {
            var name = $"{baseName}{art.Suffix}";
            var layout = spawnLayout ?? SpawnLayout;
            var random = new System.Random(seed);
            var root = new GameObject(name);
            var decor = Kit.Group(root.transform, "Decor");
            decorate(decor, art, random);
            Moss(decor, art, random, 12);
            Rubble(decor, art, random, 10);
            VorgDressing.InteriorProps(decor, art, random, name);
            Kit.Combine(decor, name);
            VorgDressing.InteriorLights(root.transform, art, name);

            var spawns = Kit.Group(root.transform, "SpawnPoints");
            var points = new Object[layout.Length];
            for (var i = 0; i < points.Length; i++)
                points[i] = Kit.Group(spawns, $"Spawn{i}", new Vector3(layout[i].x, 0f, layout[i].y), new Vector3(0f, 180f, 0f));
            var anchor = Kit.Group(root.transform, "Anchor", anchorPosition, new Vector3(0f, anchorYaw, 0f));

            var interior = root.AddComponent<RoomInterior>();
            Kit.Wire(interior, ("spawnPoints", points), ("anchor", anchor));
            Kit.SetLayer(root, VorgProjectSetup.WorldLayer);
            return Kit.SaveAsPrefab<RoomInterior>(root, $"{Folder}/{name}.prefab");
        }

        private static void BuildEntranceDecor(Transform decor, VorgArt art, System.Random random)
        {
            Kit.Box(decor, "RugBorder", Vector3.up * 0.012f, new Vector3(4.4f, 0.014f, 7.4f), art.Gold);
            Kit.Box(decor, "Rug", Vector3.up * 0.022f, new Vector3(4f, 0.014f, 7f), art.TerracottaDark);
            Kit.Box(decor, "RugInner", Vector3.up * 0.032f, new Vector3(2.7f, 0.014f, 5.7f), art.Terracotta);
            foreach (var x in new[] { -6.5f, 6.5f })
            foreach (var z in new[] { -6.5f, 6.5f })
                Brazier(decor, new Vector3(x, 0f, z), art);
        }

        internal static void PortalDecor(Transform decor, VorgArt art, System.Random random)
        {
            RuneCircle(decor, new Vector3(5f, 0f, 5f), 3.4f, art);
            ShortColumn(decor, new Vector3(-4.6f, 0f, 4.6f), art);
            ShortColumn(decor, new Vector3(4.6f, 0f, -4.6f), art);
            VorgPropBuilder.Crate(decor, new Vector3(-7.6f, 0f, -7.4f), 18f, 1f, art);
            VorgPropBuilder.Barrel(decor, new Vector3(-6.4f, 0f, -7.7f), art);
        }

        private static void BuildRewardDecor(Transform decor, VorgArt art, System.Random random)
        {
            RewardPedestal(decor, art);
            foreach (var x in new[] { -6.5f, 6.5f }) Brazier(decor, new Vector3(x, 0f, 6.5f), art);
        }

        internal static void RewardPedestal(Transform decor, VorgArt art)
        {
            RuneCircle(decor, Vector3.zero, 3f, art);
            Kit.Box(decor, "Pedestal", Vector3.up * 0.25f, new Vector3(2.5f, 0.5f, 1.9f), art.Trim);
            Kit.Box(decor, "PedestalTop", Vector3.up * 0.55f, new Vector3(2.1f, 0.12f, 1.5f), art.WallC);
            VorgExternalAssets.Place(decor, "Dungeon/chest", Vector3.up * 0.61f, 180f, 2.45f, art);
            foreach (var x in new[] { -0.95f, 0.95f })
                Kit.Cylinder(decor, "PedestalCandle", new Vector3(x, 0.72f, -0.48f), new Vector3(0.1f, 0.12f, 0.1f), art.Parchment);
            Kit.Blocker(decor, "PedestalCollider", Vector3.up * 0.75f, new Vector3(2.5f, 1.5f, 1.9f));
        }

        private static void BuildArenaDecor(Transform decor, VorgArt art, System.Random random)
        {
            RuneCircle(decor, Vector3.zero, 6.4f, art);
            VorgPropBuilder.Barrel(decor, new Vector3(7.7f, 0f, 7.5f), art);
            VorgPropBuilder.Barrel(decor, new Vector3(6.6f, 0f, 7.9f), art);
            VorgPropBuilder.Crate(decor, new Vector3(7.8f, 0f, 6.1f), 12f, 0.95f, art);
            VorgPropBuilder.Crate(decor, new Vector3(-7.7f, 0f, 7.5f), -18f, 1.15f, art);
            VorgPropBuilder.Crate(decor, new Vector3(7.7f, 0f, -7.6f), 28f, 1.05f, art);
            VorgPropBuilder.Barrel(decor, new Vector3(-7.7f, 0f, -7.6f), art);
        }

        private static void BuildColumnsDecor(Transform decor, VorgArt art, System.Random random)
        {
            foreach (var x in new[] { -4.6f, 4.6f })
            foreach (var z in new[] { -4.6f, 4.6f })
                ShortColumn(decor, new Vector3(x, 0f, z), art);
            Kit.Cylinder(decor, "InlayOuter", Vector3.up * 0.012f, new Vector3(3.2f, 0.012f, 3.2f), art.Trim);
            Kit.Cylinder(decor, "Inlay", Vector3.up * 0.02f, new Vector3(2.6f, 0.012f, 2.6f), art.TerracottaDark);
            VorgPropBuilder.Barrel(decor, new Vector3(7.8f, 0f, 7.7f), art);
            VorgPropBuilder.Crate(decor, new Vector3(-7.7f, 0f, -7.6f), 22f, 1f, art);
        }

        private static void BuildCryptDecor(Transform decor, VorgArt art, System.Random random)
        {
            Sarcophagus(decor, new Vector3(-5.6f, 0f, 5.2f), 0f, art);
            Sarcophagus(decor, new Vector3(5.6f, 0f, 5.2f), 0f, art);
            Sarcophagus(decor, new Vector3(-5.6f, 0f, -5.2f), 0f, art);
            Sarcophagus(decor, new Vector3(5.6f, 0f, -5.2f), 0f, art);
            Kit.Box(decor, "AisleX", Vector3.up * 0.012f, new Vector3(13f, 0.014f, 1.3f), art.Trim);
            Kit.Box(decor, "AisleZ", Vector3.up * 0.014f, new Vector3(1.3f, 0.014f, 13f), art.Trim);
        }

        private static void Sarcophagus(Transform parent, Vector3 position, float yaw, VorgArt art)
        {
            var group = Kit.Group(parent, "Sarcophagus", position, new Vector3(0f, yaw, 0f));
            VorgArchitecture.Sarcophagus(group, art);
            Kit.Blocker(group, "Collider", Vector3.up * 0.5f, new Vector3(3.2f, 1f, 1.75f));
        }

        private static void ShortColumn(Transform parent, Vector3 position, VorgArt art)
        {
            var group = Kit.Group(parent, "Column", position);
            VorgArchitecture.ShortColumn(group, art);
            Kit.Blocker(group, "Collider", Vector3.up * 0.75f, new Vector3(1.3f, 1.5f, 1.3f));
        }

        internal static void Brazier(Transform parent, Vector3 position, VorgArt art)
        {
            var group = Kit.Group(parent, "Brazier", position);
            Kit.Cylinder(group, "Stand", Vector3.up * 0.35f, new Vector3(0.18f, 0.35f, 0.18f), art.Steel);
            Kit.Cylinder(group, "Foot", Vector3.up * 0.04f, new Vector3(0.62f, 0.04f, 0.62f), art.Steel);
            Kit.Cylinder(group, "Bowl", Vector3.up * 0.74f, new Vector3(0.78f, 0.09f, 0.78f), art.Steel);
            Kit.Ball(group, "Coals", Vector3.up * 0.84f, new Vector3(0.58f, 0.22f, 0.58f), art.Ember);
            Kit.Blocker(group, "Collider", Vector3.up * 0.5f, new Vector3(0.8f, 1f, 0.8f));
        }

        internal static void RuneCircle(Transform parent, Vector3 center, float diameter, VorgArt art)
        {
            Kit.Cylinder(parent, "RuneOuter", center + Vector3.up * 0.01f, new Vector3(diameter, 0.012f, diameter), art.Trim);
            Kit.Cylinder(parent, "RuneInner", center + Vector3.up * 0.018f, new Vector3(diameter * 0.89f, 0.012f, diameter * 0.89f), art.FloorB);
            for (var i = 0; i < 8; i++)
            {
                var radians = i * Mathf.PI / 4f;
                var offset = new Vector3(Mathf.Sin(radians), 0f, Mathf.Cos(radians)) * (diameter * 0.35f);
                Kit.Box(parent, $"Rune{i}", center + offset + Vector3.up * 0.032f, new Vector3(0.16f, 0.012f, diameter * 0.097f), art.RuneGold, new Vector3(0f, i * 45f, 0f));
            }

            Kit.Box(parent, "SigilA", center + Vector3.up * 0.032f, new Vector3(diameter * 0.33f, 0.012f, 0.14f), art.RuneGold, new Vector3(0f, 45f, 0f));
            Kit.Box(parent, "SigilB", center + Vector3.up * 0.032f, new Vector3(diameter * 0.33f, 0.012f, 0.14f), art.RuneGold, new Vector3(0f, -45f, 0f));
        }

        private static void Moss(Transform parent, VorgArt art, System.Random random, int count) =>
            VorgDressing.GroundCover(parent, art, random, count);

        private static void Rubble(Transform parent, VorgArt art, System.Random random, int count)
        {
            for (var i = 0; i < count; i++)
            {
                var along = -7.5f + (float)random.NextDouble() * 15f;
                if (Mathf.Abs(along) < 3f) along += along < 0f ? -3f : 3f;
                var position = random.Next(2) == 0 ? new Vector3(along, 0.08f, 8.45f) : new Vector3(8.45f, 0.08f, along);
                var size = 0.14f + (float)random.NextDouble() * 0.2f;
                Kit.Box(parent, "Rubble", position, Vector3.one * size, art.WallC, new Vector3((float)random.NextDouble() * 60f, (float)random.NextDouble() * 180f, (float)random.NextDouble() * 60f));
            }
        }

        private static bool IsTallSide(Side side) => side == Side.North || side == Side.East;

        private static float BannerYaw(Side side) => side == Side.North ? 0f : 90f;

        internal static Vector3 SidePoint(Side side, float along, float height, float distance)
        {
            switch (side)
            {
                case Side.North: return new Vector3(along, height, distance);
                case Side.East: return new Vector3(distance, height, along);
                case Side.South: return new Vector3(along, height, -distance);
                default: return new Vector3(-distance, height, along);
            }
        }

        private static void SideBlocker(Transform parent, Side side, float along, float length)
        {
            var isNorthSouth = side == Side.North || side == Side.South;
            var size = isNorthSouth ? new Vector3(length, BlockerHeight, 1f) : new Vector3(1f, BlockerHeight, length);
            Kit.Blocker(parent, "Blocker", SidePoint(side, along, BlockerHeight * 0.5f, Edge), size);
        }

        private static void Torch(Transform parent, GameObject prefab, Side side, float along)
        {
            var torch = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            torch.transform.localPosition = SidePoint(side, along, 2.15f, HalfRoom - 0.02f);
            torch.transform.localEulerAngles = new Vector3(0f, BannerYaw(side), 0f);
        }

        private static void WallRun(Transform parent, Vector3 start, Vector3 end, float height, VorgArt art, System.Random random) =>
            VorgArchitecture.Wall(parent, start, end, height, art);

        private static void Pillar(Transform parent, Vector3 position, float height, VorgArt art) =>
            VorgArchitecture.Pillar(parent, position, height, art);
    }
}
