using UnityEditor;
using UnityEngine;

namespace VorgGates.EditorTools
{
    /// <summary>Props and room dressing that give each phase its own silhouettes, beyond a color swap.</summary>
    internal static class VorgThemeDecor
    {
        private const string PrefabFolder = "Assets/_Project/Prefabs/Props";

        public static void WallDressing(Transform parent, Vector3 position, float yaw, VorgArt art)
        {
            switch (art.Theme)
            {
                case VorgTheme.Forge:
                    IronPanel(parent, position, yaw, art);
                    break;
                case VorgTheme.Sanctuary:
                    break;
                default:
                    VorgPropBuilder.Banner(parent, position, yaw, art);
                    break;
            }
        }

        private static void IronPanel(Transform parent, Vector3 position, float yaw, VorgArt art)
        {
            var panel = Kit.Group(parent, "IronPanel", position, new Vector3(0f, yaw, 0f));
            Kit.Box(panel, "Plate", new Vector3(0f, -0.92f, 0f), new Vector3(1.5f, 1.85f, 0.08f), art.Steel);
            Kit.Box(panel, "Vent", new Vector3(0f, -0.66f, -0.05f), new Vector3(1f, 0.56f, 0.04f), art.Abyss);
            for (var i = 0; i < 3; i++)
                Kit.Box(panel, $"Slat{i}", new Vector3(0f, -0.48f - i * 0.18f, -0.08f), new Vector3(1.04f, 0.06f, 0.03f), art.Ember);
            foreach (var x in new[] { -0.62f, 0.62f })
            foreach (var y in new[] { -0.12f, -1.72f })
                Kit.Ball(panel, "Rivet", new Vector3(x, y, -0.05f), Vector3.one * 0.12f, art.Trim);
            Chain(panel, new Vector3(-0.92f, 0.1f, -0.12f), 7, art);
            Chain(panel, new Vector3(0.92f, 0.1f, -0.12f), 5, art);
        }

        public static void Chain(Transform parent, Vector3 top, int links, VorgArt art)
        {
            for (var i = 0; i < links; i++)
            {
                var yaw = i % 2 == 0 ? 0f : 90f;
                Kit.Box(parent, "ChainLink", top + Vector3.down * (i * 0.17f), new Vector3(0.11f, 0.19f, 0.035f), art.Steel, new Vector3(0f, yaw, 0f));
            }
        }

        public static void EnemyAccessory(Transform body, float height, VorgArt art)
        {
            if (art.Theme == VorgTheme.Crypt) return;
            if (art.Theme == VorgTheme.Forge)
            {
                Kit.Box(body, "SlagPlate", new Vector3(0f, height * 0.62f, -0.24f), new Vector3(height * 0.34f, height * 0.2f, 0.1f), art.Steel, new Vector3(-12f, 0f, 0f));
                Kit.Box(body, "SlagCore", new Vector3(0f, height * 0.62f, -0.31f), new Vector3(height * 0.11f, height * 0.11f, 0.05f), art.Ember, new Vector3(0f, 0f, 45f));
                Chain(body, new Vector3(height * 0.2f, height * 0.56f, 0.12f), 3, art);
                return;
            }

            for (var i = 0; i < 3; i++)
            {
                var radians = i * Mathf.PI * 2f / 3f + 0.5f;
                Kit.Box(body, $"CursedShard{i}", new Vector3(Mathf.Sin(radians) * height * 0.24f, height * (0.86f + i * 0.04f), Mathf.Cos(radians) * height * 0.24f), new Vector3(0.11f, height * 0.17f, 0.11f), art.Ember, new Vector3(18f, i * 55f, 24f));
            }
        }

