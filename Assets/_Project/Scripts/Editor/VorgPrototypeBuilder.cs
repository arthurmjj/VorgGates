using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using VorgGates.App;
using VorgGates.Core;
using VorgGates.Gameplay;
using VorgGates.Presentation;

namespace VorgGates.EditorTools
{
    /// <summary>Rebuilds the game data, prefabs and scene from code.</summary>
    public static class VorgPrototypeBuilder
    {
        private const string Root = "Assets/_Project";
        private const string ScenePath = Root + "/Scenes/Game.unity";
        private const float CameraSize = 6.9f;

        [MenuItem("Vorg'Gates/Rebuild Game")]
        public static void Build()
        {
            VorgProjectSetup.Apply();
            EnsureFolders();

            // Opening a scene unloads unused assets, so it must happen before any asset is referenced.
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.skybox = null;
            RenderSettings.fog = false;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            Kit.ResetMeshFolder();
            VorgMeshLibrary.Prepare();
            VorgTextureLibrary.Prepare();
            VorgExternalAssets.Prepare();
            VorgIconLibrary.Prepare();
            VorgArchitecture.Prepare();
            VorgBossModels.Prepare();
            VorgExecutionerModel.Prepare();

            var crypt = new VorgArt(VorgTheme.Crypt);
            var forge = new VorgArt(VorgTheme.Forge);
            var sanctuary = new VorgArt(VorgTheme.Sanctuary);

            var magicBolt = VorgCombatBuilder.BuildMagicBolt(crypt);
            var arrow = VorgCombatBuilder.BuildArrow(crypt);
            var weapons = CreateWeapons(magicBolt);
            var crawler = CreateEnemy("E_Rastejante", EnemyKind.Crawler, EnemyBehavior.MeleeLunge, "Rastejante", 40, 10, 3.6f, 1.9f, 0f, 0.4f, 2.2f, 0.14f, 1.6f, 0.75f, 0f, null, 0f);
            var archer = CreateEnemy("E_ArqueiroOssudo", EnemyKind.Archer, EnemyBehavior.Ranged, "Arqueiro Ossudo", 30, 8, 3.1f, 10f, 7f, 0.6f, 0f, 0.14f, 1f, 1.4f, 0.1f, arrow, 11f);
            var brute = CreateEnemy("E_Bruto", EnemyKind.Brute, EnemyBehavior.Charger, "Bruto", 120, 20, 2.3f, 6.5f, 0f, 0.9f, 7f, 0.42f, 1.6f, 1.7f, 0.75f, null, 0f);
            var guardian = CreateBoss("E_GuardiaoDaCripta", "Guardião da Cripta", 420, 2.1f, 3.9f, magicBolt, VorgBossBuilder.GuardianPatterns());
            var executioner = CreateBoss("E_CarrascoDaForja", "Carrasco da Forja", 640, 2.6f, 4f, VorgCombatBuilder.BuildFireBolt(forge), VorgBossBuilder.ExecutionerPatterns());
            var sentinel = CreateBoss("E_SentinelaDeVorg", "Sentinela de Vorg", 900, 2.8f, 4.5f, VorgCombatBuilder.BuildVoidBolt(sanctuary), VorgBossBuilder.SentinelPatterns());
            AssetDatabase.SaveAssets();

            var playerPrefab = VorgCharacterBuilder.BuildPlayer(crypt, weapons);
            var rewardPickup = VorgCombatBuilder.BuildRewardPickup(crypt);
            var healOrb = VorgCombatBuilder.BuildHealOrb(crypt);
            var sword = VorgCombatBuilder.BuildLegendarySword(crypt);

            var cryptKit = BuildKit(crypt, crawler, archer, brute, VorgBossBuilder.BuildGuardian(crypt, guardian));
            var forgeKit = BuildKit(forge, crawler, archer, brute, VorgBossBuilder.BuildExecutioner(forge, executioner));
            var sanctuaryKit = BuildKit(sanctuary, crawler, archer, brute, VorgBossBuilder.BuildSentinel(sanctuary, sentinel));

            var campaign = new[]
            {
                CreateDungeon("D_CriptaDeMusgo", "Cripta de Musgo", cryptKit, 6, 2, 1, 4, 2, 0, WeaponKind.Axe, 1f, 1f, false),
                CreateDungeon("D_ForjaAfundada", "Forja Afundada", forgeKit, 7, 3, 1, 5, 3, 0, WeaponKind.Staff, 1.2f, 1.1f, false),
                CreateDungeon("D_SantuarioDeCinzas", "Santuário de Cinzas", sanctuaryKit, 7, 3, 1, 5, 3, 1, WeaponKind.Staff, 1.35f, 1.2f, true)
            };
            AssetDatabase.SaveAssets();

            var atmospheres = new[]
            {
                CreateAtmosphere("A_Cripta", crypt, "#7F88A6", "#565A72", "#34344A", "#C9D4F0", 1.05f, new Vector3(58f, 118f, 0f), "#171926", 0.35f, 10f, 12f, new Color(1f, 1f, 0.97f), 0.28f),
                CreateAtmosphere("A_Forja", forge, "#55555F", "#3A3940", "#1C191B", "#B9BCC8", 0.6f, new Vector3(64f, 132f, 0f), "#0D0807", 0.25f, 14f, 8f, Color.white, 0.32f),
                CreateAtmosphere("A_Santuario", sanctuary, "#77749E", "#524E76", "#28253E", "#CFCAF2", 1f, new Vector3(52f, 96f, 0f), "#05040A", 0.3f, 12f, 6f, new Color(0.98f, 0.97f, 1f), 0.34f)
            };

            var player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, scene);
            player.transform.SetPositionAndRotation(Vector3.up * 0.05f, Quaternion.Euler(0f, 45f, 0f));

