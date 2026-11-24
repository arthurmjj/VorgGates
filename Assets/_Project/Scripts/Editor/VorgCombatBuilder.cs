using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using VorgGates.Core;
using VorgGates.Gameplay;
using VorgGates.Presentation;

namespace VorgGates.EditorTools
{
    /// <summary>Builds weapon models, ranged enemies, projectiles and pickups from low-poly parts.</summary>
    internal static class VorgCombatBuilder
    {
        private const string EnemyFolder = "Assets/_Project/Prefabs/Enemies";
        private const string PropFolder = "Assets/_Project/Prefabs/Props";

        public static GameObject[] BuildWeaponModels(Transform parent, VorgArt art) => new[]
        {
            BuildSwordModel(parent, art),
            BuildAxeModel(parent, art),
            BuildStaffModel(parent, art)
        };

        private static GameObject BuildSwordModel(Transform parent, VorgArt art)
        {
            var model = Kit.Group(parent, "Model_Espada");
            Kit.Box(model, "Grip", new Vector3(0f, 0f, 0.08f), new Vector3(0.07f, 0.07f, 0.24f), art.WoodDark);
            for (var i = 0; i < 3; i++) Kit.Box(model, "GripWrap", new Vector3(0f, 0f, 0.02f + i * 0.07f), new Vector3(0.085f, 0.085f, 0.025f), art.Gold);
            Kit.Ball(model, "Pommel", new Vector3(0f, 0f, -0.07f), Vector3.one * 0.13f, art.Gold);
            Kit.Box(model, "Guard", new Vector3(0f, 0f, 0.23f), new Vector3(0.46f, 0.08f, 0.09f), art.Gold);
            foreach (var side in new[] { -1f, 1f }) Kit.Box(model, "GuardFlare", new Vector3(side * 0.26f, 0f, 0.27f), new Vector3(0.15f, 0.08f, 0.09f), art.Gold, new Vector3(0f, side * -32f, 0f));
            Kit.Ball(model, "GuardGem", new Vector3(0f, 0.045f, 0.23f), new Vector3(0.09f, 0.05f, 0.09f), art.Terracotta);
            Kit.Box(model, "Blade", new Vector3(0f, 0f, 0.84f), new Vector3(0.15f, 0.04f, 1.14f), art.Steel);
            Kit.Box(model, "Fuller", new Vector3(0f, 0.012f, 0.72f), new Vector3(0.035f, 0.03f, 0.84f), art.DarkSteel);
            Kit.Box(model, "BladeTip", new Vector3(0f, 0f, 1.42f), new Vector3(0.11f, 0.04f, 0.11f), art.Steel, new Vector3(0f, 45f, 0f));
            return model.gameObject;
        }

        private static GameObject BuildAxeModel(Transform parent, VorgArt art)
        {
            var model = Kit.Group(parent, "Model_Machado");
            Kit.Box(model, "Handle", new Vector3(0f, 0f, 0.62f), new Vector3(0.09f, 0.09f, 1.5f), art.Wood);
            Kit.Box(model, "Collar", new Vector3(0f, 0f, 1.16f), new Vector3(0.15f, 0.15f, 0.34f), art.Gold);
            Kit.Box(model, "Cheek", new Vector3(0.2f, 0f, 1.16f), new Vector3(0.36f, 0.08f, 0.42f), art.Steel);
            Kit.Box(model, "BladeA", new Vector3(0.45f, 0f, 1.3f), new Vector3(0.34f, 0.05f, 0.42f), art.Steel, new Vector3(0f, 30f, 0f));
            Kit.Box(model, "BladeB", new Vector3(0.45f, 0f, 1.02f), new Vector3(0.34f, 0.05f, 0.42f), art.Steel, new Vector3(0f, -30f, 0f));
            Kit.Box(model, "Edge", new Vector3(0.62f, 0f, 1.16f), new Vector3(0.08f, 0.06f, 0.82f), art.Parchment);
            Kit.Box(model, "Spike", new Vector3(-0.15f, 0f, 1.16f), new Vector3(0.22f, 0.07f, 0.16f), art.Steel, new Vector3(0f, 45f, 0f));
            Kit.Box(model, "BackBladeA", new Vector3(-0.3f, 0f, 1.25f), new Vector3(0.26f, 0.05f, 0.24f), art.Steel, new Vector3(0f, -30f, 0f));
            Kit.Box(model, "BackBladeB", new Vector3(-0.3f, 0f, 1.07f), new Vector3(0.26f, 0.05f, 0.24f), art.Steel, new Vector3(0f, 30f, 0f));
            Kit.Box(model, "BladeInlay", new Vector3(0.33f, 0.035f, 1.16f), new Vector3(0.2f, 0.02f, 0.28f), art.Gold);
            for (var i = 0; i < 3; i++) Kit.Box(model, "HandleWrap", new Vector3(0f, 0f, 0.1f + i * 0.09f), new Vector3(0.115f, 0.115f, 0.04f), art.TerracottaDark);
            Kit.Ball(model, "HandleCap", new Vector3(0f, 0f, -0.15f), Vector3.one * 0.13f, art.Gold);
            return model.gameObject;
        }