        public static void ForgeEntrance(Transform decor, VorgArt art, System.Random random)
        {
            Kit.Box(decor, "GrateFrame", Vector3.up * 0.012f, new Vector3(4.4f, 0.014f, 7.4f), art.Steel);
            Kit.Box(decor, "GrateGlow", Vector3.up * 0.022f, new Vector3(4f, 0.014f, 7f), art.Ember);
            for (var i = 0; i < 9; i++)
                Kit.Box(decor, $"GrateBar{i}", new Vector3(0f, 0.04f, -3f + i * 0.75f), new Vector3(4f, 0.03f, 0.3f), art.Abyss);
            foreach (var x in new[] { -6.2f, 6.2f })
            foreach (var z in new[] { -6.2f, 6.2f })
                Anvil(decor, new Vector3(x, 0f, z), 30f, art);
        }

        public static void ForgeFoundry(Transform decor, VorgArt art, System.Random random)
        {
            LavaChannel(decor, new Vector3(0f, 0f, 0f), 11f, 0f, art);
            LavaChannel(decor, new Vector3(0f, 0f, 0f), 11f, 90f, art);
            Crucible(decor, new Vector3(-5.8f, 0f, 5.6f), art);
            Crucible(decor, new Vector3(5.8f, 0f, -5.6f), art);
            Anvil(decor, new Vector3(5.8f, 0f, 5.6f), 20f, art);
            Anvil(decor, new Vector3(-5.8f, 0f, -5.6f), -35f, art);
        }

        public static void ForgeAnvils(Transform decor, VorgArt art, System.Random random)
        {
            foreach (var x in new[] { -4.8f, 4.8f })
            foreach (var z in new[] { -4.8f, 4.8f })
                Anvil(decor, new Vector3(x, 0f, z), x * z > 0f ? 25f : -25f, art);
            ScrapPile(decor, new Vector3(7.5f, 0f, 7.4f), art, random);
            ScrapPile(decor, new Vector3(-7.5f, 0f, -7.4f), art, random);
            Kit.Cylinder(decor, "HeatRing", Vector3.up * 0.012f, new Vector3(4f, 0.012f, 4f), art.Steel);
            Kit.Cylinder(decor, "HeatCore", Vector3.up * 0.02f, new Vector3(3.3f, 0.012f, 3.3f), art.Ember);
        }

        public static void ForgeChannels(Transform decor, VorgArt art, System.Random random)
        {
            foreach (var offset in new[] { -3.6f, 3.6f })
            {
                LavaChannel(decor, new Vector3(offset, 0f, 0f), 15f, 0f, art);
                LavaChannel(decor, new Vector3(0f, 0f, offset), 15f, 90f, art);
            }

            Crucible(decor, new Vector3(6.2f, 0f, 6.2f), art);
            Crucible(decor, new Vector3(-6.2f, 0f, -6.2f), art);
            ScrapPile(decor, new Vector3(-6.4f, 0f, 6.4f), art, random);
            ScrapPile(decor, new Vector3(6.4f, 0f, -6.4f), art, random);
        }

        public static void ForgeBoss(Transform decor, VorgArt art, System.Random random)
        {
            Kit.Cylinder(decor, "PitRim", Vector3.up * 0.012f, new Vector3(11.5f, 0.012f, 11.5f), art.Steel);
            Kit.Cylinder(decor, "PitFloor", Vector3.up * 0.02f, new Vector3(10.7f, 0.012f, 10.7f), art.FloorB);
            for (var i = 0; i < 12; i++)
            {
                var radians = i * Mathf.PI / 6f;
                Kit.Box(decor, $"PitGlow{i}", new Vector3(Mathf.Sin(radians) * 5f, 0.032f, Mathf.Cos(radians) * 5f), new Vector3(0.22f, 0.012f, 1.2f), art.Ember, new Vector3(0f, i * 30f, 0f));
            }

            foreach (var x in new[] { -7.6f, 7.6f })
            foreach (var z in new[] { -7.6f, 7.6f })
                Crucible(decor, new Vector3(x, 0f, z), art);
        }

