using UnityEngine;
using VorgGates.Core;

namespace VorgGates.EditorTools
{
    /// <summary>Visual-only architecture and decoration. Nothing here adds a collider.</summary>
    internal static class VorgDressing
    {
        private const float WallFace = 8.97f;

        private static readonly string[] CryptCover =
        {
            "Nature/grass", "Nature/grass_large", "Nature/grass_leafs", "Nature/grass_leafsLarge", "Nature/plant_bushSmall",
            "Nature/plant_flatShort", "Nature/mushroom_tanGroup", "Nature/mushroom_redGroup", "Nature/flower_purpleA", "Nature/grass_leafs"
        };

        private static GameObject Prop(Transform parent, string model, float x, float z, float yaw, float scale, VorgArt art) =>
            VorgExternalAssets.Place(parent, model, new Vector3(x, 0f, z), yaw, scale, art);

        public static void GroundCover(Transform parent, VorgArt art, System.Random random, int count)
        {
            var total = art.Theme == VorgTheme.Crypt ? count + 12 : count;
            for (var i = 0; i < total; i++)
            {
                var angle = (float)random.NextDouble() * Mathf.PI * 2f;
                var distance = 3.1f + (float)random.NextDouble() * 5.3f;
                var x = Mathf.Cos(angle) * distance;
                var z = Mathf.Sin(angle) * distance;
                var yaw = (float)random.NextDouble() * 360f;
                switch (art.Theme)
                {
                    case VorgTheme.Forge:
                        Prop(parent, "Graveyard/debris", x, z, yaw, 1.5f + (float)random.NextDouble(), art);
                        break;
                    case VorgTheme.Sanctuary:
                        if (i % 3 == 0) CrystalCluster(parent, new Vector3(x, 0f, z), 0.32f + (float)random.NextDouble() * 0.3f, art, random);
                        else Prop(parent, "Graveyard/debris", x, z, yaw, 1.4f + (float)random.NextDouble(), art);
                        break;
                    default:
                        Prop(parent, CryptCover[random.Next(CryptCover.Length)], x, z, yaw, 2.1f + (float)random.NextDouble() * 1.2f, art);
                        break;
                }
            }
        }

        public static void WallFeatures(Transform parent, Side side, float from, float to, VorgArt art, System.Random random)
        {
            var yaw = side == Side.North ? 0f : 90f;
            for (var along = from + 1.1f; along < to - 0.5f; along += 2.05f)
            {
                if (Mathf.Abs(along) < 0.95f || Mathf.Abs(Mathf.Abs(along) - 5.5f) < 0.62f || Mathf.Abs(along - 7.9f) < 0.8f) continue;
                var position = VorgRoomBuilder.SidePoint(side, along, 0f, WallFace);
                if (art.Theme != VorgTheme.Crypt) continue;
                var group = Kit.Group(parent, "WallFeature", position, new Vector3(0f, yaw, 0f));
                Vines(group, art, random);
            }
        }

        private static void Vines(Transform group, VorgArt art, System.Random random)
        {
            var strands = 2 + random.Next(2);
            for (var s = 0; s < strands; s++)
            {
                var x = -0.45f + s * 0.42f + (float)random.NextDouble() * 0.12f;
                var length = 3 + random.Next(4);
                var previous = new Vector3(x, 3.32f, -0.05f);
                for (var i = 0; i < length; i++)
                {
                    var next = previous + new Vector3(((float)random.NextDouble() - 0.5f) * 0.24f, -0.34f, 0f);
                    var middle = (previous + next) * 0.5f;
                    var tilt = Mathf.Atan2(next.x - previous.x, previous.y - next.y) * Mathf.Rad2Deg;
                    Kit.Capsule(group, "Vine", middle, new Vector3(0.055f, 0.2f, 0.055f), art.MossDark, new Vector3(0f, 0f, tilt));
                    if (i % 2 == 0) Kit.Ball(group, "Leaf", next + new Vector3(0.08f, 0f, -0.03f), new Vector3(0.2f, 0.13f, 0.08f), art.Moss, new Vector3(0f, 0f, 28f));
                    previous = next;
                }
            }
        }