        private static GameObject BuildStaffModel(Transform parent, VorgArt art)
        {
            var model = Kit.Group(parent, "Model_Cajado");
            Kit.Cylinder(model, "Shaft", new Vector3(0f, 0f, 0.62f), new Vector3(0.1f, 0.86f, 0.1f), art.Wood, new Vector3(90f, 0f, 0f));
            Kit.Cylinder(model, "BandA", new Vector3(0f, 0f, 0.4f), new Vector3(0.14f, 0.04f, 0.14f), art.Gold, new Vector3(90f, 0f, 0f));
            Kit.Cylinder(model, "Crown", new Vector3(0f, 0f, 1.46f), new Vector3(0.28f, 0.07f, 0.28f), art.Gold, new Vector3(90f, 0f, 0f));
            for (var i = 0; i < 4; i++)
            {
                var radians = i * Mathf.PI * 0.5f;
                Kit.Box(model, $"Prong{i}", new Vector3(Mathf.Cos(radians) * 0.17f, Mathf.Sin(radians) * 0.17f, 1.62f), new Vector3(0.06f, 0.06f, 0.3f), art.Gold);
            }

            Kit.Ball(model, "Orb", new Vector3(0f, 0f, 1.7f), Vector3.one * 0.32f, art.Ether);
            for (var i = 0; i < 3; i++)
            {
                var radians = i * Mathf.PI * 2f / 3f;
                Kit.Box(model, "StaffRoot", new Vector3(Mathf.Cos(radians) * 0.07f, Mathf.Sin(radians) * 0.07f, 0.92f + i * 0.13f), new Vector3(0.05f, 0.05f, 0.3f), art.WoodDark, new Vector3(Mathf.Sin(radians) * 14f, Mathf.Cos(radians) * 14f, 0f));
            }

            Kit.Box(model, "CharmCord", new Vector3(0.1f, -0.1f, 1.24f), new Vector3(0.02f, 0.2f, 0.02f), art.Parchment);
            Kit.Ball(model, "Charm", new Vector3(0.1f, -0.22f, 1.24f), Vector3.one * 0.08f, art.Gold);
            return model.gameObject;
        }

        public static Projectile BuildMagicBolt(VorgArt art)
        {
            var root = new GameObject("Projectile_Magic");
            var core = Kit.Ball(root.transform, "Core", Vector3.zero, Vector3.one * 0.38f, art.Ether);
            var halo = Kit.Ball(root.transform, "Tail", Vector3.back * 0.3f, new Vector3(0.22f, 0.22f, 0.58f), art.Ether);
            NoShadow(core, halo);
            var projectile = root.AddComponent<Projectile>();
            Kit.Wire(projectile, ("radius", 0.2f));
            Kit.SetLayer(root, VorgProjectSetup.PlayerHitboxLayer);
            return Kit.SaveAsPrefab<Projectile>(root, $"{PropFolder}/Projectile_Magic.prefab");
        }

        public static Projectile BuildFireBolt(VorgArt art)
        {
            var root = new GameObject("Projectile_Fire");
            NoShadow(
                Kit.Ball(root.transform, "Core", Vector3.zero, Vector3.one * 0.46f, art.Flame),
                Kit.Box(root.transform, "ShardA", Vector3.back * 0.3f, new Vector3(0.24f, 0.24f, 0.52f), art.Ember, new Vector3(0f, 0f, 45f)));
            var projectile = root.AddComponent<Projectile>();
            Kit.Wire(projectile, ("radius", 0.24f));
            Kit.SetLayer(root, VorgProjectSetup.EnemyProjectileLayer);
            return Kit.SaveAsPrefab<Projectile>(root, $"{PropFolder}/Projectile_Fire.prefab");
        }