        public static void ForgeExit(Transform decor, VorgArt art, System.Random random)
        {
            Kit.Cylinder(decor, "PortalPad", new Vector3(5f, 0.012f, 5f), new Vector3(5.6f, 0.012f, 5.6f), art.Ember);
            Anvil(decor, new Vector3(-5.6f, 0f, 5f), 15f, art);
            Crucible(decor, new Vector3(5.6f, 0f, -5.4f), art);
            ScrapPile(decor, new Vector3(-7f, 0f, -7f), art, random);
        }

        public static void ForgeReward(Transform decor, VorgArt art, System.Random random)
        {
            VorgRoomBuilder.RewardPedestal(decor, art);
            foreach (var x in new[] { -6.2f, 6.2f }) Crucible(decor, new Vector3(x, 0f, 6.2f), art);
            LavaChannel(decor, new Vector3(0f, 0f, -5.5f), 9f, 90f, art);
        }

        public static void SanctuaryEntrance(Transform decor, VorgArt art, System.Random random)
        {
            Kit.Box(decor, "PathEdge", Vector3.up * 0.012f, new Vector3(4.4f, 0.014f, 7.4f), art.Trim);
            Kit.Box(decor, "Path", Vector3.up * 0.022f, new Vector3(4f, 0.014f, 7f), art.TerracottaDark);
            for (var i = 0; i < 4; i++) Kit.Box(decor, $"PathGlyph{i}", new Vector3(0f, 0.034f, -2.5f + i * 1.65f), Vector3.one * 0.5f + Vector3.down * 0.48f, art.RuneGold, new Vector3(0f, 45f, 0f));
            foreach (var x in new[] { -6.3f, 6.3f })
            foreach (var z in new[] { -6.3f, 6.3f })
                Obelisk(decor, new Vector3(x, 0f, z), art);
        }

        public static void SanctuaryAltars(Transform decor, VorgArt art, System.Random random)
        {
            Altar(decor, new Vector3(-5.6f, 0f, 5.4f), art);
            Altar(decor, new Vector3(5.6f, 0f, -5.4f), art);
            CrystalCluster(decor, new Vector3(5.8f, 0f, 5.8f), art);
            CrystalCluster(decor, new Vector3(-5.8f, 0f, -5.8f), art);
            VorgRoomBuilder.RuneCircle(decor, Vector3.zero, 5.2f, art);
        }

        public static void SanctuaryObelisks(Transform decor, VorgArt art, System.Random random)
        {
            foreach (var x in new[] { -4.7f, 4.7f })
            foreach (var z in new[] { -4.7f, 4.7f })
                Obelisk(decor, new Vector3(x, 0f, z), art);
            Kit.Box(decor, "CrossA", Vector3.up * 0.014f, new Vector3(12f, 0.012f, 0.32f), art.RuneGold, new Vector3(0f, 45f, 0f));
            Kit.Box(decor, "CrossB", Vector3.up * 0.016f, new Vector3(12f, 0.012f, 0.32f), art.RuneGold, new Vector3(0f, -45f, 0f));
        }

        public static void SanctuaryRuins(Transform decor, VorgArt art, System.Random random)
        {
            BrokenArch(decor, new Vector3(-5.5f, 0f, 5.6f), 45f, art);
            BrokenArch(decor, new Vector3(5.5f, 0f, -5.6f), 45f, art);
            CrystalCluster(decor, new Vector3(-6.2f, 0f, -6.2f), art);
            CrystalCluster(decor, new Vector3(6.2f, 0f, 6.2f), art);
            for (var i = 0; i < 12; i++)
            {
                var position = new Vector3((float)random.NextDouble() * 15f - 7.5f, 0.012f, (float)random.NextDouble() * 15f - 7.5f);
                Kit.Box(decor, "FloorCrack", position, new Vector3(0.08f, 0.012f, 1f + (float)random.NextDouble() * 2f), art.Ember, new Vector3(0f, (float)random.NextDouble() * 180f, 0f));
            }
        }