        public static void PillarDetail(Transform pillar, float height, VorgArt art)
        {
            switch (art.Theme)
            {
                case VorgTheme.Forge:
                    if (height <= 2f) break;
                    Kit.Box(pillar, "HeatSlit", new Vector3(0f, height * 0.5f, -0.64f), new Vector3(0.14f, 0.7f, 0.04f), art.Lava);
                    Kit.Box(pillar, "HeatSlitSide", new Vector3(-0.64f, height * 0.5f, 0f), new Vector3(0.04f, 0.7f, 0.14f), art.Lava);
                    break;
                case VorgTheme.Sanctuary:
                    if (height <= 2f) break;
                    Kit.Ball(pillar, "SpireGem", Vector3.up * (height + 0.42f), new Vector3(0.2f, 0.34f, 0.2f), art.Crystal);
                    break;
                default:
                    VorgExternalAssets.Place(pillar, "Nature/grass_large", new Vector3(0.2f, height + 0.08f, -0.18f), height * 70f, 2.1f, art);
                    break;
            }
        }

        public static void InteriorProps(Transform decor, VorgArt art, System.Random random, string name)
        {
            if (name.Contains("Entrance") || name.Contains("Boss")) Landmark(decor, art, random, true);
            else if (!name.Contains("Exit_Sanctuary")) Landmark(decor, art, random, false);
            LowProps(decor, art, random);
        }

        private static void Landmark(Transform decor, VorgArt art, System.Random random, bool grand)
        {
            switch (art.Theme)
            {
                case VorgTheme.Forge:
                    Furnace(decor, new Vector3(4.35f, 0f, 8.62f), 0f, art);
                    if (grand) Furnace(decor, new Vector3(8.62f, 0f, -4.35f), 90f, art);
                    ToolRack(decor, new Vector3(8.45f, 0f, 3.9f), 90f, art);
                    break;
                case VorgTheme.Sanctuary:
                    Statue(decor, new Vector3(4.4f, 0f, 8.55f), 180f, art, true);
                    Statue(decor, new Vector3(8.55f, 0f, -4.4f), -90f, art, true);
                    CrystalCluster(decor, new Vector3(8.6f, 0f, 3.8f), 0.8f, art, random);
                    if (grand) CrystalCluster(decor, new Vector3(-4.2f, 0f, 8.6f), 0.85f, art, random);
                    break;
                default:
                    Statue(decor, new Vector3(4.4f, 0f, 8.55f), 180f, art, false);
                    Prop(decor, "Graveyard/cross-column", 8.6f, -4.3f, -30f, 1.45f, art);
                    Prop(decor, "Graveyard/gravestone-decorative", 8.6f, 3.9f, -90f, 1.7f, art);
                    Prop(decor, "Graveyard/gravestone-cross", -4.3f, 8.6f, 180f, 1.6f, art);
                    if (grand) Statue(decor, new Vector3(8.55f, 0f, -6.2f), -90f, art, false);
                    break;
            }
        }

        private static void LowProps(Transform decor, VorgArt art, System.Random random)
        {
            string[] props;
            switch (art.Theme)
            {
                case VorgTheme.Forge:
                    props = new[] { "Dungeon/stones", "Graveyard/fire-basket", "Dungeon/barrel", "Graveyard/debris" };
                    break;
                case VorgTheme.Sanctuary:
                    props = new[] { "Graveyard/candle-multiple", "Graveyard/urn-square", "Graveyard/gravestone-broken", "Graveyard/candle-multiple" };
                    break;
                default:
                    props = new[] { "Graveyard/urn-round", "Graveyard/candle-multiple", "Graveyard/gravestone-broken", "Dungeon/pot" };
                    break;
            }

            var spots = new[] { new Vector2(-4.2f, -8.25f), new Vector2(4.3f, -8.25f), new Vector2(-8.25f, 4.2f), new Vector2(-8.25f, -4.3f) };
            for (var i = 0; i < spots.Length; i++)
                Prop(decor, props[i], spots[i].x, spots[i].y, (float)random.NextDouble() * 360f, 1.45f, art);
        }

        public static void InteriorLights(Transform root, VorgArt art, string name)
        {
            if (art.Theme == VorgTheme.Forge)
            {
                AddLight(root, new Vector3(4.35f, 1.5f, 6.5f), art.TorchColor, 8.5f, 3.4f);
                if (name.Contains("Boss")) AddLight(root, new Vector3(6.5f, 1.5f, -4.35f), art.TorchColor, 8.5f, 3f);
            }
            else if (art.Theme == VorgTheme.Sanctuary && !name.Contains("Exit"))
            {
                AddLight(root, new Vector3(7f, 1.5f, 3.6f), VorgArt.Hex("#B98CFF"), 7.5f, 2.6f);
            }
        }