        public static Projectile BuildVoidBolt(VorgArt art)
        {
            var root = new GameObject("Projectile_Void");
            NoShadow(
                Kit.Box(root.transform, "Crystal", Vector3.zero, new Vector3(0.3f, 0.3f, 0.62f), art.Ember, new Vector3(0f, 0f, 45f)),
                Kit.Ball(root.transform, "Core", Vector3.zero, Vector3.one * 0.24f, art.Parchment));
            var projectile = root.AddComponent<Projectile>();
            Kit.Wire(projectile, ("radius", 0.24f));
            Kit.SetLayer(root, VorgProjectSetup.EnemyProjectileLayer);
            return Kit.SaveAsPrefab<Projectile>(root, $"{PropFolder}/Projectile_Void.prefab");
        }

        public static Projectile BuildArrow(VorgArt art)
        {
            var root = new GameObject("Projectile_Arrow");
            var shaft = Kit.Box(root.transform, "Shaft", Vector3.zero, new Vector3(0.06f, 0.06f, 0.95f), art.Parchment);
            var tip = Kit.Box(root.transform, "Tip", Vector3.forward * 0.52f, new Vector3(0.16f, 0.05f, 0.16f), art.Ember, new Vector3(0f, 45f, 0f));
            var fletchA = Kit.Box(root.transform, "FletchA", Vector3.back * 0.42f, new Vector3(0.26f, 0.025f, 0.2f), art.Terracotta);
            var fletchB = Kit.Box(root.transform, "FletchB", Vector3.back * 0.42f, new Vector3(0.025f, 0.26f, 0.2f), art.Terracotta);
            NoShadow(shaft, tip, fletchA, fletchB);
            var projectile = root.AddComponent<Projectile>();
            Kit.Wire(projectile, ("radius", 0.18f));
            Kit.SetLayer(root, VorgProjectSetup.EnemyProjectileLayer);
            return Kit.SaveAsPrefab<Projectile>(root, $"{PropFolder}/Projectile_Arrow.prefab");
        }

        public static HealOrb BuildHealOrb(VorgArt art)
        {
            var root = new GameObject("HealOrb");
            NoShadow(
                Kit.Ball(root.transform, "Core", Vector3.zero, Vector3.one * 0.42f, art.HealGlow),
                Kit.Box(root.transform, "CrossV", Vector3.zero, new Vector3(0.12f, 0.62f, 0.12f), art.Parchment),
                Kit.Box(root.transform, "CrossH", Vector3.zero, new Vector3(0.62f, 0.12f, 0.12f), art.Parchment));
            VorgFx.Sparkles(root.transform, "HealSparks", art.GoldSpark, Vector3.down * 0.25f, 0.34f, 5f, 0.16f, 0.65f, 10);
            root.AddComponent<HealOrb>();
            return Kit.SaveAsPrefab<HealOrb>(root, $"{PropFolder}/HealOrb.prefab");
        }

        public static WeaponPickup BuildRewardPickup(VorgArt art)
        {
            var root = new GameObject("RewardPickup");
            var trigger = root.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 1.5f;
            trigger.center = Vector3.up;

            var floatRoot = Kit.Group(root.transform, "Float", Vector3.up * 2.35f);
            var models = BuildWeaponModels(floatRoot, art);
            foreach (var model in models) model.transform.localPosition = Vector3.back * 0.7f;
            var elixir = Kit.Group(floatRoot, "Model_Elixir");
            VorgExternalAssets.Place(elixir, "Dungeon/potion", Vector3.down * 0.42f, 0f, 2.1f, art);
            Kit.Ball(elixir, "ElixirGlow", Vector3.down * 0.16f, Vector3.one * 0.42f, art.HealGlow);
            foreach (var renderer in floatRoot.GetComponentsInChildren<MeshRenderer>(true)) renderer.shadowCastingMode = ShadowCastingMode.Off;
            VorgFx.Sparkles(floatRoot, "RewardSparks", art.GoldSpark, Vector3.down * 1.2f, 0.85f, 7f, 0.2f, 0.85f, 16);

            var ring = Kit.Cylinder(root.transform, "GlowRing", Vector3.up * 0.04f, new Vector3(3.3f, 0.012f, 3.3f), art.RuneGold);
            NoShadow(ring);

            var pickup = root.AddComponent<WeaponPickup>();
            Kit.Wire(pickup, ("weaponModels", new Object[] { models[0], models[1], models[2] }), ("elixirModel", elixir.gameObject), ("floatRoot", floatRoot), ("trigger", trigger));
            Kit.SetLayer(root, VorgProjectSetup.InteractableLayer);
            return Kit.SaveAsPrefab<WeaponPickup>(root, $"{PropFolder}/RewardPickup.prefab");
        }

