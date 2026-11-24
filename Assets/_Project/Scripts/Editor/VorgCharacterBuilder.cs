using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using VorgGates.Gameplay;
using VorgGates.Presentation;

namespace VorgGates.EditorTools
{
    internal static class VorgCharacterBuilder
    {
        private const string PrefabFolder = "Assets/_Project/Prefabs";

        public static GameObject BuildPlayer(VorgArt art, WeaponDefinition[] weapons)
        {
            var root = new GameObject("Player");
            var controller = root.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.42f;
            controller.center = new Vector3(0f, 0.9f, 0f);
            controller.skinWidth = 0.04f;
            controller.stepOffset = 0.2f;

            var visual = Kit.Group(root.transform, "Visual");
            var body = Kit.Group(visual, "Body");
            BuildKnightTorso(body, art);
            BuildKnightHelm(body, art);
            BuildKnightShield(body, art);
            var cape = BuildCape(body, art);

            var legL = BuildPlayerLeg(visual, "LegL", -0.17f, art);
            var legR = BuildPlayerLeg(visual, "LegR", 0.17f, art);

            var pivot = Kit.Group(visual, "WeaponPivot", new Vector3(0.43f, 0.98f, 0.13f));
            var weaponModels = VorgCombatBuilder.BuildWeaponModels(pivot, art);

            var effects = Kit.Group(root.transform, "Effects");
            VorgFx.ContactShadow(effects, art, 1.7f, false);
            var slash = new GameObject("Slash");
            slash.transform.SetParent(effects, false);
            slash.transform.localPosition = Vector3.up * 0.98f;
            var slashFilter = slash.AddComponent<MeshFilter>();
            var slashRenderer = slash.AddComponent<MeshRenderer>();
            slashRenderer.sharedMaterial = art.Slash;
            slashRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            var health = root.AddComponent<Health>();
            root.AddComponent<PlayerMotor>();
            root.AddComponent<PlayerAim>();
            var dodge = root.AddComponent<PlayerDodge>();
            root.AddComponent<PlayerInputReader>();
            var weapon = root.AddComponent<WeaponUser>();
            var inventory = root.AddComponent<PlayerInventory>();
            var interactor = root.AddComponent<Interactor>();
            root.AddComponent<PlayerStateMachine>();

            Kit.Wire(inventory, ("weapons", (Object[])weapons));
            Kit.Wire(weapon, ("weapon", weapons[0]), ("targetMask", (LayerMask)(1 << VorgProjectSetup.EnemyLayer)), ("obstacleMask", (LayerMask)(1 << VorgProjectSetup.WorldLayer)));
            Kit.Wire(interactor, ("interactableMask", (LayerMask)(1 << VorgProjectSetup.InteractableLayer)));
            Kit.Wire(root.AddComponent<SwingVisual>(), ("weaponUser", weapon), ("weaponPivot", pivot), ("weaponModels", (Object[])weaponModels), ("slashFilter", slashFilter), ("slashRenderer", slashRenderer));
            Kit.Wire(root.AddComponent<ProceduralLimbs>(), ("body", body), ("groupA", new Object[] { legL }), ("groupB", new Object[] { legR }), ("stepsPerMeter", 0.9f));
            Kit.Wire(root.AddComponent<HitFlash>(), ("health", health), ("visualRoot", visual), ("flashMaterial", art.HitFlash));
            Kit.Wire(root.AddComponent<HeroPose>(), ("visual", visual), ("body", body), ("cape", cape), ("weaponUser", weapon), ("dodge", dodge));

            Kit.SetLayer(root, VorgProjectSetup.PlayerLayer);
            return Kit.SaveAsPrefab<GameObject>(root, $"{PrefabFolder}/Player/Player.prefab");
        }

