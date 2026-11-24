using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VorgGates.App;
using VorgGates.Core;
using VorgGates.Gameplay;
using VorgGates.Presentation;

namespace VorgGates.Tests
{
    public sealed class PresentationTests
    {
        private GameBootstrap game;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("Game");
            yield return null;
            yield return WaitUntil(() =>
            {
                game = Object.FindAnyObjectByType<GameBootstrap>();
                return game != null && game.IsReady;
            }, 8f, "Game did not become ready");
        }

        [TearDown]
        public void TearDown() => Time.timeScale = 1f;

        [UnityTest]
        public IEnumerator Game_StartsInMainMenu_AndMouseClickStartsCampaign()
        {
            Assert.AreEqual(GameFlowState.Menu, game.Flow.State);
            Assert.AreEqual(MenuScreen.Main, game.Menu.Current);
            yield return null;
            yield return null;

            var settings = InputSystem.settings;
            var background = settings.backgroundBehavior;
            var editorBehavior = settings.editorInputBehaviorInPlayMode;
            settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            var mouse = InputSystem.AddDevice<Mouse>();
            InputSystem.EnableDevice(mouse);
            try
            {
                yield return null;
                var button = UnityEngine.UIElements.UQueryExtensions.Q(game.Menu.Root, "btn-play");
                var panelWidth = button.panel.visualTree.layout.width;
                var scale = Screen.width / panelWidth;
                var center = button.worldBound.center;
                var screen = new Vector2(center.x * scale, Screen.height - center.y * scale);

                InputSystem.QueueStateEvent(mouse, new MouseState { position = screen });
                yield return null;
                InputSystem.QueueStateEvent(mouse, new MouseState { position = screen }.WithButton(MouseButton.Left));
                yield return WaitUntil(() => game.Flow.State == GameFlowState.Playing, 2f, "Clicking Jogar did not start the game");
                Assert.AreEqual(MenuScreen.None, game.Menu.Current);
                Assert.IsTrue(game.Hud.IsPhaseCardVisible, "The phase name must appear");
                Assert.AreEqual("Cripta de Musgo", game.Hud.PhaseCardTitle);
                yield return null;
                yield return null;
                Assert.IsFalse(game.Player.Weapon.IsAttacking, "The menu click must not swing the weapon");
            }
            finally
            {
                settings.backgroundBehavior = background;
                settings.editorInputBehaviorInPlayMode = editorBehavior;
                InputSystem.RemoveDevice(mouse);
            }
        }

        [UnityTest]
        public IEnumerator Credits_AppearOnlyInTheMainMenuScreens_AndHudShowsVorg()
        {
            var credits = (UnityEngine.UIElements.Label)UnityEngine.UIElements.UQueryExtensions.Q(game.Menu.Root, "menu-credits");
            var playerName = (UnityEngine.UIElements.Label)UnityEngine.UIElements.UQueryExtensions.Q(game.Menu.Root, "player-name");
            bool Visible() => credits.style.display == UnityEngine.UIElements.DisplayStyle.Flex;

            Assert.AreEqual("Desenvolvido pelos alunos Arthur Martins Jorge e Gabriel De Castro Domingo", credits.text);
            Assert.AreEqual("VORG", playerName.text);
            Assert.IsTrue(Visible(), "Main menu");
            game.Menu.Show(MenuScreen.Controls);
            Assert.IsTrue(Visible(), "Controls opened from the main menu");
            game.Menu.Show(MenuScreen.Main);

            game.StartNewCampaign();
            yield return null;
            Assert.IsFalse(Visible(), "Gameplay");
            game.Player.SetInput(new ScriptedPlayerInput());
            game.PauseGame();
            Assert.IsFalse(Visible(), "Pause");
            game.Menu.Show(MenuScreen.Controls);
            Assert.IsFalse(Visible(), "Controls opened from pause");
            game.Menu.Show(MenuScreen.Pause);
            game.ResumeGame();
            Assert.IsFalse(Visible(), "Resumed gameplay");
        }