        public static EnemyBrain BuildArcher(VorgArt art, EnemyDefinition definition)
        {
            var root = EnemyRoot($"Enemy_ArqueiroOssudo{art.Suffix}", 0.5f, 1.9f, 0.45f, 1.9f);
            var visual = Kit.Group(root.transform, "Visual");
            var body = Kit.Group(visual, "Body");

            Kit.Cylinder(body, "Pelvis", new Vector3(0f, 0.86f, 0f), new Vector3(0.46f, 0.08f, 0.32f), art.Parchment);
            Kit.Cylinder(body, "Spine", new Vector3(0f, 1.1f, -0.03f), new Vector3(0.1f, 0.28f, 0.1f), art.Parchment);
            for (var i = 0; i < 4; i++)
            {
                var width = 0.62f - i * 0.05f;
                Kit.Cylinder(body, $"Rib{i}", new Vector3(0f, 1.42f - i * 0.12f, 0f), new Vector3(width, 0.022f, width * 0.68f), art.Parchment);
            }

            Kit.Box(body, "Tabard", new Vector3(0f, 0.72f, 0.14f), new Vector3(0.34f, 0.5f, 0.04f), art.MossDark);
            Kit.Ball(body, "ShoulderL", new Vector3(-0.33f, 1.47f, 0f), Vector3.one * 0.19f, art.Parchment);
            Kit.Ball(body, "ShoulderR", new Vector3(0.33f, 1.47f, 0f), Vector3.one * 0.19f, art.Parchment);
            Kit.Capsule(body, "ArmR", new Vector3(0.36f, 1.2f, 0.12f), new Vector3(0.09f, 0.28f, 0.09f), art.Parchment, new Vector3(28f, 0f, 0f));
            Kit.Capsule(body, "ArmL", new Vector3(-0.36f, 1.34f, 0.28f), new Vector3(0.09f, 0.3f, 0.09f), art.Parchment, new Vector3(82f, 0f, 0f));

            Kit.Ball(body, "Skull", new Vector3(0f, 1.8f, 0.02f), new Vector3(0.46f, 0.48f, 0.5f), art.Parchment);
            Kit.Box(body, "Jaw", new Vector3(0f, 1.61f, 0.1f), new Vector3(0.28f, 0.11f, 0.26f), art.Parchment);
            Kit.Ball(body, "EyeL", new Vector3(-0.1f, 1.83f, 0.23f), Vector3.one * 0.11f, art.Ember);
            Kit.Ball(body, "EyeR", new Vector3(0.1f, 1.83f, 0.23f), Vector3.one * 0.11f, art.Ember);
            Kit.Ball(body, "Hood", new Vector3(0f, 1.87f, -0.08f), new Vector3(0.56f, 0.56f, 0.56f), art.MossDark);
            Kit.Box(body, "HoodPeak", new Vector3(0f, 2.15f, -0.2f), new Vector3(0.18f, 0.24f, 0.18f), art.MossDark, new Vector3(-24f, 45f, 0f));
            Kit.Box(body, "Quiver", new Vector3(0.14f, 1.32f, -0.3f), new Vector3(0.2f, 0.62f, 0.18f), art.WoodDark, new Vector3(-10f, 0f, -16f));
            for (var i = 0; i < 3; i++)
                Kit.Box(body, $"QuiverArrow{i}", new Vector3(0.1f + i * 0.08f, 1.72f, -0.36f), new Vector3(0.05f, 0.24f, 0.05f), art.Terracotta, new Vector3(-10f, 0f, -16f));

            for (var i = 0; i < 7; i++)
            {
                var t = (i - 3) / 3f;
                var angle = t * 62f;
                var position = new Vector3(-0.36f + Mathf.Cos(t * 1.1f) * 0.3f - 0.3f + 0.3f, 1.34f + Mathf.Sin(t * 1.1f) * 0.62f, 0.62f);
                Kit.Box(body, $"Bow{i}", position, new Vector3(0.07f, 0.24f, 0.07f), art.Wood, new Vector3(0f, 0f, angle));
            }

            Kit.Box(body, "BowString", new Vector3(-0.3f, 1.34f, 0.62f), new Vector3(0.012f, 1.14f, 0.012f), art.Parchment);

            var legL = Kit.Group(visual, "LegL", new Vector3(-0.15f, 0.84f, 0f));
            var legR = Kit.Group(visual, "LegR", new Vector3(0.15f, 0.84f, 0f));
            foreach (var leg in new[] { legL, legR })
            {
                Kit.Capsule(leg, "Bone", new Vector3(0f, -0.4f, 0f), new Vector3(0.11f, 0.42f, 0.11f), art.Parchment);
                Kit.Box(leg, "Foot", new Vector3(0f, -0.79f, 0.07f), new Vector3(0.15f, 0.08f, 0.32f), art.Parchment);
            }

            return FinishEnemy(root, visual, body, definition, art, new Object[] { legL }, new Object[] { legR }, 1f, 9f);
        }

