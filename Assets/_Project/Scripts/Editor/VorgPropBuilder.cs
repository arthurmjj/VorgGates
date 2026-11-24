using UnityEngine;
using UnityEngine.AI;
using VorgGates.Gameplay;
using VorgGates.Presentation;

namespace VorgGates.EditorTools
{
    internal static class VorgPropBuilder
    {
        private const string PrefabFolder = "Assets/_Project/Prefabs/Props";

        public static DoorController BuildDoor(VorgArt art, string name, float height)
        {
            var root = new GameObject(name);
            var blocker = root.AddComponent<BoxCollider>();
            blocker.center = Vector3.up * 2.5f;
            blocker.size = new Vector3(4.2f, 5f, 1f);
            blocker.enabled = false;

            var visual = Kit.Group(root.transform, "Visual");
            for (var i = 0; i < 9; i++)
            {
                var x = -1.84f + i * 0.46f;
                Kit.Cylinder(visual, $"Bar{i}", new Vector3(x, height * 0.5f, 0f), new Vector3(0.13f, height * 0.5f, 0.13f), art.Steel);
                Kit.Box(visual, $"Spike{i}", new Vector3(x, height + 0.06f, 0f), new Vector3(0.11f, 0.11f, 0.11f), art.Gold, new Vector3(45f, 0f, 45f));
            }

            var rails = height > 2f ? 3 : 2;
            for (var i = 0; i < rails; i++)
                Kit.Box(visual, $"Rail{i}", new Vector3(0f, height * (i + 0.5f) / rails, 0f), new Vector3(4.05f, 0.16f, 0.2f), art.WoodDark);

            var obstacle = root.AddComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.center = Vector3.up * 1.5f;
            obstacle.size = new Vector3(4.4f, 3f, 1.2f);
            obstacle.carving = true;
            obstacle.carveOnlyStationary = false;
            obstacle.enabled = false;

            var door = root.AddComponent<DoorController>();
            Kit.Wire(door, ("visual", visual), ("blocker", blocker), ("navObstacle", obstacle), ("travel", height + 0.25f));
            Kit.SetLayer(root, VorgProjectSetup.WorldLayer);
            return Kit.SaveAsPrefab<DoorController>(root, $"{PrefabFolder}/{name}{art.Suffix}.prefab");
        }