        public static void SanctuaryBoss(Transform decor, VorgArt art, System.Random random)
        {
            VorgRoomBuilder.RuneCircle(decor, Vector3.zero, 13f, art);
            for (var i = 0; i < 8; i++)
            {
                var radians = i * Mathf.PI / 4f + Mathf.PI / 8f;
                CrystalCluster(decor, new Vector3(Mathf.Sin(radians) * 8.1f, 0f, Mathf.Cos(radians) * 8.1f), art, false);
            }
        }

        public static void SanctuaryExit(Transform decor, VorgArt art, System.Random random)
        {
            VorgRoomBuilder.RuneCircle(decor, Vector3.zero, 10.5f, art);
            Kit.Cylinder(decor, "DaisLow", Vector3.up * 0.045f, new Vector3(6.2f, 0.045f, 6.2f), art.Trim);
            Kit.Cylinder(decor, "DaisHigh", Vector3.up * 0.11f, new Vector3(4.7f, 0.05f, 4.7f), art.WallC);
            Kit.Box(decor, "CarpetBorder", new Vector3(0f, 0.018f, -5.7f), new Vector3(2.5f, 0.018f, 6.2f), art.Gold);
            Kit.Box(decor, "Carpet", new Vector3(0f, 0.03f, -5.7f), new Vector3(2.15f, 0.018f, 6.2f), art.TerracottaDark);
            for (var i = 0; i < 16; i++)
            {
                var radians = i * Mathf.PI / 8f;
                Kit.Box(decor, "DaisRune", new Vector3(Mathf.Sin(radians) * 2.74f, 0.17f, Mathf.Cos(radians) * 2.74f), new Vector3(0.12f, 0.018f, 0.44f), art.RuneGold, new Vector3(0f, i * 22.5f, 0f));
            }

            foreach (var x in new[] { -6.4f, 6.4f })
            foreach (var z in new[] { -6.4f, 6.4f })
                Obelisk(decor, new Vector3(x, 0f, z), art);

            VorgDressing.Statue(decor, new Vector3(-3.6f, 0f, 8.55f), 180f, art, true);
            VorgDressing.Statue(decor, new Vector3(3.6f, 0f, 8.55f), 180f, art, true);
            VorgDressing.Statue(decor, new Vector3(8.55f, 0f, -3.6f), -90f, art, true);
            VorgDressing.Statue(decor, new Vector3(8.55f, 0f, 3.6f), -90f, art, true);
            foreach (var spot in new[] { new Vector2(-3.5f, -2.9f), new Vector2(3.5f, -2.9f), new Vector2(-3.5f, 2.9f), new Vector2(3.5f, 2.9f) })
            {
                VorgExternalAssets.Place(decor, "Graveyard/candle-multiple", new Vector3(spot.x, 0f, spot.y), spot.x * 40f, 1.9f, art);
                Kit.Ball(decor, "CandleGlow", new Vector3(spot.x, 0.46f, spot.y), new Vector3(0.1f, 0.18f, 0.1f), art.Flame);
            }
        }

        public static void SanctuaryReward(Transform decor, VorgArt art, System.Random random)
        {
            VorgRoomBuilder.RewardPedestal(decor, art);
            foreach (var x in new[] { -6.2f, 6.2f }) CrystalCluster(decor, new Vector3(x, 0f, 6.2f), art);
            Altar(decor, new Vector3(-5.8f, 0f, -5.4f), art);
        }

        public static void CryptBoss(Transform decor, VorgArt art, System.Random random)
        {
            VorgRoomBuilder.RuneCircle(decor, Vector3.zero, 12f, art);
            foreach (var x in new[] { -7.7f, 7.7f })
            foreach (var z in new[] { -7.7f, 7.7f })
                VorgRoomBuilder.Brazier(decor, new Vector3(x, 0f, z), art);
        }

        public static void CryptExit(Transform decor, VorgArt art, System.Random random) => VorgRoomBuilder.PortalDecor(decor, art, random);