        private static void BuildKnightTorso(Transform body, VorgArt art)
        {
            Kit.Capsule(body, "Gambeson", new Vector3(0f, 1.02f, 0f), new Vector3(0.62f, 0.47f, 0.46f), art.DarkSteel);
            Kit.Box(body, "Breastplate", new Vector3(0f, 1.2f, 0.17f), new Vector3(0.56f, 0.46f, 0.2f), art.Steel, new Vector3(-7f, 0f, 0f));
            Kit.Box(body, "BreastRidge", new Vector3(0f, 1.2f, 0.28f), new Vector3(0.06f, 0.42f, 0.04f), art.Gold, new Vector3(-7f, 0f, 0f));
            Kit.Box(body, "Backplate", new Vector3(0f, 1.2f, -0.17f), new Vector3(0.54f, 0.44f, 0.14f), art.Steel, new Vector3(6f, 0f, 0f));
            Kit.Box(body, "Tabard", new Vector3(0f, 0.72f, 0.22f), new Vector3(0.38f, 0.62f, 0.05f), art.Terracotta, new Vector3(-3f, 0f, 0f));
            Kit.Box(body, "TabardTrim", new Vector3(0f, 0.43f, 0.245f), new Vector3(0.4f, 0.05f, 0.03f), art.Gold);
            Kit.Box(body, "CrossV", new Vector3(0f, 0.74f, 0.255f), new Vector3(0.06f, 0.34f, 0.02f), art.Gold);
            Kit.Box(body, "CrossH", new Vector3(0f, 0.8f, 0.255f), new Vector3(0.22f, 0.06f, 0.02f), art.Gold);
            Kit.Cylinder(body, "Belt", new Vector3(0f, 0.92f, 0f), new Vector3(0.62f, 0.05f, 0.5f), art.WoodDark);
            Kit.Box(body, "Buckle", new Vector3(0f, 0.92f, 0.26f), new Vector3(0.13f, 0.12f, 0.04f), art.Gold);
            Kit.Box(body, "Pouch", new Vector3(0.28f, 0.82f, 0.1f), new Vector3(0.14f, 0.16f, 0.12f), art.Wood);
            foreach (var side in new[] { -1f, 1f })
            {
                Kit.Box(body, "Tasset", new Vector3(side * 0.24f, 0.72f, 0.06f), new Vector3(0.22f, 0.3f, 0.34f), art.Steel, new Vector3(0f, 0f, side * 9f));
                Kit.Ball(body, "Pauldron", new Vector3(side * 0.4f, 1.34f, 0f), new Vector3(0.4f, 0.3f, 0.42f), art.Steel);
                Kit.Box(body, "PauldronLame", new Vector3(side * 0.45f, 1.22f, 0f), new Vector3(0.28f, 0.07f, 0.4f), art.Gold, new Vector3(0f, 0f, side * -24f));
                Kit.Box(body, "PauldronRidge", new Vector3(side * 0.4f, 1.5f, 0f), new Vector3(0.06f, 0.08f, 0.36f), art.Gold);
                Kit.Capsule(body, "Arm", new Vector3(side * 0.43f, 1f, 0.04f), new Vector3(0.17f, 0.27f, 0.17f), art.DarkSteel);
                Kit.Cylinder(body, "Vambrace", new Vector3(side * 0.43f, 0.9f, 0.05f), new Vector3(0.2f, 0.12f, 0.2f), art.Steel);
                Kit.Ball(body, "Gauntlet", new Vector3(side * 0.43f, 0.74f, 0.08f), Vector3.one * 0.19f, art.Steel);
            }
        }

        private static void BuildKnightHelm(Transform body, VorgArt art)
        {
            Kit.Cylinder(body, "Gorget", new Vector3(0f, 1.5f, 0f), new Vector3(0.36f, 0.07f, 0.36f), art.DarkSteel);
            Kit.Cylinder(body, "GreatHelm", new Vector3(0f, 1.74f, 0f), new Vector3(0.52f, 0.24f, 0.54f), art.Steel);
            Kit.Ball(body, "HelmDome", new Vector3(0f, 1.97f, 0f), new Vector3(0.52f, 0.28f, 0.54f), art.Steel);
            Kit.Box(body, "VisorSlit", new Vector3(0f, 1.78f, 0.265f), new Vector3(0.38f, 0.05f, 0.04f), art.Abyss);
            Kit.Box(body, "HelmCrossV", new Vector3(0f, 1.74f, 0.28f), new Vector3(0.07f, 0.44f, 0.03f), art.Gold);
            Kit.Box(body, "HelmCrossH", new Vector3(0f, 1.84f, 0.28f), new Vector3(0.44f, 0.05f, 0.03f), art.Gold);
            foreach (var side in new[] { -1f, 1f })
            {
                Kit.Box(body, "Breath", new Vector3(side * 0.12f, 1.64f, 0.262f), new Vector3(0.035f, 0.08f, 0.03f), art.Abyss);
                Kit.Box(body, "HelmWing", new Vector3(side * 0.34f, 2.03f, -0.02f), new Vector3(0.05f, 0.34f, 0.22f), art.Gold, new Vector3(0f, 0f, side * -28f));
            }

            Kit.Box(body, "CrestBase", new Vector3(0f, 2.13f, -0.02f), new Vector3(0.09f, 0.07f, 0.36f), art.Gold);
            for (var i = 0; i < 4; i++)
                Kit.Box(body, "Plume", new Vector3(0f, 2.24f - i * 0.03f, 0.1f - i * 0.17f), new Vector3(0.09f, 0.22f - i * 0.02f, 0.2f), art.Terracotta, new Vector3(-16f - i * 9f, 0f, 0f));
        }

