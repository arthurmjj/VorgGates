using UnityEngine;
using VorgGates.Gameplay;
using VorgGates.Presentation;

namespace VorgGates.EditorTools
{
    /// <summary>Three bosses with distinct silhouettes, built from low-poly parts.</summary>
    internal static class VorgBossBuilder
    {
        private const string Folder = "Assets/_Project/Prefabs/Enemies";
        private const int TelegraphDiscs = 6;

        public static BossPattern[] GuardianPatterns() => new[]
        {
            new BossPattern { kind = BossPatternKind.Slam, damage = 20, telegraph = 1f, recovery = 1.25f, duration = 0.3f, range = 3.4f, radius = 4f },
            new BossPattern { kind = BossPatternKind.Charge, damage = 22, telegraph = 1f, recovery = 1.4f, duration = 0.5f, range = 9f, distance = 10f }
        };

        public static BossPattern[] ExecutionerPatterns() => new[]
        {
            new BossPattern { kind = BossPatternKind.Cone, damage = 28, telegraph = 0.85f, recovery = 1f, duration = 0.3f, range = 4.2f, radius = 5.5f, arcDegrees = 110f },
            new BossPattern { kind = BossPatternKind.Volley, damage = 16, telegraph = 0.8f, recovery = 1f, duration = 0.25f, range = 12f, radius = 9f, arcDegrees = 70f, count = 5, projectileSpeed = 10f },
            new BossPattern { kind = BossPatternKind.Bursts, damage = 26, telegraph = 1.05f, recovery = 1f, duration = 0.3f, range = 14f, radius = 2.1f, count = 4 }
        };

        public static BossPattern[] SentinelPatterns() => new[]
        {
            new BossPattern { kind = BossPatternKind.Nova, damage = 20, telegraph = 0.85f, recovery = 0.85f, duration = 0.25f, range = 13f, radius = 3f, count = 14, projectileSpeed = 8.5f },
            new BossPattern { kind = BossPatternKind.Bursts, damage = 32, telegraph = 0.95f, recovery = 0.85f, duration = 0.3f, range = 15f, radius = 2.3f, count = 6 },
            new BossPattern { kind = BossPatternKind.BlinkSlam, damage = 36, telegraph = 0.95f, recovery = 1.15f, duration = 0.3f, range = 15f, radius = 3.2f }
        };

