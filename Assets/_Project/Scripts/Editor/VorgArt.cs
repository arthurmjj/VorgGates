using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VorgGates.EditorTools
{
    internal enum VorgTheme
    {
        Crypt,
        Forge,
        Sanctuary
    }

    /// <summary>Shared URP materials of one phase. Every color comes from the GDD art bible.</summary>
    internal sealed class VorgArt
    {
        private const string Folder = "Assets/_Project/Art/Materials";

        private readonly Dictionary<string, Material> kitMaterials = new Dictionary<string, Material>();

        public readonly VorgTheme Theme;
        public readonly string Suffix;
        public readonly Color TorchColor;
        public readonly Color PortalColor;
        public readonly Material FloorA;
        public readonly Material FloorB;
        public readonly Material FloorC;
        public readonly Material Grout;
        public readonly Material WallA;
        public readonly Material WallB;
        public readonly Material WallC;
        public readonly Material Trim;
        public readonly Material Terracotta;
        public readonly Material TerracottaDark;
        public readonly Material Parchment;
        public readonly Material Moss;
        public readonly Material MossDark;
        public readonly Material Abyss;
        public readonly Material Gold;
        public readonly Material Steel;
        public readonly Material DarkSteel;
        public readonly Material Wood;
        public readonly Material WoodDark;
        public readonly Material Skin;
        public readonly Material Flame;
        public readonly Material Ember;
        public readonly Material Ether;
        public readonly Material RuneGold;
        public readonly Material Lava;
        public readonly Material Crystal;
        public readonly Material PortalSwirl;
        public readonly Material HealGlow;
        public readonly Material HitFlash;
        public readonly Material Slash;
        public readonly Material Warning;
        public readonly Material WarningSoft;
        public readonly Material Ghost;
        public readonly Material Shadow;
        public readonly Material EnemyGlow;
        public readonly Material LightBeam;
        public readonly Material GoldHalo;
        public readonly Material ArchTrim;
        public readonly Material ArchFloor;
        public readonly Material ArchBeam;
        public readonly Material ArchWindow;
        public readonly Material CoffinStone;
        public readonly Material SentinelArmor;
        public readonly Material GoldSpark;
        public readonly Material MagicSpark;
        public readonly Material Mist;

        public VorgArt(VorgTheme theme)
        {
            Theme = theme;
            Suffix = theme == VorgTheme.Crypt ? string.Empty : $"_{theme}";
            TorchColor = Hex(Pick("#FFB066", "#FF6A2A", "#A86BFF"));
            PortalColor = Hex(Pick("#3DDAD7", "#FF8A3C", "#B98CFF"));

            var stone = VorgTextureLibrary.Stone;
            var floor = VorgTextureLibrary.Floor;
            var wood = VorgTextureLibrary.Wood;
            var metal = VorgTextureLibrary.Metal;

            FloorA = Lit("FloorA", Pick("#6A6A70", "#47464C", "#4B4A5C"), 0.16f, 0f, floor);
            FloorB = Lit("FloorB", Pick("#5B5C63", "#3B3A40", "#3F3E50"), 0.12f, 0f, floor);
            FloorC = Lit("FloorC", Pick("#74736F", "#525054", "#575669"), 0.18f, 0f, floor);
            Grout = Lit("Grout", Pick("#1F2233", "#120E0C", "#09080F"), 0.02f);
            WallA = Lit("WallA", Pick("#7C7D80", "#58565C", "#5B5870"), 0.1f, 0f, stone);
            WallB = Lit("WallB", Pick("#6A6C72", "#47464C", "#4A4760"), 0.08f, 0f, stone);
            WallC = Lit("WallC", Pick("#8D8C88", "#69655F", "#6E6A86"), 0.12f, 0f, stone);
            Trim = Lit("Trim", Pick("#9C9A92", "#625C58", "#8A84A8"), 0.18f, 0f, stone);
            Terracotta = Lit("Terracotta", Pick("#B8362F", "#A83D22", "#6B3FA8"), 0.22f);
            TerracottaDark = Lit("TerracottaDark", Pick("#7A1F24", "#5C2518", "#3B2360"), 0.14f);
            Parchment = Lit("Parchment", Pick("#E8D5A8", "#C9B28C", "#D8D2E8"), 0.2f);
            Moss = Lit("Moss", Pick("#6F9A52", "#B0622F", "#BDB5A2"), 0.14f);
            MossDark = Lit("MossDark", Pick("#3F6B3C", "#633421", "#756E63"), 0.1f);
            Abyss = Lit("Abyss", Pick("#1F2233", "#15100E", "#0D0B14"), 0.05f);
            Gold = Lit("Gold", Pick("#F2C14E", "#E8A23A", "#D9C07A"), 0.62f, 0.75f, metal);
            Steel = Lit("Steel", Pick("#B3B7C4", "#77757A", "#7F7D98"), 0.62f, 0.8f, metal);
            DarkSteel = Lit("DarkSteel", Pick("#4C505C", "#4A484E", "#403C58"), 0.5f, 0.85f, metal);
            Wood = Lit("Wood", Pick("#8A5E3E", "#4F3A2C", "#3B3340"), 0.14f, 0f, wood);
            WoodDark = Lit("WoodDark", Pick("#553A2A", "#2E211A", "#231F2A"), 0.1f, 0f, wood);
            Skin = Lit("Skin", Pick("#D9A77E", "#B98A68", "#A79DBB"), 0.2f);
            Flame = Emissive("Flame", Pick("#FF9A3C", "#FF5A1E", "#A45CFF"), 3.2f);
            Ember = Emissive("Ember", Pick("#E0703A", "#FF4A1E", "#C77DFF"), 2.6f);
            Ether = Emissive("Ether", Pick("#3DDAD7", "#FF8A3C", "#B98CFF"), 2.2f);
            HealGlow = Emissive("HealGlow", "#58E07A", 1.8f);
            RuneGold = Animated("RuneGold", Pick("#6B4A16", "#6A2408", "#2E1C66"), Pick("#F2C14E", "#FF9A3C", "#B48CFF"), 1.5f, 0.1f, 1.1f, 0.3f);
            Lava = Animated("Lava", Pick("#5A1E08", "#3A0A03", "#2A1050"), Pick("#FF7A24", "#FF5A14", "#C77DFF"), 1.35f, 0.22f, 0.95f, 0.1f);
            Crystal = Animated("Crystal", Pick("#2C7A6C", "#7A2C10", "#3A1C8C"), Pick("#8CF2DC", "#FFB46A", "#D9A8FF"), 1.7f, 0.12f, 2.2f, 0.22f);
            PortalSwirl = Animated("PortalSwirl", Pick("#0D4E58", "#6A2008", "#2E1470"), Pick("#7CF7EE", "#FFC36A", "#D6B0FF"), 2.2f, 0.65f, 1.5f, 0.12f);
            HitFlash = Emissive("HitFlash", "#D98A6A", 0.75f);
            Slash = Transparent("Slash", "#E8D5A8", 0.5f);
            Warning = Transparent("Warning", Pick("#FF6A2A", "#FFD23C", "#FF4FD8"), 0.68f);
            WarningSoft = Transparent("WarningSoft", Pick("#FF6A2A", "#FFD23C", "#FF4FD8"), 0.2f);
            Ghost = Transparent("Ghost", Pick("#A9B2C8", "#D08A5C", "#A98CE8"), 0.3f);
            Shadow = Transparent("ContactShadow", "#05050A", 0.62f, VorgTextureLibrary.Soft);
            EnemyGlow = Transparent("EnemyGlow", Pick("#FFD9A0", "#FFC27A", "#FFB9A0"), Theme == VorgTheme.Crypt ? 0.08f : 0.26f, VorgTextureLibrary.Soft, true);
            LightBeam = Transparent("LightBeam", "#FFD777", 0.42f, VorgTextureLibrary.Beam, true);
            GoldHalo = Transparent("GoldHalo", "#FFC95A", 0.5f, VorgTextureLibrary.Soft, true);
            CoffinStone = Lit("ArchCoffin", "#D8D4C8", 0.12f, 0f, VorgArchitecture.Texture("Crypt/Textures/Coffin_Diffuse_256x256.png"));
            var knightMap = VorgArchitecture.Texture("Knight/Textures/knightlowres.png");
            SentinelArmor = Lit("SentinelArmor", "#8E7FD8", 0.55f, 0.55f, knightMap);
            SentinelArmor.EnableKeyword("_EMISSION");
            SentinelArmor.SetTexture("_EmissionMap", knightMap);
            SentinelArmor.SetColor("_EmissionColor", Hex("#8A52FF") * 0.42f);
            SentinelArmor.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            switch (theme)
            {
                case VorgTheme.Forge:
                    ArchTrim = Lit("ArchPlate", "#5A5A60", 0.4f, 0.45f, VorgTextureLibrary.Plate);
                    ArchFloor = Lit("ArchFloor", "#6E6C70", 0.32f, 0.3f, VorgTextureLibrary.Plate);
                    ArchBeam = Lit("ArchBeam", "#85807A", 0.46f, 0.6f, metal);
                    ArchWindow = ArchTrim;
                    break;
                case VorgTheme.Sanctuary:
                    var gothic = VorgArchitecture.Texture("Cathedral/Textures/trim_sheet_512x512.png");
                    ArchTrim = Lit("ArchStone", "#9A94C4", 0.16f, 0f, gothic);
                    ArchFloor = Lit("ArchFloor", "#8C86B4", 0.3f, 0f, gothic);
                    ArchBeam = ArchTrim;
                    ArchWindow = Lit("ArchWindow", "#FFFFFF", 0.3f, 0f, VorgArchitecture.Texture("Cathedral/Textures/window_diffuse_512x512.png"));
                    ArchWindow.EnableKeyword("_EMISSION");
                    ArchWindow.SetTexture("_EmissionMap", VorgArchitecture.Texture("Cathedral/Textures/window_emission_512x512.png"));
                    ArchWindow.SetColor("_EmissionColor", Hex("#C39BFF") * 3f);
                    ArchWindow.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
                    break;
                default:
                    var mossy = VorgArchitecture.Texture("Crypt/Textures/TrimSheet_Mossy_512x512.png");
                    ArchTrim = Lit("ArchStone", "#E9E7DE", 0.12f, 0f, mossy);
                    ArchFloor = Lit("ArchFloor", "#D4D3CB", 0.16f, 0f, mossy);
                    ArchBeam = ArchTrim;
                    ArchWindow = ArchTrim;
                    break;
            }

            GoldSpark = ParticleMaterial("GoldSpark", "#FFD777", VorgExternalAssets.Particle("light_01"), true);
            MagicSpark = ParticleMaterial("MagicSpark", Pick("#BDF5D8", "#FF9A4A", "#C99CFF"), VorgExternalAssets.Particle("magic_02"), true);
            Mist = ParticleMaterial("Mist", Pick("#A8B6A8", "#6A564C", "#6C5FA0"), VorgExternalAssets.Particle("smoke_04"), false);
        }

        public static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out var color) ? color : Color.magenta;

        public Material KitMaterial(string kit)
        {
            if (kitMaterials.TryGetValue(kit, out var material)) return material;
            material = Lit($"Kenney{kit}", Pick("#E8E6E0", "#8E8A88", "#6E6A88"), 0.14f, 0f, VorgExternalAssets.Colormap(kit));
            kitMaterials[kit] = material;
            return material;
        }

        public Material NatureMaterial(string slot)
        {
            var name = slot.ToLowerInvariant();
            if (name.Contains("leaf")) return MossDark;
            if (name.Contains("grass")) return Moss;
            if (name.Contains("dirt")) return FloorC;
            if (name.Contains("stone") || name.Contains("rock")) return WallC;
            if (name.Contains("inner") || name.Contains("tan")) return Parchment;
            if (name.Contains("wood")) return Wood;
            if (name.Contains("red")) return Terracotta;
            if (name.Contains("purple")) return Crystal;
            return WallB;
        }

        /// <summary>Blackened forge armor for the Executioner, with heat where the source art had stains.</summary>
        public Material ExecutionerMaterial(string part)
        {
            const string folder = "UndeadKnight/Textures";
            string tint;
            float smoothness, metallic, glow;
            switch (part)
            {
                case "Armor": tint = "#77757E"; smoothness = 0.5f; metallic = 0.42f; glow = 3.4f; break;
                case "Rings": tint = "#6A6870"; smoothness = 0.4f; metallic = 0.5f; glow = 0f; break;
                case "Cape": tint = "#D8452A"; smoothness = 0.12f; metallic = 0f; glow = 3f; break;
                case "Cloth": tint = "#B23C20"; smoothness = 0.1f; metallic = 0f; glow = 0f; break;
                case "Leather": tint = "#6E5A50"; smoothness = 0.24f; metallic = 0f; glow = 0f; break;
                case "Claymore_Blade": tint = "#8A8890"; smoothness = 0.6f; metallic = 0.6f; glow = 2.6f; break;
                default: tint = "#4A4340"; smoothness = 0.4f; metallic = 0.6f; glow = 0f; break;
            }

            var material = Lit($"Executioner_{part}", tint, smoothness, metallic, VorgArchitecture.Texture($"{folder}/{part}_baseColor.png"));
            material.SetTexture("_BumpMap", VorgArchitecture.Texture($"{folder}/{part}_normal.png"));
            material.EnableKeyword("_NORMALMAP");
            if (glow > 0f)
            {
                material.EnableKeyword("_EMISSION");
                material.SetTexture("_EmissionMap", VorgArchitecture.Texture($"{folder}/{part}_emission.png"));
                material.SetColor("_EmissionColor", Color.white * glow);
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        public Material Particle(string hex) => ParticleMaterial("Particle", hex, VorgTextureLibrary.Soft, true);

        private string Pick(string crypt, string forge, string sanctuary) =>
            Theme == VorgTheme.Crypt ? crypt : Theme == VorgTheme.Forge ? forge : sanctuary;

        private string AssetName(string name) => Theme == VorgTheme.Crypt ? $"M_{name}" : $"M_{Theme}_{name}";

        private Material Lit(string name, string hex, float smoothness, float metallic = 0f, Texture2D map = null)
        {
            var material = GetOrCreate(AssetName(name), "Universal Render Pipeline/Lit");
            material.SetColor("_BaseColor", Hex(hex));
            material.SetFloat("_Smoothness", smoothness);
            material.SetFloat("_Metallic", metallic);
            material.SetTexture("_BaseMap", map);
            EditorUtility.SetDirty(material);
            return material;
        }

        private Material Emissive(string name, string hex, float intensity)
        {
            var material = GetOrCreate(AssetName(name), "Universal Render Pipeline/Lit");
            var color = Hex(hex);
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", 0.1f);
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * intensity);
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            EditorUtility.SetDirty(material);
            return material;
        }

        private Material Animated(string name, string dark, string hot, float intensity, float speed, float scale, float pulse)
        {
            var material = GetOrCreate(AssetName(name), "VorgGates/AnimatedEmissive");
            material.shader = Shader.Find("VorgGates/AnimatedEmissive");
            material.shaderKeywords = new string[0];
            material.SetColor("_ColorA", Hex(dark));
            material.SetColor("_ColorB", Hex(hot) * intensity);
            material.SetFloat("_Speed", speed);
            material.SetFloat("_Scale", scale);
            material.SetFloat("_Pulse", pulse);
            material.renderQueue = -1;
            EditorUtility.SetDirty(material);
            return material;
        }

        private Material Transparent(string name, string hex, float alpha, Texture2D map = null, bool additive = false)
        {
            var material = GetOrCreate(AssetName(name), "Universal Render Pipeline/Unlit");
            var color = Hex(hex);
            color.a = alpha;
            material.SetColor("_BaseColor", color);
            material.SetTexture("_BaseMap", map);
            ConfigureTransparent(material, additive);
            EditorUtility.SetDirty(material);
            return material;
        }

        private Material ParticleMaterial(string name, string hex, Texture2D map, bool additive)
        {
            var material = GetOrCreate(AssetName(name), "Universal Render Pipeline/Particles/Unlit");
            material.SetColor("_BaseColor", Hex(hex));
            material.SetTexture("_BaseMap", map);
            ConfigureTransparent(material, additive);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void ConfigureTransparent(Material material, bool additive)
        {
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", additive ? 2f : 0f);
            material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)(additive ? UnityEngine.Rendering.BlendMode.One : UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha));
            material.SetFloat("_ZWrite", 0f);
            material.SetFloat("_Cull", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

        private static Material GetOrCreate(string name, string shaderName)
        {
            var path = $"{Folder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            material = new Material(Shader.Find(shaderName)) { name = name };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }
    }
}