        private static void BuildKnightShield(Transform body, VorgArt art)
        {
            var shield = Kit.Group(body, "Shield", new Vector3(-0.63f, 1f, 0.13f), new Vector3(0f, -12f, 0f));
            Kit.Box(shield, "ShieldFaceTop", new Vector3(0f, 0.14f, 0f), new Vector3(0.05f, 0.44f, 0.62f), art.TerracottaDark);
            Kit.Box(shield, "ShieldTipA", new Vector3(0f, -0.14f, 0.13f), new Vector3(0.05f, 0.5f, 0.32f), art.TerracottaDark, new Vector3(-24f, 0f, 0f));
            Kit.Box(shield, "ShieldTipB", new Vector3(0f, -0.14f, -0.13f), new Vector3(0.05f, 0.5f, 0.32f), art.TerracottaDark, new Vector3(24f, 0f, 0f));
            Kit.Box(shield, "RimTop", new Vector3(-0.015f, 0.37f, 0f), new Vector3(0.07f, 0.05f, 0.66f), art.Gold);
            foreach (var side in new[] { -1f, 1f })
            {
                Kit.Box(shield, "RimSide", new Vector3(-0.015f, 0.15f, side * 0.32f), new Vector3(0.07f, 0.44f, 0.05f), art.Gold);
                Kit.Box(shield, "RimTip", new Vector3(-0.015f, -0.25f, side * 0.17f), new Vector3(0.07f, 0.48f, 0.05f), art.Gold, new Vector3(side * -33f, 0f, 0f));
            }

            Kit.Box(shield, "EmblemV", new Vector3(-0.04f, 0.04f, 0f), new Vector3(0.025f, 0.56f, 0.09f), art.Parchment);
            Kit.Box(shield, "EmblemH", new Vector3(-0.04f, 0.14f, 0f), new Vector3(0.025f, 0.09f, 0.42f), art.Parchment);
            Kit.Ball(shield, "ShieldBoss", new Vector3(-0.05f, 0.14f, 0f), new Vector3(0.12f, 0.17f, 0.17f), art.Gold);
        }

        private static Transform BuildCape(Transform body, VorgArt art)
        {
            var cape = Kit.Group(body, "Cape", new Vector3(0f, 1.46f, -0.24f));
            Kit.Box(cape, "CapeCollar", new Vector3(0f, 0.02f, 0.02f), new Vector3(0.66f, 0.1f, 0.12f), art.Terracotta);
            for (var i = 0; i < 5; i++)
            {
                var width = 0.62f + i * 0.055f;
                Kit.Box(cape, "CapePanel", new Vector3(0f, -0.14f - i * 0.25f, -0.02f - i * 0.018f), new Vector3(width, 0.27f, 0.045f), i % 2 == 0 ? art.TerracottaDark : art.Terracotta, new Vector3(4f, 0f, 0f));
            }

            foreach (var x in new[] { -0.3f, -0.1f, 0.1f, 0.3f })
                Kit.Box(cape, "CapeTatter", new Vector3(x, -1.33f - Mathf.Abs(x) * 0.18f, -0.1f), new Vector3(0.16f, 0.2f, 0.04f), art.TerracottaDark, new Vector3(4f, 0f, x * 42f));
            Kit.Box(cape, "CapeClaspL", new Vector3(-0.28f, 0.03f, 0.1f), Vector3.one * 0.1f, art.Gold, new Vector3(0f, 0f, 45f));
            Kit.Box(cape, "CapeClaspR", new Vector3(0.28f, 0.03f, 0.1f), Vector3.one * 0.1f, art.Gold, new Vector3(0f, 0f, 45f));
            return cape;
        }