        public static EnemyBrain BuildGuardian(VorgArt art, EnemyDefinition definition)
        {
            var root = VorgCombatBuilder.EnemyRoot("Boss_GuardiaoDaCripta", 1.15f, 3f, 0.8f, 3f);
            var visual = Kit.Group(root.transform, "Visual");
            var body = Kit.Group(visual, "Body");

            Kit.Box(body, "Pelvis", new Vector3(0f, 0.98f, 0f), new Vector3(1.5f, 0.55f, 1.1f), art.WallB, new Vector3(0f, 8f, 0f));
            Kit.Box(body, "Torso", new Vector3(0f, 1.78f, 0f), new Vector3(2.1f, 1.25f, 1.45f), art.WallC, new Vector3(4f, -6f, 2f));
            Kit.Box(body, "TorsoSlab", new Vector3(0.12f, 2.28f, 0.1f), new Vector3(2.4f, 0.48f, 1.55f), art.WallA, new Vector3(-4f, 12f, -3f));
            Kit.Box(body, "ChestRune", new Vector3(0f, 1.78f, 0.76f), new Vector3(0.55f, 0.55f, 0.05f), art.Ether, new Vector3(0f, 0f, 45f));
            Kit.Box(body, "Head", new Vector3(0f, 2.86f, 0.14f), new Vector3(0.92f, 0.72f, 0.86f), art.WallC, new Vector3(0f, -10f, 0f));
            Kit.Box(body, "Brow", new Vector3(0f, 3.04f, 0.5f), new Vector3(1f, 0.16f, 0.26f), art.WallB);
            Kit.Ball(body, "EyeL", new Vector3(-0.22f, 2.88f, 0.57f), Vector3.one * 0.15f, art.Ether);
            Kit.Ball(body, "EyeR", new Vector3(0.22f, 2.88f, 0.57f), Vector3.one * 0.15f, art.Ether);
            Kit.Ball(body, "MossL", new Vector3(-1.08f, 2.55f, 0f), new Vector3(0.95f, 0.38f, 0.95f), art.Moss);
            Kit.Ball(body, "MossR", new Vector3(1.1f, 2.52f, 0.05f), new Vector3(0.82f, 0.32f, 0.86f), art.MossDark);
            Kit.Ball(body, "MossHead", new Vector3(0.1f, 3.26f, 0.1f), new Vector3(0.7f, 0.2f, 0.62f), art.Moss);

            Kit.Box(body, "ShoulderL", new Vector3(-1.32f, 2.2f, 0f), new Vector3(0.9f, 0.85f, 0.95f), art.WallA, new Vector3(0f, 0f, 12f));
            Kit.Box(body, "ArmL", new Vector3(-1.46f, 1.38f, 0.14f), new Vector3(0.62f, 1.1f, 0.68f), art.WallB, new Vector3(10f, 0f, 6f));
            Kit.Box(body, "Shield", new Vector3(-1.62f, 1.22f, 0.62f), new Vector3(1.55f, 2f, 0.2f), art.Trim, new Vector3(0f, -24f, 0f));
            Kit.Box(body, "ShieldInlay", new Vector3(-1.56f, 1.22f, 0.76f), new Vector3(0.28f, 1.45f, 0.05f), art.Gold, new Vector3(0f, -24f, 0f));
            Kit.Box(body, "ShieldBar", new Vector3(-1.56f, 1.48f, 0.76f), new Vector3(0.95f, 0.26f, 0.05f), art.Gold, new Vector3(0f, -24f, 0f));

            Kit.Box(body, "ShoulderR", new Vector3(1.32f, 2.16f, 0f), new Vector3(0.9f, 0.85f, 0.95f), art.WallA, new Vector3(0f, 0f, -14f));
            Kit.Box(body, "ArmR", new Vector3(1.48f, 1.32f, 0.2f), new Vector3(0.68f, 1.2f, 0.72f), art.WallB, new Vector3(-16f, 0f, -6f));
            Kit.Box(body, "FistR", new Vector3(1.54f, 0.62f, 0.42f), new Vector3(0.98f, 0.82f, 0.98f), art.WallC, new Vector3(12f, 20f, 8f));

            var legL = Kit.Group(visual, "LegL", new Vector3(-0.58f, 0.78f, 0f));
            var legR = Kit.Group(visual, "LegR", new Vector3(0.58f, 0.78f, 0f));
            foreach (var leg in new[] { legL, legR })
            {
                Kit.Box(leg, "Thigh", new Vector3(0f, -0.28f, 0f), new Vector3(0.72f, 0.72f, 0.78f), art.WallB);
                Kit.Box(leg, "Foot", new Vector3(0f, -0.66f, 0.14f), new Vector3(0.9f, 0.26f, 1.15f), art.WallA);
            }

            VorgDressing.GuardianDetails(body, art);
            return Finish(root, visual, body, definition, art, new Object[] { legL }, new Object[] { legR }, 0.04f);
        }

        public static EnemyBrain BuildExecutioner(VorgArt art, EnemyDefinition definition)
        {
            var root = VorgCombatBuilder.EnemyRoot("Boss_CarrascoDaForja", 0.85f, 3f, 0.7f, 3f);
            var visual = Kit.Group(root.transform, "Visual");
            var body = Kit.Group(visual, "Body");
            var wirePose = VorgExecutionerModel.Place(root, body, 4.1f, art);
            return Finish(root, visual, body, definition, art, new Object[0], new Object[0], 0f, null, wirePose);
        }

        public static EnemyBrain BuildSentinel(VorgArt art, EnemyDefinition definition)
        {
            var root = VorgCombatBuilder.EnemyRoot("Boss_SentinelaDeVorg", 1f, 3.2f, 0.75f, 3f);
            var visual = Kit.Group(root.transform, "Visual");
            var body = Kit.Group(visual, "Body");
            var animator = VorgBossModels.PlaceKnight(body, 3.45f, art.SentinelArmor);
            var aura = Kit.Group(root.transform, "SentinelAura");
            VorgFx.Sparkles(aura, "VioletEnergy", art.MagicSpark, Vector3.up * 0.2f, 1.25f, 12f, 0.24f, 1.25f, 30);
            var light = new GameObject("SentinelLight").AddComponent<Light>();
            light.transform.SetParent(aura, false);
            light.transform.localPosition = new Vector3(0f, 2.3f, 0.9f);
            light.type = LightType.Point;
            light.color = VorgArt.Hex("#B98CFF");
            light.range = 6f;
            light.intensity = 2.4f;
            light.shadows = LightShadows.None;
            return Finish(root, visual, body, definition, art, new Object[0], new Object[0], 0.03f, animator);
        }