        private static void Anvil(Transform parent, Vector3 position, float yaw, VorgArt art)
        {
            var anvil = Kit.Group(parent, "Anvil", position, new Vector3(0f, yaw, 0f));
            Kit.Box(anvil, "Stump", Vector3.up * 0.3f, new Vector3(0.92f, 0.6f, 0.92f), art.WoodDark);
            Kit.Box(anvil, "Waist", Vector3.up * 0.72f, new Vector3(0.42f, 0.26f, 0.5f), art.Steel);
            Kit.Box(anvil, "Top", Vector3.up * 0.94f, new Vector3(0.62f, 0.2f, 1.25f), art.Steel);
            Kit.Box(anvil, "Horn", new Vector3(0f, 0.95f, 0.78f), new Vector3(0.3f, 0.14f, 0.44f), art.Steel, new Vector3(0f, 45f, 0f));
            Kit.Box(anvil, "HotBar", new Vector3(0.05f, 1.07f, -0.1f), new Vector3(0.12f, 0.08f, 0.7f), art.Ember, new Vector3(0f, 14f, 0f));
            Kit.Blocker(anvil, "Collider", Vector3.up * 0.55f, new Vector3(0.95f, 1.1f, 1.25f));
        }

        private static void Crucible(Transform parent, Vector3 position, VorgArt art)
        {
            var crucible = Kit.Group(parent, "Crucible", position);
            Kit.Cylinder(crucible, "Foot", Vector3.up * 0.09f, new Vector3(1.55f, 0.09f, 1.55f), art.Trim);
            Kit.Cylinder(crucible, "Body", Vector3.up * 0.58f, new Vector3(1.3f, 0.42f, 1.3f), art.Steel);
            Kit.Cylinder(crucible, "Lip", Vector3.up * 1.02f, new Vector3(1.48f, 0.05f, 1.48f), art.Trim);
            Kit.Cylinder(crucible, "Molten", Vector3.up * 1.07f, new Vector3(1.14f, 0.015f, 1.14f), art.Lava);
            foreach (var side in new[] { -1f, 1f })
                Kit.Box(crucible, "Handle", new Vector3(side * 0.76f, 0.72f, 0f), new Vector3(0.2f, 0.14f, 0.42f), art.Abyss);
            Kit.Blocker(crucible, "Collider", Vector3.up * 0.55f, new Vector3(1.4f, 1.1f, 1.4f));
        }

        private static void LavaChannel(Transform parent, Vector3 center, float length, float yaw, VorgArt art)
        {
            var channel = Kit.Group(parent, "LavaChannel", center + Vector3.up * 0.01f, new Vector3(0f, yaw, 0f));
            Kit.Box(channel, "Rim", Vector3.zero, new Vector3(0.82f, 0.012f, length), art.Abyss);
            Kit.Box(channel, "Lava", Vector3.up * 0.008f, new Vector3(0.48f, 0.012f, length - 0.3f), art.Lava);
        }

        private static void ScrapPile(Transform parent, Vector3 position, VorgArt art, System.Random random)
        {
            var pile = Kit.Group(parent, "ScrapPile", position);
            for (var i = 0; i < 7; i++)
            {
                var offset = new Vector3((float)random.NextDouble() - 0.5f, 0.12f + i * 0.06f, (float)random.NextDouble() - 0.5f) * 1.25f;
                var size = new Vector3(0.28f + (float)random.NextDouble() * 0.4f, 0.09f, 0.18f + (float)random.NextDouble() * 0.5f);
                Kit.Box(pile, "Scrap", offset, size, i % 3 == 0 ? art.Trim : art.Steel, new Vector3((float)random.NextDouble() * 35f, (float)random.NextDouble() * 180f, (float)random.NextDouble() * 35f));
            }

            Kit.Blocker(pile, "Collider", Vector3.up * 0.35f, new Vector3(1.4f, 0.7f, 1.4f));
        }