        [UnityTest]
        public IEnumerator Pause_FreezesGame_AndContinueKeepsTheCheckpoint()
        {
            game.Player.Inventory.Add(WeaponKind.Axe);
            Assert.IsTrue(game.JumpToDungeon(1, 4242));
            var input = new ScriptedPlayerInput();
            game.Player.SetInput(input);
            yield return null;

            input.Pause = true;
            yield return WaitUntil(() => game.Flow.State == GameFlowState.Paused, 1f, "Esc did not pause");
            Assert.AreEqual(0f, Time.timeScale);
            Assert.AreEqual(MenuScreen.Pause, game.Menu.Current);

            game.ResumeGame();
            Assert.AreEqual(GameFlowState.Playing, game.Flow.State);
            Assert.AreEqual(1f, Time.timeScale);
            Assert.AreEqual(MenuScreen.None, game.Menu.Current);

            game.PauseGame();
            game.Player.Inventory.Add(WeaponKind.Staff);
            game.ReturnToMenu();
            Assert.AreEqual(GameFlowState.Menu, game.Flow.State);
            Assert.AreEqual(1f, Time.timeScale);
            Assert.IsFalse(game.Player.Inventory.Owns(WeaponKind.Staff), "Leaving must return to the checkpoint");

            game.ContinueCampaign();
            yield return null;
            Assert.AreEqual(GameFlowState.Playing, game.Flow.State);
            Assert.AreEqual(1, game.Session.DungeonIndex);
            Assert.IsTrue(game.Player.Inventory.Owns(WeaponKind.Axe));
        }

        [UnityTest]
        public IEnumerator Boss_TurnsTranslucent_OnlyWhenCoveringThePlayer()
        {
            Assert.IsTrue(game.JumpToDungeon(0, 4242));
            var input = new ScriptedPlayerInput();
            game.Player.SetInput(input);
            game.Player.Health.SetLocked(true);
            yield return null;
            var node = game.Director.Layout.Rooms.First(r => r.Kind == RoomKind.Boss);
            var room = game.Director.Current.Rooms[node.Id];
            game.Player.Motor.Teleport(room.transform.position + new Vector3(0f, 0.05f, -4f), Quaternion.identity);
            yield return WaitUntil(() => room.Enemies.Count == 1 && room.Enemies[0].State != EnemyState.Dormant, 3f, "Boss did not activate");

            var boss = room.Enemies[0];
            var fade = boss.GetComponent<OcclusionFade>();
            Assert.IsNotNull(fade);
            var toCamera = -Camera.main.transform.forward;
            toCamera.y = 0f;
            toCamera.Normalize();

            game.Player.Motor.Teleport(boss.transform.position - toCamera * 1.6f + Vector3.up * 0.05f, Quaternion.identity);
            yield return WaitUntil(() => fade.IsFaded, 1f, "Boss should fade when it covers the player");

            game.Player.Motor.Teleport(boss.transform.position + toCamera * 4.5f + Vector3.up * 0.05f, Quaternion.identity);
            yield return WaitUntil(() => !fade.IsFaded, 1f, "Boss should be solid when the player is in front");
        }

        [UnityTest]
        public IEnumerator Audio_HasAllClips_AndRespondsToGameEvents()
        {
            var library = game.Audio.Library;
            Assert.AreEqual(System.Enum.GetValues(typeof(SfxId)).Length, library.Effects.Length);
            Assert.IsTrue(library.Effects.All(c => c != null && c.length > 0f), "Missing sound effect");
            Assert.AreEqual(RunSession.DungeonCount, library.Music.Length);
            Assert.IsTrue(library.Music.All(c => c != null && c.length > 10f), "Missing music");
            Assert.IsTrue(game.Audio.IsMusicPlaying, "Menu music must play");

            Assert.IsTrue(game.JumpToDungeon(1, 4242));
            yield return null;
            Assert.AreEqual(1, game.Audio.CurrentMusic);
            var input = new ScriptedPlayerInput { AimPoint = game.Player.transform.position + Vector3.forward };
            game.Player.SetInput(input);
            var before = game.Audio.EffectsPlayed;
            input.AttackHeld = true;
            yield return null;
            input.AttackHeld = false;
            yield return null;
            Assert.Greater(game.Audio.EffectsPlayed, before, "Attacking must play a sound");
        }

        private static IEnumerator WaitUntil(System.Func<bool> condition, float timeout, string message)
        {
            var deadline = Time.realtimeSinceStartup + timeout;
            while (!condition())
            {
                Assert.Less(Time.realtimeSinceStartup, deadline, message);
                yield return null;
            }
        }
    }
}