        public static EnemyBrain BuildBrute(VorgArt art, EnemyDefinition definition)
        {
            var root = EnemyRoot($"Enemy_Bruto{art.Suffix}", 0.95f, 2.5f, 0.85f, 2.5f);
            var visual = Kit.Group(root.transform, "Visual");
            var body = Kit.Group(visual, "Body");

            Kit.Ball(body, "Gut", new Vector3(0f, 1.18f, 0.05f), new Vector3(1.6f, 1.34f, 1.3f), art.TerracottaDark);
            Kit.Ball(body, "Chest", new Vector3(0f, 1.72f, 0f), new Vector3(1.85f, 1.05f, 1.2f), art.TerracottaDark);
            Kit.Box(body, "ChestPlate", new Vector3(0f, 1.62f, 0.54f), new Vector3(1.18f, 0.86f, 0.2f), art.Steel, new Vector3(-6f, 0f, 0f));
            Kit.Box(body, "Rivet", new Vector3(0f, 1.62f, 0.67f), new Vector3(0.28f, 0.28f, 0.06f), art.Gold, new Vector3(0f, 0f, 45f));
            Kit.Cylinder(body, "Belt", new Vector3(0f, 0.92f, 0.03f), new Vector3(1.5f, 0.1f, 1.24f), art.WoodDark);
            Kit.Box(body, "Buckle", new Vector3(0f, 0.92f, 0.65f), new Vector3(0.36f, 0.26f, 0.08f), art.Gold);
            Kit.Ball(body, "ShoulderL", new Vector3(-0.98f, 2.02f, 0f), new Vector3(0.82f, 0.68f, 0.82f), art.Steel);
            Kit.Ball(body, "ShoulderR", new Vector3(0.98f, 2.02f, 0f), new Vector3(0.82f, 0.68f, 0.82f), art.Steel);
            foreach (var side in new[] { -1f, 1f })
            {
                Kit.Box(body, "ShoulderSpike", new Vector3(side * 1.13f, 2.42f, 0f), new Vector3(0.2f, 0.42f, 0.2f), art.Parchment, new Vector3(0f, 45f, side * -18f));
                Kit.Capsule(body, "Arm", new Vector3(side * 1.1f, 1.38f, 0.1f), new Vector3(0.48f, 0.56f, 0.48f), art.TerracottaDark);
                Kit.Ball(body, "Fist", new Vector3(side * 1.12f, 0.8f, 0.18f), Vector3.one * 0.52f, art.Skin);
            }

            Kit.Ball(body, "Head", new Vector3(0f, 2.42f, 0.2f), new Vector3(0.74f, 0.66f, 0.72f), art.Skin);
            Kit.Ball(body, "Helmet", new Vector3(0f, 2.56f, 0.13f), new Vector3(0.82f, 0.6f, 0.82f), art.Steel);
            Kit.Box(body, "Visor", new Vector3(0f, 2.43f, 0.5f), new Vector3(0.62f, 0.09f, 0.14f), art.Abyss);
            Kit.Ball(body, "EyeL", new Vector3(-0.15f, 2.43f, 0.55f), Vector3.one * 0.12f, art.Ember);
            Kit.Ball(body, "EyeR", new Vector3(0.15f, 2.43f, 0.55f), Vector3.one * 0.12f, art.Ember);
            foreach (var side in new[] { -1f, 1f })
            {
                Kit.Box(body, "HornBase", new Vector3(side * 0.48f, 2.72f, 0.1f), new Vector3(0.24f, 0.24f, 0.24f), art.Parchment, new Vector3(0f, 0f, side * -35f));
                Kit.Box(body, "HornTip", new Vector3(side * 0.62f, 2.94f, 0.1f), new Vector3(0.14f, 0.34f, 0.14f), art.Parchment, new Vector3(0f, 0f, side * -18f));
            }

            Kit.Cylinder(body, "HammerShaft", new Vector3(1.18f, 1.16f, 0.72f), new Vector3(0.14f, 0.72f, 0.14f), art.Wood, new Vector3(62f, 0f, 0f));
            Kit.Box(body, "HammerHead", new Vector3(1.18f, 1.48f, 1.34f), new Vector3(0.82f, 0.56f, 0.56f), art.Steel, new Vector3(62f, 0f, 0f));
            Kit.Box(body, "HammerBand", new Vector3(1.18f, 1.48f, 1.34f), new Vector3(0.88f, 0.14f, 0.6f), art.Gold, new Vector3(62f, 0f, 0f));

            var legL = Kit.Group(visual, "LegL", new Vector3(-0.45f, 0.82f, 0f));
            var legR = Kit.Group(visual, "LegR", new Vector3(0.45f, 0.82f, 0f));
            foreach (var leg in new[] { legL, legR })
            {
                Kit.Capsule(leg, "Thigh", new Vector3(0f, -0.38f, 0f), new Vector3(0.56f, 0.46f, 0.6f), art.WoodDark);
                Kit.Box(leg, "Boot", new Vector3(0f, -0.72f, 0.12f), new Vector3(0.62f, 0.22f, 0.86f), art.Abyss);
            }

            return FinishEnemy(root, visual, body, definition, art, new Object[] { legL }, new Object[] { legR }, 2.4f, 7f);
        }