        public static EnemyBrain BuildCrawler(VorgArt art, EnemyDefinition definition)
        {
            var root = new GameObject($"Enemy_Rastejante{art.Suffix}");
            var capsule = root.AddComponent<CapsuleCollider>();
            capsule.isTrigger = true;
            capsule.center = new Vector3(0f, 0.55f, 0f);
            capsule.radius = 0.6f;
            capsule.height = 1.2f;

            var agent = root.AddComponent<NavMeshAgent>();
            agent.radius = 0.55f;
            agent.height = 1.1f;
            agent.acceleration = 30f;
            agent.angularSpeed = 720f;
            agent.autoBraking = false;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;

            var visual = Kit.Group(root.transform, "Visual");
            var body = Kit.Group(visual, "Body");
            Kit.Ball(body, "Abdomen", new Vector3(0f, 0.52f, -0.28f), new Vector3(1.04f, 0.68f, 1.3f), art.MossDark);
            Kit.Ball(body, "ShellFront", new Vector3(0f, 0.72f, 0.04f), new Vector3(0.82f, 0.42f, 0.56f), art.Moss);
            Kit.Ball(body, "ShellMid", new Vector3(0f, 0.8f, -0.34f), new Vector3(0.86f, 0.44f, 0.6f), art.Moss);
            Kit.Ball(body, "ShellBack", new Vector3(0f, 0.68f, -0.72f), new Vector3(0.66f, 0.36f, 0.48f), art.Moss);
            for (var i = 0; i < 3; i++)
                Kit.Box(body, $"Spine{i}", new Vector3(0f, 1.03f - i * 0.03f, 0.04f - i * 0.38f), new Vector3(0.09f, 0.3f, 0.2f), art.Parchment, new Vector3(-20f, 0f, 0f));
            Kit.Ball(body, "Head", new Vector3(0f, 0.5f, 0.44f), new Vector3(0.66f, 0.52f, 0.58f), art.Moss);
            Kit.Ball(body, "EyeL", new Vector3(-0.18f, 0.63f, 0.68f), Vector3.one * 0.15f, art.Ember);
            Kit.Ball(body, "EyeR", new Vector3(0.18f, 0.63f, 0.68f), Vector3.one * 0.15f, art.Ember);
            Kit.Box(body, "MandibleL", new Vector3(-0.2f, 0.36f, 0.86f), new Vector3(0.09f, 0.09f, 0.46f), art.Parchment, new Vector3(0f, 24f, 0f));
            Kit.Box(body, "MandibleR", new Vector3(0.2f, 0.36f, 0.86f), new Vector3(0.09f, 0.09f, 0.46f), art.Parchment, new Vector3(0f, -24f, 0f));

            var legsA = new Object[3];
            var legsB = new Object[3];
            var legZ = new[] { 0.22f, -0.2f, -0.62f };
            for (var i = 0; i < 3; i++)
            {
                var left = BuildCrawlerLeg(visual, $"LegL{i}", -1f, legZ[i], art);
                var right = BuildCrawlerLeg(visual, $"LegR{i}", 1f, legZ[i], art);
                legsA[i] = i % 2 == 0 ? left : right;
                legsB[i] = i % 2 == 0 ? right : left;
            }

            root.transform.localScale = Vector3.one * 0.8f;
            return VorgCombatBuilder.FinishCrawler(root, visual, body, definition, art, legsA, legsB);
        }

        private static Transform BuildPlayerLeg(Transform parent, string name, float x, VorgArt art)
        {
            var leg = Kit.Group(parent, name, new Vector3(x, 0.66f, 0f));
            Kit.Capsule(leg, "Thigh", new Vector3(0f, -0.2f, 0f), new Vector3(0.23f, 0.24f, 0.25f), art.DarkSteel);
            Kit.Ball(leg, "Poleyn", new Vector3(0f, -0.3f, 0.09f), new Vector3(0.2f, 0.17f, 0.16f), art.Gold);
            Kit.Cylinder(leg, "Greave", new Vector3(0f, -0.44f, 0f), new Vector3(0.23f, 0.13f, 0.25f), art.Steel);
            Kit.Box(leg, "Boot", new Vector3(0f, -0.59f, 0.07f), new Vector3(0.25f, 0.14f, 0.42f), art.DarkSteel);
            Kit.Box(leg, "BootToe", new Vector3(0f, -0.61f, 0.28f), new Vector3(0.18f, 0.09f, 0.12f), art.Steel, new Vector3(0f, 45f, 0f));
            return leg;
        }

        private static Transform BuildCrawlerLeg(Transform parent, string name, float side, float z, VorgArt art)
        {
            var leg = Kit.Group(parent, name, new Vector3(side * 0.42f, 0.42f, z));
            Kit.Box(leg, "Upper", new Vector3(side * 0.24f, 0.03f, 0f), new Vector3(0.52f, 0.09f, 0.1f), art.MossDark, new Vector3(0f, 0f, side * 18f));
            Kit.Box(leg, "Lower", new Vector3(side * 0.56f, -0.2f, 0f), new Vector3(0.09f, 0.48f, 0.09f), art.Parchment, new Vector3(0f, 0f, side * 18f));
            return leg;
        }
    }
}
