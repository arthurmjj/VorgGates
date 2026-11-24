using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using VorgGates.App;
using VorgGates.Core;
using VorgGates.Gameplay;
using VorgGates.Presentation;

namespace VorgGates.Tests
{
    /// <summary>Plays the whole campaign in a Windows player. Compiled only into the smoke build.</summary>
    internal sealed class BuildSmokeRunner : MonoBehaviour
    {
        private const float WatchdogSeconds = 420f;
        private const float SampleSeconds = 8f;
        private const float BotTimeScale = 3f;

        private readonly List<string> lines = new List<string>();
        private GameBootstrap game;
        private bool finished;
        private int loggedErrors;
        private string firstError;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot() => new GameObject("BuildSmokeRunner").AddComponent<BuildSmokeRunner>();

        private void Awake() => Application.logMessageReceived += OnLog;

        private void OnDestroy() => Application.logMessageReceived -= OnLog;

        private void OnLog(string message, string stackTrace, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
            loggedErrors++;
            if (firstError == null) firstError = message;
        }

        private void Update()
        {
            if (!finished && Time.realtimeSinceStartup > WatchdogSeconds) Finish(false, "watchdog timeout");
        }

        private IEnumerator Start()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;
            yield return Until(() => (game = FindAnyObjectByType<GameBootstrap>()) != null && game.IsReady);
            if (finished) yield break;
            lines.Add($"resolution={Screen.width}x{Screen.height}; gpu={SystemInfo.graphicsDeviceName}; api={SystemInfo.graphicsDeviceType}");

            yield return CaptureInterface();
            if (finished || Environment.GetCommandLineArgs().Contains("-smokeUiOnly"))
            {
                Finish(true, "interface captured");
                yield break;
            }

            for (var phase = 0; phase < RunSession.DungeonCount; phase++)
            {
                yield return SampleBossFight(phase);
                if (finished) yield break;
            }

            game.Session.RestartCampaign();
            game.Player.Inventory.Refresh();
            if (!Check(game.JumpToDungeon(0, 1037), "campaign reset")) yield break;
            yield return null;
            Time.timeScale = BotTimeScale;

            yield return CompletePhase(0);
            if (finished) yield break;
            if (!Check(game.Session.DungeonIndex == 1 && game.Theme.AppliedIndex == 1, "portal led to phase 2 with its theme")) yield break;
            if (!Check(game.Player.Inventory.Owns(WeaponKind.Axe), "axe preserved through the portal")) yield break;

            var beforeDeath = game.Director.Layout.Signature;
            game.Player.SetInput(new ScriptedPlayerInput());
            game.Player.Health.SetLocked(false);
            game.Player.Inventory.Add(WeaponKind.Staff);
            game.Player.Health.ApplyDamage(new DamageInfo(9999, Vector3.forward, 0f));
            yield return Until(() => game.Flow.State == GameFlowState.Playing);
            if (finished) yield break;
            if (!Check(game.Session.DungeonIndex == 1, "death in phase 2 restarted only phase 2")) yield break;
            if (!Check(game.Director.Layout.Signature != beforeDeath, "death generated a new layout")) yield break;
            if (!Check(game.Player.Inventory.Owns(WeaponKind.Axe) && !game.Player.Inventory.Owns(WeaponKind.Staff), "death kept entry weapons and removed the new one")) yield break;
            if (!Check(game.Player.Health.Current == game.Player.Health.Max, "death restored health")) yield break;

            yield return CompletePhase(1);
            if (finished) yield break;
            if (!Check(game.Session.DungeonIndex == 2 && game.Theme.AppliedIndex == 2, "portal led to phase 3 with its theme")) yield break;
            if (!Check(game.Player.Inventory.Owns(WeaponKind.Staff), "staff preserved through the portal")) yield break;

            yield return CompletePhase(2);
            if (finished) yield break;
            if (!Check(game.Flow.State == GameFlowState.Victory, "final boss defeated and sword taken")) yield break;

            Time.timeScale = 1f;
            yield return new WaitForSeconds(1.3f);
            game.Player.SetInput(new ScriptedPlayerInput { Interact = true });
            yield return Until(() => game.Flow.State == GameFlowState.Playing);
            if (finished) yield break;
            if (!Check(game.Session.DungeonIndex == 0 && !game.Player.Inventory.Owns(WeaponKind.Axe), "victory restarted a clean campaign")) yield break;

            Finish(true, "complete");
        }

        private IEnumerator CaptureInterface()
        {
            if (!Check(game.Flow.State == GameFlowState.Menu && game.Menu.Current == MenuScreen.Main, "game opened in the main menu")) yield break;
            if (!Check(game.Audio.IsMusicPlaying, "menu music is playing")) yield break;
            yield return new WaitForSecondsRealtime(0.5f);
            yield return Capture("menu");

            game.Menu.Show(MenuScreen.Controls);
            yield return new WaitForSecondsRealtime(0.2f);
            yield return Capture("controls");
            game.Menu.Show(MenuScreen.Main);

            game.StartNewCampaign();
            if (!Check(game.Flow.State == GameFlowState.Playing, "Jogar started the campaign")) yield break;
            yield return new WaitForSecondsRealtime(0.6f);
            yield return Capture("phase_card");
            yield return new WaitForSecondsRealtime(1.6f);
            yield return Capture("hud");

            game.Hud.ShowPhaseCard(2, game.Director.DefinitionAt(2).DisplayName);
            yield return new WaitForSecondsRealtime(0.6f);
            yield return Capture("phase3_card");
            game.Hud.HidePhaseCard(0);
            yield return new WaitForSecondsRealtime(0.55f);
            game.Hud.ShowVictory(754f, 2);
            yield return new WaitForSecondsRealtime(0.2f);
            yield return Capture("victory_text");

            game.Player.SetInput(new ScriptedPlayerInput());
            game.PauseGame();
            if (!Check(game.Flow.State == GameFlowState.Paused && Time.timeScale == 0f, "pause froze the game")) yield break;
            yield return new WaitForSecondsRealtime(0.25f);
            yield return Capture("pause");
            game.ReturnToMenu();
            Check(game.Flow.State == GameFlowState.Menu && Time.timeScale == 1f, "pause returned to the menu");
        }