        private static void AddLight(Transform parent, Vector3 position, Color color, float range, float intensity)
        {
            var go = new GameObject("AccentLight");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.range = range;
            light.intensity = intensity;
            light.shadows = LightShadows.None;
        }

        public static void Statue(Transform parent, Vector3 position, float yaw, VorgArt art, bool cursed)
        {
            var statue = Kit.Group(parent, "Statue", position, new Vector3(0f, yaw, 0f));
            Kit.Box(statue, "PlinthStone", Vector3.up * 0.16f, new Vector3(1.05f, 0.32f, 1.05f), art.Trim);
            Kit.Box(statue, "PlinthTop", Vector3.up * 0.38f, new Vector3(0.84f, 0.14f, 0.84f), art.WallC);
            for (var i = 0; i < 4; i++)
            {
                var t = i / 3f;
                var width = Mathf.Lerp(0.74f, 0.42f, t);
                Kit.Cylinder(statue, "Robe", Vector3.up * (0.62f + i * 0.34f), new Vector3(width, 0.2f, width * 0.82f), i % 2 == 0 ? art.WallA : art.WallC);
            }

            foreach (var side in new[] { -1f, 1f })
            {
                Kit.Ball(statue, "Shoulder", new Vector3(side * 0.3f, 1.84f, 0f), new Vector3(0.3f, 0.26f, 0.3f), art.WallC);
                Kit.Capsule(statue, "Arm", new Vector3(side * 0.2f, 1.52f, 0.2f), new Vector3(0.15f, 0.34f, 0.15f), art.WallA, new Vector3(36f, 0f, side * 28f));
            }

            Kit.Ball(statue, "Hood", new Vector3(0f, 2.16f, -0.03f), new Vector3(0.46f, 0.56f, 0.5f), art.WallB);
            Kit.Ball(statue, "Face", new Vector3(0f, 2.12f, 0.12f), new Vector3(0.28f, 0.34f, 0.26f), cursed ? art.Abyss : art.WallC);
            if (cursed)
            {
                Kit.Ball(statue, "EyeL", new Vector3(-0.06f, 2.16f, 0.24f), Vector3.one * 0.05f, art.Ember);
                Kit.Ball(statue, "EyeR", new Vector3(0.06f, 2.16f, 0.24f), Vector3.one * 0.05f, art.Ember);
            }

            Kit.Box(statue, "StoneBlade", new Vector3(0f, 0.98f, 0.36f), new Vector3(0.12f, 1.05f, 0.04f), cursed ? art.Crystal : art.Steel);
            Kit.Box(statue, "StoneGuard", new Vector3(0f, 1.52f, 0.36f), new Vector3(0.44f, 0.07f, 0.08f), art.Gold);
            Kit.Box(statue, "StoneGrip", new Vector3(0f, 1.66f, 0.36f), new Vector3(0.07f, 0.24f, 0.07f), art.WoodDark);
        }

        public static void Furnace(Transform parent, Vector3 position, float yaw, VorgArt art)
        {
            var furnace = Kit.Group(parent, "Furnace", position, new Vector3(0f, yaw, 0f));
            furnace.localScale = new Vector3(1f, 1f, 0.5f);
            Kit.Box(furnace, "BaseBlock", Vector3.up * 0.2f, new Vector3(2.3f, 0.4f, 1.5f), art.WallB);
            Kit.Box(furnace, "BodyStone", Vector3.up * 1.05f, new Vector3(2f, 1.35f, 1.26f), art.WallA);
            Kit.Box(furnace, "Shoulder", Vector3.up * 1.84f, new Vector3(1.66f, 0.3f, 1.08f), art.WallC);
            Kit.Box(furnace, "Chimney", new Vector3(0f, 2.62f, 0.2f), new Vector3(0.82f, 1.3f, 0.72f), art.WallB);
            Kit.Box(furnace, "ChimneyCap", new Vector3(0f, 3.3f, 0.2f), new Vector3(1.02f, 0.14f, 0.92f), art.DarkSteel);
            Kit.Box(furnace, "Mouth", new Vector3(0f, 0.86f, -0.61f), new Vector3(1.04f, 0.74f, 0.08f), art.Lava);
            Kit.Box(furnace, "ArchTop", new Vector3(0f, 1.3f, -0.66f), new Vector3(1.3f, 0.16f, 0.14f), art.Trim);
            foreach (var side in new[] { -1f, 1f })
            {
                Kit.Box(furnace, "ArchSide", new Vector3(side * 0.6f, 0.84f, -0.66f), new Vector3(0.16f, 0.82f, 0.14f), art.Trim);
                Kit.Box(furnace, "Band", new Vector3(side * 0.96f, 1.05f, -0.64f), new Vector3(0.1f, 1.42f, 0.06f), art.DarkSteel);
                VorgThemeDecor.Chain(furnace, new Vector3(side * 0.9f, 1.95f, -0.62f), 4, art);
            }

            for (var i = 0; i < 4; i++)
                Kit.Box(furnace, "GrateBar", new Vector3(-0.36f + i * 0.24f, 0.86f, -0.67f), new Vector3(0.05f, 0.72f, 0.05f), art.DarkSteel);
        }