        public static PrototypePortal BuildPortal(VorgArt art)
        {
            const float ringRadius = 1.72f;
            const int segments = 16;
            var center = new Vector3(0f, 2.05f, 0f);

            var root = new GameObject("Portal");
            var trigger = root.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.center = Vector3.up * 1.5f;
            trigger.size = new Vector3(3.6f, 3f, 2f);

            Kit.Box(root.transform, "StepLow", Vector3.up * 0.1f, new Vector3(4.6f, 0.2f, 2.2f), art.WallB);
            Kit.Box(root.transform, "StepHigh", Vector3.up * 0.28f, new Vector3(3.9f, 0.18f, 1.6f), art.Trim);

            var ring = Kit.Group(root.transform, "Ring", center);
            for (var i = 0; i < segments; i++)
            {
                var angle = i * 360f / segments;
                var radians = angle * Mathf.Deg2Rad;
                var position = new Vector3(Mathf.Sin(radians), Mathf.Cos(radians), 0f) * ringRadius;
                var material = i == 0 ? art.Gold : i % 2 == 0 ? art.WallC : art.WallA;
                var size = i == 0 ? new Vector3(0.78f, 0.62f, 0.72f) : new Vector3(0.72f, 0.5f, 0.56f);
                Kit.Box(ring, $"Stone{i}", position, size, material, new Vector3(0f, 0f, -angle));
            }

            Kit.Cylinder(root.transform, "Seal", center + Vector3.forward * 0.08f, new Vector3(3.1f, 0.03f, 3.1f), art.Abyss, new Vector3(90f, 0f, 0f));
            for (var i = 0; i < 6; i++)
            {
                var radians = i * Mathf.PI / 3f;
                var position = center + new Vector3(Mathf.Sin(radians), Mathf.Cos(radians), 0f) * 1.05f + Vector3.back * 0.03f;
                Kit.Box(root.transform, $"Rune{i}", position, new Vector3(0.14f, 0.3f, 0.03f), art.RuneGold, new Vector3(0f, 0f, -i * 60f));
            }

            var active = Kit.Group(root.transform, "ActiveVisual");
            var swirl = Kit.Cylinder(active, "Swirl", center + Vector3.back * 0.08f, new Vector3(2.95f, 0.025f, 2.95f), art.PortalSwirl, new Vector3(90f, 0f, 0f));
            for (var i = 0; i < 3; i++)
            {
                var spoke = Kit.Box(swirl.transform, $"Spoke{i}", Vector3.up * 1.2f, new Vector3(0.12f, 0.8f, 0.72f), art.Parchment, new Vector3(0f, i * 60f, 0f));
                spoke.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            swirl.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var lightObject = new GameObject("PortalLight");
            lightObject.transform.SetParent(active, false);
            lightObject.transform.localPosition = center + Vector3.back * 1.3f;
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = art.PortalColor;
            light.range = 9f;
            light.intensity = 3.2f;
            light.shadows = LightShadows.None;
            VorgFx.Sparkles(active, "PortalSparks", art.MagicSpark, center + Vector3.down * 1.8f, 1.5f, 8f, 0.26f, 1.1f, 20);

            var portal = root.AddComponent<PrototypePortal>();
            Kit.Wire(portal, ("activeVisual", active.gameObject));
            Kit.Wire(root.AddComponent<PortalVisual>(), ("swirl", swirl.transform));
            Kit.SetLayer(root, VorgProjectSetup.InteractableLayer);
            return Kit.SaveAsPrefab<PrototypePortal>(root, $"{PrefabFolder}/Portal{art.Suffix}.prefab");
        }

        public static GameObject BuildTorch(VorgArt art)
        {
            var root = new GameObject("Torch");
            Kit.Box(root.transform, "WallPlate", new Vector3(0f, 0f, 0.05f), new Vector3(0.28f, 0.42f, 0.08f), art.WoodDark);
            Kit.Box(root.transform, "Arm", new Vector3(0f, 0f, -0.16f), new Vector3(0.09f, 0.09f, 0.4f), art.Steel);
            Kit.Cylinder(root.transform, "Handle", new Vector3(0f, 0.2f, -0.34f), new Vector3(0.12f, 0.3f, 0.12f), art.Wood);
            Kit.Cylinder(root.transform, "Bowl", new Vector3(0f, 0.5f, -0.34f), new Vector3(0.34f, 0.08f, 0.34f), art.Steel);
            var flame = Kit.Group(root.transform, "Flame", new Vector3(0f, 0.56f, -0.34f));
            Kit.Ball(flame, "Outer", new Vector3(0f, 0.2f, 0f), new Vector3(0.28f, 0.48f, 0.28f), art.Flame).GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Kit.Ball(flame, "Inner", new Vector3(0f, 0.15f, 0f), new Vector3(0.15f, 0.28f, 0.15f), art.Parchment).GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            var lightObject = new GameObject("TorchLight");
            lightObject.transform.SetParent(root.transform, false);
            lightObject.transform.localPosition = new Vector3(0f, 0.8f, -0.8f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = art.TorchColor;
            light.range = 10f;
            light.intensity = 3.4f;
            light.shadows = LightShadows.None;
            Kit.Wire(root.AddComponent<FlameFlicker>(), ("flameLight", light), ("flame", flame));
            return Kit.SaveAsPrefab<GameObject>(root, $"{PrefabFolder}/Torch{art.Suffix}.prefab");
        }

        public static void Barrel(Transform parent, Vector3 position, VorgArt art)
        {
            var barrel = Kit.Group(parent, "Barrel", position);
            Kit.Cylinder(barrel, "Staves", Vector3.up * 0.55f, new Vector3(0.82f, 0.55f, 0.82f), art.Wood);
            Kit.Cylinder(barrel, "Belly", Vector3.up * 0.55f, new Vector3(0.92f, 0.22f, 0.92f), art.Wood);
            Kit.Cylinder(barrel, "BandTop", Vector3.up * 0.88f, new Vector3(0.87f, 0.035f, 0.87f), art.Steel);
            Kit.Cylinder(barrel, "BandLow", Vector3.up * 0.22f, new Vector3(0.87f, 0.035f, 0.87f), art.Steel);
            Kit.Cylinder(barrel, "Lid", Vector3.up * 1.09f, new Vector3(0.7f, 0.015f, 0.7f), art.WoodDark);
            Kit.Blocker(barrel, "Collider", Vector3.up * 0.6f, new Vector3(0.95f, 1.2f, 0.95f));
        }

        public static void Crate(Transform parent, Vector3 position, float yaw, float size, VorgArt art)
        {
            var crate = Kit.Group(parent, "Crate", position, new Vector3(0f, yaw, 0f));
            Kit.Box(crate, "Body", Vector3.up * size * 0.5f, Vector3.one * size, art.Wood);
            Kit.Box(crate, "BraceA", new Vector3(0f, size * 0.5f, -size * 0.51f), new Vector3(size * 1.3f, 0.1f, 0.04f), art.WoodDark, new Vector3(0f, 0f, 45f));
            Kit.Box(crate, "BraceB", new Vector3(-size * 0.51f, size * 0.5f, 0f), new Vector3(0.04f, 0.1f, size * 1.3f), art.WoodDark, new Vector3(45f, 0f, 0f));
            Kit.Box(crate, "Top", Vector3.up * (size + 0.03f), new Vector3(size * 1.04f, 0.07f, size * 1.04f), art.WoodDark);
            Kit.Blocker(crate, "Collider", Vector3.up * size * 0.5f, Vector3.one * size);
        }

        public static void Banner(Transform parent, Vector3 position, float yaw, VorgArt art)
        {
            var banner = Kit.Group(parent, "Banner", position, new Vector3(0f, yaw, 0f));
            Kit.Cylinder(banner, "Rod", Vector3.zero, new Vector3(0.07f, 0.72f, 0.07f), art.Gold, new Vector3(0f, 0f, 90f));
            Kit.Box(banner, "Cloth", new Vector3(0f, -0.82f, 0f), new Vector3(1.15f, 1.56f, 0.05f), art.TerracottaDark);
            Kit.Box(banner, "Stripe", new Vector3(0f, -0.82f, -0.035f), new Vector3(0.82f, 1.3f, 0.02f), art.Terracotta);
            Kit.Box(banner, "SigilV", new Vector3(0f, -0.72f, -0.055f), new Vector3(0.12f, 0.72f, 0.02f), art.Gold);
            Kit.Box(banner, "SigilH", new Vector3(0f, -0.56f, -0.055f), new Vector3(0.52f, 0.11f, 0.02f), art.Gold);
            Kit.Box(banner, "TailL", new Vector3(-0.3f, -1.68f, 0f), new Vector3(0.55f, 0.55f, 0.05f), art.TerracottaDark, new Vector3(0f, 0f, 45f));
            Kit.Box(banner, "TailR", new Vector3(0.3f, -1.68f, 0f), new Vector3(0.55f, 0.55f, 0.05f), art.TerracottaDark, new Vector3(0f, 0f, 45f));
        }
    }
}