        private static void Obelisk(Transform parent, Vector3 position, VorgArt art)
        {
            var obelisk = Kit.Group(parent, "Obelisk", position, new Vector3(0f, 18f, 0f));
            Kit.Box(obelisk, "Base", Vector3.up * 0.12f, new Vector3(1.36f, 0.24f, 1.36f), art.Trim);
            Kit.Box(obelisk, "Shaft", Vector3.up * 0.76f, new Vector3(0.86f, 1.05f, 0.86f), art.WallA);
            Kit.Box(obelisk, "Tip", new Vector3(0.06f, 1.38f, 0f), new Vector3(0.58f, 0.46f, 0.58f), art.WallC, new Vector3(35f, 45f, 0f));
            Kit.Box(obelisk, "Rune", new Vector3(0f, 0.8f, -0.44f), new Vector3(0.18f, 0.52f, 0.03f), art.Ember);
            Kit.Box(obelisk, "RuneSide", new Vector3(-0.44f, 0.8f, 0f), new Vector3(0.03f, 0.52f, 0.18f), art.Ember);
            Kit.Blocker(obelisk, "Collider", Vector3.up * 0.7f, new Vector3(1.1f, 1.4f, 1.1f));
        }

        private static void CrystalCluster(Transform parent, Vector3 position, VorgArt art, bool blocks = true)
        {
            var cluster = Kit.Group(parent, "Crystals", position);
            Kit.Cylinder(cluster, "Ash", Vector3.up * 0.03f, new Vector3(1.5f, 0.03f, 1.5f), art.MossDark);
            var offsets = new[] { new Vector3(0f, 0.5f, 0f), new Vector3(0.36f, 0.3f, 0.12f), new Vector3(-0.3f, 0.34f, 0.24f), new Vector3(0.05f, 0.24f, -0.42f) };
            var heights = new[] { 1f, 0.6f, 0.68f, 0.5f };
            for (var i = 0; i < offsets.Length; i++)
                Kit.Box(cluster, $"Crystal{i}", offsets[i], new Vector3(0.28f, heights[i], 0.28f), art.Ember, new Vector3(i * 11f - 12f, i * 40f + 20f, i * 9f - 10f));
            if (blocks) Kit.Blocker(cluster, "Collider", Vector3.up * 0.5f, new Vector3(1.05f, 1f, 1.05f));
        }

        private static void Altar(Transform parent, Vector3 position, VorgArt art)
        {
            var altar = Kit.Group(parent, "Altar", position);
            Kit.Box(altar, "Step", Vector3.up * 0.12f, new Vector3(3f, 0.24f, 1.8f), art.Trim);
            Kit.Box(altar, "Body", Vector3.up * 0.54f, new Vector3(2.5f, 0.62f, 1.25f), art.WallA);
            Kit.Box(altar, "Top", Vector3.up * 0.9f, new Vector3(2.75f, 0.12f, 1.45f), art.WallC, new Vector3(0f, 0f, 2f));
            Kit.Box(altar, "Stain", Vector3.up * 0.97f, new Vector3(1.5f, 0.02f, 0.6f), art.Ember);
            Kit.Blocker(altar, "Collider", Vector3.up * 0.5f, new Vector3(3f, 1f, 1.8f));
        }

        private static void BrokenArch(Transform parent, Vector3 position, float yaw, VorgArt art)
        {
            var arch = Kit.Group(parent, "BrokenArch", position, new Vector3(0f, yaw, 0f));
            Kit.Box(arch, "PostA", new Vector3(-1.05f, 0.72f, 0f), new Vector3(0.62f, 1.44f, 0.62f), art.WallA);
            Kit.Box(arch, "PostB", new Vector3(1.05f, 0.4f, 0f), new Vector3(0.62f, 0.8f, 0.62f), art.WallB, new Vector3(0f, 0f, 7f));
            Kit.Box(arch, "Fallen", new Vector3(0.1f, 0.22f, 0.72f), new Vector3(1.75f, 0.42f, 0.56f), art.WallC, new Vector3(0f, 22f, 6f));
            Kit.Box(arch, "CrackGlow", new Vector3(-1.05f, 0.9f, -0.33f), new Vector3(0.06f, 0.8f, 0.03f), art.Ember, new Vector3(0f, 0f, 12f));
            Kit.Blocker(arch, "ColliderA", new Vector3(-1.05f, 0.72f, 0f), new Vector3(0.62f, 1.44f, 0.62f));
            Kit.Blocker(arch, "ColliderB", new Vector3(1.05f, 0.4f, 0f), new Vector3(0.62f, 0.8f, 0.62f));
            Kit.Blocker(arch, "ColliderC", new Vector3(0.1f, 0.22f, 0.72f), new Vector3(1.75f, 0.44f, 0.56f));
        }