        private static void ToolRack(Transform parent, Vector3 position, float yaw, VorgArt art)
        {
            var rack = Kit.Group(parent, "ToolRack", position, new Vector3(0f, yaw, 0f));
            Kit.Box(rack, "Board", new Vector3(0f, 1.72f, 0.28f), new Vector3(1.9f, 0.7f, 0.07f), art.WoodDark);
            for (var i = 0; i < 4; i++)
            {
                var x = -0.66f + i * 0.44f;
                Kit.Box(rack, "Handle", new Vector3(x, 1.28f, 0.22f), new Vector3(0.07f, 1.05f, 0.05f), art.Wood);
                Kit.Box(rack, "Head", new Vector3(x, 0.78f, 0.22f), i % 2 == 0 ? new Vector3(0.36f, 0.18f, 0.1f) : new Vector3(0.2f, 0.3f, 0.05f), art.Steel);
            }
        }

        public static void CrystalCluster(Transform parent, Vector3 position, float size, VorgArt art, System.Random random)
        {
            var cluster = Kit.Group(parent, "CrystalCluster", position, new Vector3(0f, (float)random.NextDouble() * 360f, 0f));
            Kit.Ball(cluster, "RockBase", Vector3.up * 0.05f * size, new Vector3(1.1f, 0.26f, 1f) * size, art.WallB);
            var count = size > 0.7f ? 5 : 3;
            for (var i = 0; i < count; i++)
            {
                var angle = i * Mathf.PI * 2f / count;
                var offset = i == 0 ? Vector3.zero : new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 0.26f * size;
                var height = (i == 0 ? 1.45f : 0.72f + (float)random.NextDouble() * 0.42f) * size;
                var tilt = new Vector3(offset.z * 52f / Mathf.Max(0.2f, size), (float)random.NextDouble() * 180f, -offset.x * 52f / Mathf.Max(0.2f, size));
                Kit.Ball(cluster, "Crystal", offset + Vector3.up * (height * 0.44f), new Vector3(0.3f * size, height, 0.3f * size), art.Crystal, tilt);
            }
        }

        public static void EnemyDetails(Transform body, EnemyKind kind, VorgArt art)
        {
            switch (kind)
            {
                case EnemyKind.Crawler:
                    for (var i = 0; i < 4; i++)
                        Kit.Box(body, "CarapacePlate", new Vector3(0f, 0.92f - i * 0.04f, 0.16f - i * 0.28f), new Vector3(0.72f - i * 0.06f, 0.09f, 0.3f), art.MossDark, new Vector3(-14f, 0f, 0f));
                    foreach (var side in new[] { -1f, 1f })
                    {
                        Kit.Capsule(body, "Antenna", new Vector3(side * 0.16f, 0.86f, 0.62f), new Vector3(0.04f, 0.24f, 0.04f), art.Parchment, new Vector3(38f, 0f, side * -24f));
                        Kit.Ball(body, "CheekPlate", new Vector3(side * 0.32f, 0.44f, 0.56f), new Vector3(0.22f, 0.2f, 0.32f), art.MossDark);
                    }

                    break;
                case EnemyKind.Archer:
                    Kit.Box(body, "TatteredCloak", new Vector3(0f, 1.12f, -0.24f), new Vector3(0.62f, 1.1f, 0.05f), art.MossDark, new Vector3(8f, 0f, 0f));
                    for (var i = 0; i < 4; i++)
                        Kit.Box(body, "CloakTatter", new Vector3(-0.22f + i * 0.15f, 0.48f - (i % 2) * 0.08f, -0.31f), new Vector3(0.12f, 0.3f, 0.04f), art.MossDark, new Vector3(8f, 0f, i * 11f - 16f));
                    Kit.Box(body, "ShoulderGuard", new Vector3(0.36f, 1.58f, 0f), new Vector3(0.3f, 0.09f, 0.34f), art.DarkSteel, new Vector3(0f, 0f, -18f));
                    break;
                case EnemyKind.Brute:
                    Kit.Box(body, "BellyPlate", new Vector3(0f, 1.18f, 0.66f), new Vector3(0.86f, 0.42f, 0.1f), art.DarkSteel, new Vector3(-8f, 0f, 0f));
                    for (var i = 0; i < 3; i++)
                    {
                        Kit.Box(body, "BackSpike", new Vector3(-0.42f + i * 0.42f, 2.24f, -0.42f), new Vector3(0.16f, 0.46f, 0.16f), art.Parchment, new Vector3(-28f, 45f, 0f));
                        Kit.Ball(body, "PlateRivet", new Vector3(-0.3f + i * 0.3f, 1.2f, 0.73f), Vector3.one * 0.08f, art.Gold);
                    }

                    VorgThemeDecor.Chain(body, new Vector3(-0.78f, 1.86f, 0.5f), 5, art);
                    break;
            }
        }

