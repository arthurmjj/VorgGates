using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace VorgGates.EditorTools
{
    /// <summary>Windows player builds. The smoke build adds an automated run; the release build never contains it.</summary>
    public static class VorgBuildTools
    {
        public const string ReleasePath = "Builds/Release/Windows/VorgGates.exe";
        public const string SmokePath = "Builds/Smoke/VorgGatesSmoke.exe";
        private const string SmokeDefine = "VORG_BUILD_SMOKE";

        [MenuItem("Vorg'Gates/Build Windows Release")]
        public static void BuildReleaseMenu() => Debug.Log(BuildRelease());

        public static string BuildRelease() => Build(ReleasePath, null);

        public static string BuildSmoke() => Build(SmokePath, new[] { SmokeDefine });

        private static string Build(string outputPath, string[] defines)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/_Project/Scenes/Game.unity" },
                locationPathName = outputPath,
                target = BuildTarget.StandaloneWindows64,
                targetGroup = BuildTargetGroup.Standalone,
                options = BuildOptions.None,
                extraScriptingDefines = defines
            };

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;
            var megabytes = summary.totalSize / (1024f * 1024f);
            return $"result={summary.result}; errors={summary.totalErrors}; warnings={summary.totalWarnings}; sizeMB={megabytes:F1}; seconds={summary.totalTime.TotalSeconds:F0}; path={Path.GetFullPath(outputPath)}; exists={File.Exists(outputPath)}";
        }
    }
}