            var cameraRig = BuildCamera(player.transform, out var camera);
            var light = BuildLight();
            var volume = BuildVolume(atmospheres[0].Volume);
            var hud = BuildHud();
            var menu = hud.gameObject.AddComponent<MenuController>();
            var audioLibrary = VorgAudioBuilder.Build();

            var input = AssetDatabase.LoadAssetAtPath<InputActionAsset>($"{Root}/Input/VorgGatesControls.inputactions");
            Kit.Wire(player.GetComponent<PlayerInputReader>(), ("actions", input), ("aimCamera", camera));
            PrefabUtility.RecordPrefabInstancePropertyModifications(player.GetComponent<PlayerInputReader>());

            var systems = new GameObject("[Game]");
            var navMesh = systems.AddComponent<RuntimeNavMesh>();
            Kit.Wire(navMesh, ("sourceMask", (LayerMask)(1 << VorgProjectSetup.WorldLayer)));
            var builder = systems.AddComponent<DungeonBuilder>();
            Kit.Wire(builder, ("rewardPrefab", rewardPickup), ("healOrbPrefab", healOrb), ("swordPrefab", sword));
            var projectiles = new GameObject("Projectiles").AddComponent<ProjectileSystem>();
            projectiles.transform.SetParent(systems.transform);
            var director = systems.AddComponent<DungeonDirector>();
            Kit.Wire(director, ("campaign", (Object[])campaign), ("builder", builder), ("navMesh", navMesh), ("projectiles", projectiles));
            var theme = new GameObject("ThemeDirector").AddComponent<ThemeDirector>();
            theme.transform.SetParent(systems.transform);
            Kit.Wire(theme, ("atmospheres", (Object[])atmospheres), ("mainLight", light), ("gameCamera", camera), ("volume", volume), ("follow", player.transform));
            var audioObject = new GameObject("Audio");
            audioObject.transform.SetParent(systems.transform);
            var audioDirector = audioObject.AddComponent<AudioDirector>();
            Kit.Wire(audioDirector, ("library", audioLibrary));
            Kit.Wire(systems.AddComponent<GameBootstrap>(),
                ("player", player.GetComponent<PlayerStateMachine>()),
                ("director", director),
                ("cameraRig", cameraRig),
                ("themeDirector", theme),
                ("hud", hud),
                ("menu", menu),
                ("audioDirector", audioDirector));

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true),
                new EditorBuildSettingsScene("Assets/Scenes/SampleScene.unity", false)
            };
            AssetDatabase.SaveAssets();
            Debug.Log("Vorg'Gates: game rebuilt.");
        }

        private static DungeonKit BuildKit(VorgArt art, EnemyDefinition crawler, EnemyDefinition archer, EnemyDefinition brute, EnemyBrain boss)
        {
            var tallDoor = VorgPropBuilder.BuildDoor(art, "Door_Tall", 3f);
            var lowDoor = VorgPropBuilder.BuildDoor(art, "Door_Low", 0.95f);
            var portal = VorgPropBuilder.BuildPortal(art);
            var torch = VorgPropBuilder.BuildTorch(art);
            var rooms = VorgRoomBuilder.Build(art, torch);
            var enemies = new[]
            {
                VorgCharacterBuilder.BuildCrawler(art, crawler),
                VorgCombatBuilder.BuildArcher(art, archer),
                VorgCombatBuilder.BuildBrute(art, brute),
                boss
            };

            var kit = GetOrCreateAsset<DungeonKit>($"{Root}/Data/Dungeons/K_{art.Theme}.asset");
            Kit.Wire(kit,
                ("roomPrefab", rooms.Room),
                ("corridorPrefab", rooms.Corridor),
                ("entranceInterior", rooms.Entrance),
                ("exitInterior", rooms.Exit),
                ("rewardInterior", rooms.Reward),
                ("bossInterior", rooms.Boss),
                ("combatInteriors", (Object[])rooms.Combat),
                ("tallDoor", tallDoor),
                ("lowDoor", lowDoor),
                ("portalPrefab", portal),
                ("enemyPrefabs", (Object[])enemies));
            return kit;
        }

        private static void EnsureFolders()
        {
            foreach (var path in new[]
                     {
                         "Art", "Art/Materials", "Art/Meshes", "Data", "Data/Weapons", "Data/Enemies", "Data/Dungeons", "Data/Atmospheres",
                         "Prefabs", "Prefabs/Player", "Prefabs/Enemies", "Prefabs/Props", "Prefabs/Rooms", "Scenes", "Settings"
                     })
            {
                var full = $"{Root}/{path}";
                if (AssetDatabase.IsValidFolder(full)) continue;
                var slash = full.LastIndexOf('/');
                AssetDatabase.CreateFolder(full.Substring(0, slash), full.Substring(slash + 1));
            }
        }

        private static WeaponDefinition[] CreateWeapons(Projectile magicBolt)
        {
            var sword = GetOrCreateAsset<WeaponDefinition>($"{Root}/Data/Weapons/W_Espada.asset");
            Kit.Wire(sword, ("kind", (int)WeaponKind.Sword), ("displayName", "Espada"), ("damage", 20), ("windup", 0.08f), ("active", 0.08f), ("recovery", 0.19f), ("range", 2.2f), ("arcDegrees", 110f), ("knockback", 7f), ("comboEvery", 3), ("comboDamage", 30));

            var axe = GetOrCreateAsset<WeaponDefinition>($"{Root}/Data/Weapons/W_Machado.asset");
            Kit.Wire(axe, ("kind", (int)WeaponKind.Axe), ("displayName", "Machado"), ("damage", 45), ("windup", 0.26f), ("active", 0.1f), ("recovery", 0.49f), ("range", 2.6f), ("arcDegrees", 170f), ("knockback", 16f));

            var staff = GetOrCreateAsset<WeaponDefinition>($"{Root}/Data/Weapons/W_Cajado.asset");
            Kit.Wire(staff, ("kind", (int)WeaponKind.Staff), ("displayName", "Cajado"), ("damage", 14), ("windup", 0.12f), ("active", 0.03f), ("recovery", 0.3f), ("range", 14f), ("arcDegrees", 10f), ("knockback", 2f), ("projectile", magicBolt), ("projectileSpeed", 18f));
            return new[] { sword, axe, staff };
        }

        private static EnemyDefinition CreateEnemy(string asset, EnemyKind kind, EnemyBehavior behavior, string displayName, int health, int damage, float speed, float range, float minRange, float telegraph, float lunge, float lungeTime, float hitRadius, float recovery, float resistance, Projectile projectile, float projectileSpeed)
        {
            var definition = GetOrCreateAsset<EnemyDefinition>($"{Root}/Data/Enemies/{asset}.asset");
            Kit.Wire(definition, ("kind", (int)kind), ("behavior", (int)behavior), ("displayName", displayName), ("maxHealth", health), ("damage", damage), ("moveSpeed", speed), ("attackRange", range), ("preferredMinRange", minRange), ("telegraph", telegraph), ("lungeDistance", lunge), ("lungeDuration", lungeTime), ("hitRadius", hitRadius), ("recovery", recovery), ("knockbackResistance", resistance), ("projectileSpeed", projectileSpeed));
            if (projectile != null) Kit.Wire(definition, ("projectile", projectile));
            return definition;
        }

        private static EnemyDefinition CreateBoss(string asset, string displayName, int health, float speed, float barHeight, Projectile projectile, BossPattern[] patterns)
        {
            var definition = CreateEnemy(asset, EnemyKind.Boss, EnemyBehavior.Boss, displayName, health, patterns[0].damage, speed, 5f, 0f, 1f, 0f, 0.2f, 1f, 1f, 1f, projectile, 10f);
            Kit.Wire(definition, ("barHeight", barHeight));
            definition.EditorSetPatterns(patterns);
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static DungeonDefinition CreateDungeon(string asset, string displayName, DungeonKit kit, int mainRooms, int minEnemies, int enemyStep, int maxEnemies, int tier, int rewardHost, WeaponKind reward, float healthScale, float damageScale, bool endsWithSword)
        {
            var dungeon = GetOrCreateAsset<DungeonDefinition>($"{Root}/Data/Dungeons/{asset}.asset");
            Kit.Wire(dungeon,
                ("displayName", displayName),
                ("kit", kit),
                ("mainPathRooms", mainRooms),
                ("sideRooms", 1),
                ("minEnemies", minEnemies),
                ("enemyStep", enemyStep),
                ("maxEnemies", maxEnemies),
                ("enemyTier", tier),
                ("rewardHostIndex", rewardHost),
                ("rewardWeapon", (int)reward),
                ("healthScale", healthScale),
                ("damageScale", damageScale),
                ("endsWithSword", endsWithSword ? 1 : 0));
            return dungeon;
        }

        private static DungeonAtmosphere CreateAtmosphere(string asset, VorgArt art, string sky, string equator, string ground, string light, float intensity, Vector3 lightEuler, string background, float exposure, float contrast, float saturation, Color filter, float vignette)
        {
            var profile = CreateVolumeProfile($"{Root}/Settings/VP_{art.Theme}.asset", exposure, contrast, saturation, filter, vignette);
            var atmosphere = GetOrCreateAsset<DungeonAtmosphere>($"{Root}/Data/Atmospheres/{asset}.asset");
            atmosphere.EditorConfigure(VorgArt.Hex(sky), VorgArt.Hex(equator), VorgArt.Hex(ground), VorgArt.Hex(light), intensity, lightEuler, VorgArt.Hex(background), profile, VorgThemeDecor.BuildAtmosphere(art));
            EditorUtility.SetDirty(atmosphere);
            return atmosphere;
        }

        private static VolumeProfile CreateVolumeProfile(string path, float exposure, float contrast, float saturation, Color filter, float vignetteAmount)
        {
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, path);
                AddOverride<Tonemapping>(profile).mode.Override(TonemappingMode.Neutral);
                var bloom = AddOverride<Bloom>(profile);
                bloom.threshold.Override(1.05f);
                bloom.intensity.Override(0.6f);
                bloom.scatter.Override(0.55f);
                bloom.highQualityFiltering.Override(false);
                AddOverride<Vignette>(profile);
                AddOverride<ColorAdjustments>(profile);
            }

            profile.TryGet<Vignette>(out var vignette);
            vignette.color.Override(VorgArt.Hex("#08080F"));
            vignette.intensity.Override(vignetteAmount);
            vignette.smoothness.Override(0.42f);
            profile.TryGet<ColorAdjustments>(out var color);
            color.postExposure.Override(exposure);
            color.contrast.Override(contrast);
            color.saturation.Override(saturation);
            color.colorFilter.Override(filter);
            EditorUtility.SetDirty(vignette);
            EditorUtility.SetDirty(color);
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static T GetOrCreateAsset<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static CameraRig BuildCamera(Transform target, out Camera camera)
        {
            var rotation = Quaternion.Euler(35.264f, 45f, 0f);
            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            cameraObject.transform.SetPositionAndRotation(target.position - rotation * Vector3.forward * 34f, rotation);
            camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = CameraSize;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 80f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = VorgArt.Hex("#12131E");
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<CinemachineBrain>();
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;

            var rig = new GameObject("CM Isometric");
            rig.transform.SetPositionAndRotation(cameraObject.transform.position, rotation);
            var cinemachine = rig.AddComponent<CinemachineCamera>();
            cinemachine.Follow = target;
            var lens = cinemachine.Lens;
            lens.ModeOverride = LensSettings.OverrideModes.Orthographic;
            lens.OrthographicSize = CameraSize;
            lens.NearClipPlane = 0.3f;
            lens.FarClipPlane = 80f;
            cinemachine.Lens = lens;
            var composer = rig.AddComponent<CinemachinePositionComposer>();
            composer.CameraDistance = 34f;
            composer.Damping = new Vector3(0.22f, 0.22f, 0.22f);
            composer.TargetOffset = Vector3.up * 0.9f;

            var cameraRig = rig.AddComponent<CameraRig>();
            Kit.Wire(cameraRig, ("cinemachine", cinemachine));
            return cameraRig;
        }

        private static Light BuildLight()
        {
            var lightObject = new GameObject("Main Light");
            lightObject.transform.rotation = Quaternion.Euler(58f, 118f, 0f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = VorgArt.Hex("#C4D0F2");
            light.intensity = 1.2f;
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.82f;
            return light;
        }

        private static Volume BuildVolume(VolumeProfile profile)
        {
            var volumeObject = new GameObject("Global Volume");
            var volume = volumeObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = profile;
            return volume;
        }

        private static T AddOverride<T>(VolumeProfile profile) where T : VolumeComponent
        {
            var component = profile.Add<T>(true);
            component.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(component, profile);
            return component;
        }

        private static PrototypeHud BuildHud()
        {
            var settingsPath = $"{Root}/UI/PS_Game.asset";
            var settings = AssetDatabase.LoadAssetAtPath<PanelSettings>(settingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<PanelSettings>();
                AssetDatabase.CreateAsset(settings, settingsPath);
            }

            settings.themeStyleSheet = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>($"{Root}/UI/VorgTheme.tss");
            settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            settings.referenceResolution = new Vector2Int(1920, 1080);
            settings.match = 0.5f;
            EditorUtility.SetDirty(settings);

            var hudObject = new GameObject("HUD");
            var document = hudObject.AddComponent<UIDocument>();
            document.panelSettings = settings;
            document.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{Root}/UI/Hud.uxml");
            return hudObject.AddComponent<PrototypeHud>();
        }
    }
}