        public static void GuardianDetails(Transform body, VorgArt art)
        {
            Kit.Box(body, "RockCrownA", new Vector3(-0.3f, 3.34f, -0.08f), new Vector3(0.36f, 0.5f, 0.34f), art.WallC, new Vector3(12f, 20f, 16f));
            Kit.Box(body, "RockCrownB", new Vector3(0.34f, 3.28f, 0.02f), new Vector3(0.3f, 0.42f, 0.32f), art.WallA, new Vector3(-8f, 40f, -18f));
            foreach (var side in new[] { -1f, 1f })
            {
                Kit.Box(body, "ShoulderRockA", new Vector3(side * 1.5f, 2.72f, -0.12f), new Vector3(0.62f, 0.76f, 0.6f), art.WallC, new Vector3(10f, 25f, side * -20f));
                Kit.Box(body, "ShoulderRockB", new Vector3(side * 1.08f, 2.86f, 0.2f), new Vector3(0.36f, 0.44f, 0.38f), art.WallA, new Vector3(-14f, 12f, side * 26f));
                Kit.Box(body, "Crack", new Vector3(side * 0.56f, 1.82f, 0.76f), new Vector3(0.05f, 0.82f, 0.03f), art.Crystal, new Vector3(0f, 0f, side * 24f));
                Kit.Box(body, "CrackBranch", new Vector3(side * 0.72f, 1.56f, 0.76f), new Vector3(0.04f, 0.42f, 0.03f), art.Crystal, new Vector3(0f, 0f, side * -32f));
            }

            Kit.Ball(body, "CoreSocket", new Vector3(0f, 1.78f, 0.74f), new Vector3(0.82f, 0.82f, 0.2f), art.WallB);
            Kit.Ball(body, "MagicCore", new Vector3(0f, 1.78f, 0.82f), new Vector3(0.48f, 0.48f, 0.2f), art.Crystal);
            for (var i = 0; i < 6; i++)
            {
                var a = i * Mathf.PI / 3f;
                Kit.Box(body, "CoreRune", new Vector3(Mathf.Cos(a) * 0.5f, 1.78f + Mathf.Sin(a) * 0.5f, 0.82f), new Vector3(0.08f, 0.2f, 0.03f), art.RuneGold, new Vector3(0f, 0f, i * 60f - 90f));
            }

            Kit.Ball(body, "MossBack", new Vector3(0.1f, 2.5f, -0.58f), new Vector3(1.5f, 0.5f, 0.5f), art.MossDark);
            Kit.Ball(body, "MossArm", new Vector3(1.52f, 1.5f, -0.18f), new Vector3(0.5f, 0.62f, 0.46f), art.Moss);
        }