        public static EnemyBrain FinishCrawler(GameObject root, Transform visual, Transform body, EnemyDefinition definition, VorgArt art, Object[] legsA, Object[] legsB) =>
            FinishEnemy(root, visual, body, definition, art, legsA, legsB, 1.5f, 0f, 2.1f, 28f, 0.035f);

        internal static GameObject EnemyRoot(string name, float radius, float height, float agentRadius, float agentHeight)
        {
            var root = new GameObject(name);
            var capsule = root.AddComponent<CapsuleCollider>();
            capsule.isTrigger = true;
            capsule.center = Vector3.up * (height * 0.5f);
            capsule.radius = radius;
            capsule.height = height;
            var agent = root.AddComponent<NavMeshAgent>();
            agent.radius = agentRadius;
            agent.height = agentHeight;
            agent.acceleration = 30f;
            agent.angularSpeed = 720f;
            agent.autoBraking = false;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
            return root;
        }

        private static EnemyBrain FinishEnemy(GameObject root, Transform visual, Transform body, EnemyDefinition definition, VorgArt art, Object[] groupA, Object[] groupB, float discScale, float aimLength, float steps = 1.4f, float swing = 30f, float bob = 0.05f)
        {
            VorgThemeDecor.EnemyAccessory(body, root.GetComponent<CapsuleCollider>().height, art);
            VorgDressing.EnemyDetails(body, definition.Kind, art);
            var effects = Kit.Group(root.transform, "Effects");
            VorgFx.ContactShadow(effects, art, root.GetComponent<CapsuleCollider>().radius * 3.4f, true);
            var disc = Kit.Cylinder(effects, "WarningDisc", Vector3.up * 0.04f, new Vector3(1f, 0.01f, 1f), art.Warning);
            NoShadow(disc);

            Transform line = null;
            if (aimLength > 0f)
            {
                var lineObject = Kit.Box(effects, "AimLine", Vector3.up * 0.05f, new Vector3(0.55f, 0.01f, 1f), art.Warning);
                NoShadow(lineObject);
                line = lineObject.transform;
            }

            var health = root.AddComponent<Health>();
            Kit.Wire(health, ("maxHealth", definition.MaxHealth), ("invulnerabilityAfterHit", 0f));
            var brain = root.AddComponent<EnemyBrain>();
            Kit.Wire(brain,
                ("definition", definition),
                ("targetMask", (LayerMask)(1 << VorgProjectSetup.PlayerLayer)),
                ("obstacleMask", (LayerMask)(1 << VorgProjectSetup.WorldLayer)));

            var feedback = root.AddComponent<EnemyFeedback>();
            Kit.Wire(feedback, ("brain", brain), ("visual", visual), ("warningDisc", disc.transform), ("aimLength", aimLength));
            if (line != null) Kit.Wire(feedback, ("aimLine", line));
            Kit.Wire(root.AddComponent<ProceduralLimbs>(), ("body", body), ("groupA", groupA), ("groupB", groupB), ("stepsPerMeter", steps), ("swingDegrees", swing), ("bobHeight", bob));
            var flash = root.AddComponent<HitFlash>();
            Kit.Wire(flash, ("health", health), ("visualRoot", visual), ("flashMaterial", art.HitFlash));
            if (definition.Behavior == EnemyBehavior.Charger)
                Kit.Wire(root.AddComponent<OcclusionFade>(), ("brain", brain), ("flash", flash), ("ghostMaterial", art.Ghost), ("halfWidth", 1.35f), ("height", 2.9f));

            Kit.SetLayer(root, VorgProjectSetup.EnemyLayer);
            return Kit.SaveAsPrefab<EnemyBrain>(root, $"{EnemyFolder}/{root.name}.prefab");
        }