        private static void BuildMist(Transform parent, VorgArt art)
        {
            var go = new GameObject("GroundMist");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = Vector3.down * 1.7f;
            var system = go.AddComponent<ParticleSystem>();
            var main = system.main;
            main.loop = true;
            main.playOnAwake = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = art.Theme == VorgTheme.Sanctuary ? 16 : 9;
            main.startLifetime = new ParticleSystem.MinMaxCurve(7f, 11f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(4.5f, 8f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = Color.white;

            var emission = system.emission;
            emission.rateOverTime = art.Theme == VorgTheme.Sanctuary ? 1.7f : 0.9f;

            var shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(26f, 0.2f, 26f);

            var velocity = system.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = new ParticleSystem.MinMaxCurve(-0.18f, 0.18f);
            velocity.y = new ParticleSystem.MinMaxCurve(art.Theme == VorgTheme.Forge ? 0.02f : 0f, art.Theme == VorgTheme.Forge ? 0.08f : 0.01f);
            velocity.z = new ParticleSystem.MinMaxCurve(-0.18f, 0.18f);

            var fade = system.colorOverLifetime;
            fade.enabled = true;
            var gradient = new Gradient();
            var peak = art.Theme == VorgTheme.Sanctuary ? 0.17f : art.Theme == VorgTheme.Forge ? 0.12f : 0.09f;
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(peak, 0.4f), new GradientAlphaKey(0f, 1f) });
            fade.color = new ParticleSystem.MinMaxGradient(gradient);

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = art.Mist;
            renderer.renderMode = ParticleSystemRenderMode.HorizontalBillboard;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        public static GameObject BuildAtmosphere(VorgArt art)
        {
            var isForge = art.Theme == VorgTheme.Forge;
            var isSanctuary = art.Theme == VorgTheme.Sanctuary;
            var hex = isForge ? "#FF7A2E" : isSanctuary ? "#B78CFF" : "#D8E0C8";

            var root = new GameObject("Atmosphere");
            var system = root.AddComponent<ParticleSystem>();
            var main = system.main;
            main.loop = true;
            main.playOnAwake = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 70;
            main.startLifetime = new ParticleSystem.MinMaxCurve(3.5f, 6.5f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(isForge ? 0.05f : 0.06f, isForge ? 0.12f : 0.15f);
            main.startColor = Color.white;

            var emission = system.emission;
            emission.rateOverTime = isForge ? 14f : isSanctuary ? 12f : 7f;

            var shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(30f, 4f, 30f);

            var velocity = system.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = new ParticleSystem.MinMaxCurve(-0.15f, 0.15f);
            velocity.z = new ParticleSystem.MinMaxCurve(-0.15f, 0.15f);
            velocity.y = isForge ? new ParticleSystem.MinMaxCurve(0.35f, 0.8f) : isSanctuary ? new ParticleSystem.MinMaxCurve(-0.5f, -0.2f) : new ParticleSystem.MinMaxCurve(-0.05f, 0.08f);

            var fade = system.colorOverLifetime;
            fade.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(isSanctuary ? 0.8f : 0.6f, 0.25f), new GradientAlphaKey(0f, 1f) });
            fade.color = new ParticleSystem.MinMaxGradient(gradient);

            var renderer = root.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = art.Particle(hex);
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            BuildMist(root.transform, art);
            return Kit.SaveAsPrefab<GameObject>(root, $"{PrefabFolder}/Atmosphere_{art.Theme}.prefab");
        }
    }
}