        public static void ExecutionerDetails(Transform body, VorgArt art)
        {
            Kit.Box(body, "Cuirass", new Vector3(0f, 2.06f, 0.3f), new Vector3(1.12f, 0.8f, 0.22f), art.DarkSteel, new Vector3(-6f, 0f, 0f));
            Kit.Box(body, "CuirassSeam", new Vector3(0f, 2.06f, 0.43f), new Vector3(0.07f, 0.76f, 0.03f), art.Lava);
            Kit.Box(body, "CuirassSeamH", new Vector3(0f, 2.14f, 0.43f), new Vector3(0.82f, 0.06f, 0.03f), art.Lava);
            Kit.Ball(body, "ForgeHeart", new Vector3(0f, 2.14f, 0.45f), new Vector3(0.28f, 0.28f, 0.12f), art.Flame);
            foreach (var side in new[] { -1f, 1f })
            {
                Kit.Box(body, "Pauldron", new Vector3(side * 0.82f, 2.54f, 0f), new Vector3(0.74f, 0.2f, 0.82f), art.DarkSteel, new Vector3(0f, 0f, side * -20f));
                Kit.Box(body, "PauldronEdge", new Vector3(side * 1.1f, 2.42f, 0f), new Vector3(0.08f, 0.1f, 0.84f), art.Lava, new Vector3(0f, 0f, side * -20f));
                for (var i = 0; i < 2; i++)
                    Kit.Box(body, "PauldronSpike", new Vector3(side * (0.7f + i * 0.28f), 2.84f - i * 0.1f, 0.18f - i * 0.36f), new Vector3(0.13f, 0.42f, 0.13f), art.Steel, new Vector3(0f, 45f, side * -16f));
                Kit.Box(body, "HelmHorn", new Vector3(side * 0.5f, 3.14f, 0f), new Vector3(0.16f, 0.58f, 0.16f), art.Abyss, new Vector3(0f, 45f, side * -30f));
                Kit.Box(body, "Tasset", new Vector3(side * 0.34f, 1.04f, 0.3f), new Vector3(0.4f, 0.48f, 0.08f), art.DarkSteel, new Vector3(-6f, 0f, side * 6f));
            }

            Kit.Box(body, "BackFurnace", new Vector3(0f, 1.98f, -0.46f), new Vector3(0.74f, 0.82f, 0.32f), art.DarkSteel);
            Kit.Box(body, "BackFurnaceGlow", new Vector3(0f, 1.98f, -0.63f), new Vector3(0.44f, 0.44f, 0.03f), art.Lava);
            Kit.Box(body, "CleaverEmber", new Vector3(0.62f, 2.6f, 1.45f), new Vector3(0.5f, 0.06f, 0.03f), art.Lava, new Vector3(16f, 0f, 18f));
        }

        public static void SentinelDetails(Transform body, VorgArt art)
        {
            for (var i = 0; i < 8; i++)
            {
                var a = i * Mathf.PI / 4f;
                Kit.Box(body, "HaloSpike", new Vector3(Mathf.Cos(a) * 1f, 3.04f + Mathf.Sin(a) * 1f, -0.54f), new Vector3(0.1f, 0.34f, 0.04f), art.Crystal, new Vector3(0f, 0f, i * 45f - 90f));
            }

            foreach (var side in new[] { -1f, 1f })
            {
                Kit.Box(body, "GothicPauldron", new Vector3(side * 0.86f, 2.44f, 0f), new Vector3(0.64f, 0.26f, 0.74f), art.Steel, new Vector3(0f, 0f, side * -24f));
                Kit.Box(body, "PauldronGold", new Vector3(side * 1.1f, 2.34f, 0f), new Vector3(0.08f, 0.12f, 0.76f), art.Gold, new Vector3(0f, 0f, side * -24f));
                Kit.Box(body, "PauldronSpire", new Vector3(side * 0.82f, 2.82f, 0f), new Vector3(0.14f, 0.56f, 0.14f), art.Gold, new Vector3(0f, 45f, side * -12f));
                for (var i = 0; i < 4; i++)
                    Kit.Box(body, "CrystalWing", new Vector3(side * (0.62f + i * 0.26f), 2f + i * 0.16f, -0.42f), new Vector3(0.14f, 0.9f - i * 0.1f, 0.05f), art.Crystal, new Vector3(0f, 0f, side * (-18f - i * 14f)));
                Kit.Box(body, "ArmoredSkirt", new Vector3(side * 0.42f, 1.34f, 0.5f), new Vector3(0.42f, 0.74f, 0.07f), art.Steel, new Vector3(-12f, 0f, side * 7f));
            }

            Kit.Box(body, "ChestSigilV", new Vector3(0f, 2.05f, 0.72f), new Vector3(0.07f, 0.68f, 0.03f), art.Gold);
            Kit.Box(body, "ChestSigilH", new Vector3(0f, 2.22f, 0.72f), new Vector3(0.48f, 0.07f, 0.03f), art.Gold);
            Kit.Box(body, "RobeRune", new Vector3(0f, 0.72f, 0.84f), new Vector3(0.14f, 0.54f, 0.03f), art.RuneGold);
        }
    }
}