        public static LegendarySword BuildLegendarySword(VorgArt art)
        {
            var root = new GameObject("LegendarySword");
            var trigger = root.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 2f;
            trigger.center = Vector3.up;

            Kit.Cylinder(root.transform, "StoneBase", Vector3.up * 0.22f, new Vector3(2.5f, 0.22f, 2.5f), art.WallB);
            Kit.Ball(root.transform, "Stone", Vector3.up * 0.72f, new Vector3(1.75f, 1.05f, 1.55f), art.WallC, new Vector3(0f, 25f, 8f));
            Kit.Box(root.transform, "StoneChip", new Vector3(0.45f, 1.02f, 0.2f), new Vector3(0.6f, 0.42f, 0.5f), art.WallA, new Vector3(24f, 35f, 18f));
            Kit.Cylinder(root.transform, "AltarGoldBand", Vector3.up * 0.44f, new Vector3(2.56f, 0.025f, 2.56f), art.Gold);
            Kit.Cylinder(root.transform, "AltarStep", Vector3.up * 0.5f, new Vector3(2.1f, 0.05f, 2.1f), art.Trim);
            for (var i = 0; i < 12; i++)
            {
                var radians = i * Mathf.PI / 6f;
                Kit.Box(root.transform, "AltarRune", new Vector3(Mathf.Sin(radians) * 1.245f, 0.23f, Mathf.Cos(radians) * 1.245f), new Vector3(0.14f, 0.24f, 0.03f), art.RuneGold, new Vector3(0f, i * 30f, 0f));
            }

            foreach (var x in new[] { -0.72f, 0.72f })
            foreach (var z in new[] { -0.72f, 0.72f })
            {
                Kit.Box(root.transform, "AltarPost", new Vector3(x, 0.72f, z), new Vector3(0.22f, 0.5f, 0.22f), art.Trim, new Vector3(0f, 45f, 0f));
                Kit.Ball(root.transform, "AltarPostCap", new Vector3(x, 1.02f, z), Vector3.one * 0.2f, art.Gold);
            }

            var blade = Kit.Group(root.transform, "Blade");
            Kit.Box(blade, "Steel", Vector3.up * 1.72f, new Vector3(0.26f, 1.6f, 0.06f), art.Parchment);
            Kit.Box(blade, "BladeEdgeA", new Vector3(-0.13f, 1.72f, 0f), new Vector3(0.05f, 1.56f, 0.04f), art.Gold);
            Kit.Box(blade, "BladeEdgeB", new Vector3(0.13f, 1.72f, 0f), new Vector3(0.05f, 1.56f, 0.04f), art.Gold);
            Kit.Box(blade, "Fuller", Vector3.up * 1.76f, new Vector3(0.07f, 1.4f, 0.08f), art.RuneGold);
            for (var i = 0; i < 4; i++)
                Kit.Box(blade, "BladeRune", new Vector3(0f, 1.25f + i * 0.34f, -0.045f), new Vector3(0.12f, 0.12f, 0.02f), art.Flame, new Vector3(0f, 0f, 45f));
            Kit.Box(blade, "Guard", Vector3.up * 2.58f, new Vector3(0.82f, 0.12f, 0.18f), art.Gold);
            foreach (var side in new[] { -1f, 1f })
            {
                Kit.Box(blade, "GuardWing", new Vector3(side * 0.5f, 2.68f, 0f), new Vector3(0.3f, 0.1f, 0.16f), art.Gold, new Vector3(0f, 0f, side * 34f));
                Kit.Ball(blade, "GuardOrb", new Vector3(side * 0.64f, 2.78f, 0f), Vector3.one * 0.13f, art.Parchment);
            }

            Kit.Box(blade, "GuardGem", Vector3.up * 2.58f, new Vector3(0.2f, 0.2f, 0.22f), art.Terracotta, new Vector3(0f, 0f, 45f));
            Kit.Box(blade, "Grip", Vector3.up * 2.86f, new Vector3(0.11f, 0.46f, 0.11f), art.TerracottaDark);
            for (var i = 0; i < 4; i++) Kit.Box(blade, "GripWrap", Vector3.up * (2.7f + i * 0.11f), new Vector3(0.135f, 0.03f, 0.135f), art.Gold);
            Kit.Ball(blade, "Pommel", Vector3.up * 3.16f, Vector3.one * 0.22f, art.Gold);
            Kit.Ball(blade, "PommelGem", new Vector3(0f, 3.16f, -0.1f), Vector3.one * 0.09f, art.Terracotta);

            var glow = Kit.Group(root.transform, "Glow");
            NoShadow(Kit.Part(glow, PrimitiveType.Quad, "GoldenHalo", Vector3.up * 0.2f, new Vector3(7.5f, 7.5f, 1f), art.GoldHalo, new Vector3(90f, 0f, 0f)));
            var runeRing = Kit.Group(glow, "RuneRing", Vector3.up * 0.5f);
            for (var i = 0; i < 12; i++)
            {
                var radians = i * Mathf.PI / 6f;
                NoShadow(Kit.Box(runeRing, "FloatingRune", new Vector3(Mathf.Sin(radians) * 2.05f, i % 2 == 0 ? 0.34f : 0.5f, Mathf.Cos(radians) * 2.05f), new Vector3(0.14f, 0.3f, 0.03f), art.RuneGold, new Vector3(0f, i * 30f, 0f)));
            }

            var beam = Kit.Part(glow, PrimitiveType.Cylinder, "GoldenBeam", Vector3.up * 5.4f, new Vector3(2.5f, 5.4f, 2.5f), art.LightBeam);
            NoShadow(beam);
            VorgFx.Sparkles(glow, "GoldenMotes", art.GoldSpark, Vector3.up * 0.45f, 2f, 15f, 0.24f, 1.25f, 44);
            var burst = VorgFx.Burst(glow, "TakeBurst", art.GoldSpark, Vector3.up * 1.9f, 90, 6.5f, 0.42f);
            var lightObject = new GameObject("SwordLight");
            lightObject.transform.SetParent(glow, false);
            lightObject.transform.localPosition = new Vector3(-0.8f, 2.4f, -0.8f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = VorgArt.Hex("#F2C14E");
            light.range = 11f;
            light.intensity = 5.2f;
            light.shadows = LightShadows.None;

            var sword = root.AddComponent<LegendarySword>();
            Kit.Wire(sword, ("blade", blade), ("glow", glow.gameObject));
            Kit.Wire(root.AddComponent<SwordCeremony>(), ("sword", sword), ("swordLight", light), ("beam", beam.transform), ("runeRing", runeRing), ("burst", burst));
            Kit.Blocker(root.transform, "Collider", Vector3.up * 0.7f, new Vector3(1.9f, 1.4f, 1.9f));
            foreach (var child in root.GetComponentsInChildren<Transform>(true))
                child.gameObject.layer = child.GetComponent<BoxCollider>() != null ? VorgProjectSetup.WorldLayer : VorgProjectSetup.InteractableLayer;
            return Kit.SaveAsPrefab<LegendarySword>(root, $"{PropFolder}/LegendarySword.prefab");
        }

        internal static void NoShadow(params GameObject[] parts)
        {
            foreach (var part in parts) part.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
        }
    }
}