        private IEnumerator Capture(string name)
        {
            yield return new WaitForEndOfFrame();
            var frame = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.ChangeExtension(OutputPath(), null) + $"_{Screen.height}p_{name}.png", frame.EncodeToPNG());
            Destroy(frame);
        }

        private static float MeanLuminance(Texture2D frame)
        {
            var total = 0f;
            var count = 0;
            for (var y = frame.height / 5; y < frame.height * 4 / 5; y += 8)
            for (var x = frame.width / 5; x < frame.width * 4 / 5; x += 8)
            {
                total += frame.GetPixel(x, y).grayscale;
                count++;
            }

            return total / count;
        }

        private IEnumerator CompletePhase(int phase)
        {
            var layout = game.Director.Layout;
            var bot = new DungeonBot(game);
            yield return bot.CompleteDungeon(150f);
            if (finished) yield break;
            if (!Check(bot.ReachedExit, $"phase {phase + 1} completed (seed {layout.Seed}, rooms {layout.Rooms.Count}, visited {bot.RoomsVisited}) {bot.Failure}")) yield break;
            if (phase < RunSession.DungeonCount - 1) yield return Until(() => game.Flow.State == GameFlowState.Playing);
        }

        private IEnumerator SampleBossFight(int phase)
        {
            Check(game.JumpToDungeon(phase, 777 + phase), $"phase {phase + 1} built");
            lines.Add($"phase {phase + 1} rebuildMs={game.Director.LastBuildMilliseconds:F0}");
            yield return null;
            var node = game.Director.Layout.Rooms.First(r => r.Kind == RoomKind.Boss);
            var room = game.Director.Current.Rooms[node.Id];
            game.Player.SetInput(new ScriptedPlayerInput());
            game.Player.Health.SetLocked(true);
            game.Player.Motor.Teleport(room.transform.position + new Vector3(-2f, 0.05f, -2f), Quaternion.Euler(0f, 45f, 0f));
            yield return Until(() => room.State == RoomState.Active && room.Enemies.Count == 1 && room.Enemies[0].State != EnemyState.Dormant);
            if (finished) yield break;

            var boss = room.Enemies[0];
            var samples = new List<float>(4096);
            var collections = GC.CollectionCount(0);
            var captured = false;
            var start = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - start < SampleSeconds)
            {
                yield return null;
                samples.Add(Time.unscaledDeltaTime * 1000f);
                if (captured || boss.State != EnemyState.Telegraph || boss.TelegraphRemaining > boss.CurrentTelegraph * 0.35f) continue;
                captured = true;
                yield return new WaitForEndOfFrame();
                var frame = ScreenCapture.CaptureScreenshotAsTexture();
                File.WriteAllBytes(Path.ChangeExtension(OutputPath(), null) + $"_phase{phase + 1}.png", frame.EncodeToPNG());
                lines.Add($"phase {phase + 1} meanLuminance={MeanLuminance(frame):F3}");
                Destroy(frame);
            }

            samples.Sort();
            var average = samples.Average();
            var p99 = samples[Mathf.Min(samples.Count - 1, Mathf.FloorToInt(samples.Count * 0.99f))];
            lines.Add($"phase {phase + 1} boss={boss.Definition.DisplayName}; avgFps={1000f / average:F0}; onePercentLowFps={1000f / p99:F0}; worstMs={samples[samples.Count - 1]:F1}; gen0={GC.CollectionCount(0) - collections}; captured={captured}");
        }

        private IEnumerator Until(Func<bool> condition)
        {
            while (!finished && !condition()) yield return null;
        }

        private bool Check(bool condition, string name)
        {
            lines.Add($"{(condition ? "PASS" : "FAIL")}: {name}");
            if (!condition) Finish(false, name);
            return condition;
        }

        private void Finish(bool success, string reason)
        {
            if (finished) return;
            finished = true;
            Time.timeScale = 1f;
            lines.Add($"loggedErrors={loggedErrors}; firstError={firstError ?? "none"}");
            if (loggedErrors > 0)
            {
                success = false;
                reason = "player logged errors";
            }

            lines.Add($"result={(success ? "SUCCESS" : "FAILURE")}; reason={reason}; seconds={Time.realtimeSinceStartup:F1}");
            File.WriteAllLines(OutputPath(), lines);
            Application.Quit(success ? 0 : 1);
        }

        private static string OutputPath()
        {
            var args = Environment.GetCommandLineArgs();
            var index = Array.IndexOf(args, "-smokeOut");
            return index >= 0 && index + 1 < args.Length ? args[index + 1] : Path.Combine(Application.persistentDataPath, "smoke.txt");
        }
    }
}