        private static void BuildConeLayer(Transform parent, string name, float height, Material material, out MeshFilter filter, out MeshRenderer renderer)
        {
            var cone = new GameObject(name);
            cone.transform.SetParent(parent, false);
            cone.transform.localPosition = Vector3.up * height;
            filter = cone.AddComponent<MeshFilter>();
            renderer = cone.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        private static EnemyBrain Finish(GameObject root, Transform visual, Transform body, EnemyDefinition definition, VorgArt art, Object[] groupA, Object[] groupB, float bob, Animator modelAnimator = null, System.Action<EnemyBrain> wireModel = null)
        {
            var effects = Kit.Group(root.transform, "Effects");
            VorgFx.ContactShadow(effects, art, 4.2f, true);
            var spawnDisc = Kit.Cylinder(effects, "WarningDisc", Vector3.up * 0.04f, new Vector3(1f, 0.01f, 1f), art.Warning);
            var discs = new Object[TelegraphDiscs];
            var discBacks = new Object[TelegraphDiscs];
            for (var i = 0; i < discs.Length; i++)
            {
                var back = Kit.Cylinder(effects, $"TelegraphArea{i}", Vector3.up * 0.055f, new Vector3(1f, 0.01f, 1f), art.WarningSoft);
                var disc = Kit.Cylinder(effects, $"TelegraphDisc{i}", Vector3.up * 0.07f, new Vector3(1f, 0.01f, 1f), art.Warning);
                VorgCombatBuilder.NoShadow(back, disc);
                discBacks[i] = back.transform;
                discs[i] = disc.transform;
            }

            var lineBack = Kit.Box(effects, "TelegraphLineArea", Vector3.up * 0.055f, new Vector3(1f, 0.01f, 1f), art.WarningSoft);
            var line = Kit.Box(effects, "TelegraphLine", Vector3.up * 0.07f, new Vector3(1f, 0.01f, 1f), art.Warning);
            BuildConeLayer(effects, "TelegraphConeArea", 0.055f, art.WarningSoft, out var coneBackFilter, out var coneBackRenderer);
            BuildConeLayer(effects, "TelegraphCone", 0.07f, art.Warning, out var coneFilter, out var coneRenderer);
            VorgCombatBuilder.NoShadow(spawnDisc, line, lineBack);

            var health = root.AddComponent<Health>();
            Kit.Wire(health, ("maxHealth", definition.MaxHealth), ("invulnerabilityAfterHit", 0f));
            var brain = root.AddComponent<EnemyBrain>();
            Kit.Wire(brain,
                ("definition", definition),
                ("targetMask", (LayerMask)(1 << VorgProjectSetup.PlayerLayer)),
                ("obstacleMask", (LayerMask)(1 << VorgProjectSetup.WorldLayer)));
            Kit.Wire(root.AddComponent<EnemyFeedback>(), ("brain", brain), ("visual", visual), ("warningDisc", spawnDisc.transform), ("aimLength", 0f));
            Kit.Wire(root.AddComponent<BossTelegraphView>(),
                ("brain", brain), ("discs", discs), ("discBacks", discBacks),
                ("line", line.transform), ("lineBack", lineBack.transform),
                ("coneFilter", coneFilter), ("coneRenderer", coneRenderer),
                ("coneBackFilter", coneBackFilter), ("coneBackRenderer", coneBackRenderer));
            Kit.Wire(root.AddComponent<ProceduralLimbs>(), ("body", body), ("groupA", groupA), ("groupB", groupB), ("stepsPerMeter", 0.9f), ("swingDegrees", 20f), ("bobHeight", bob));
            if (modelAnimator != null) Kit.Wire(root.AddComponent<BossModelAnimator>(), ("animator", modelAnimator), ("brain", brain));
            wireModel?.Invoke(brain);
            var flash = root.AddComponent<HitFlash>();
            Kit.Wire(flash, ("health", health), ("visualRoot", visual), ("flashMaterial", art.HitFlash));
            Kit.Wire(root.AddComponent<OcclusionFade>(), ("brain", brain), ("flash", flash), ("ghostMaterial", art.Ghost), ("halfWidth", 1.75f), ("height", wireModel != null ? 4.2f : 3.5f));

            Kit.SetLayer(root, VorgProjectSetup.EnemyLayer);
            return Kit.SaveAsPrefab<EnemyBrain>(root, $"{Folder}/{root.name}.prefab");
        }
    }
}
