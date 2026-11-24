using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VorgGates.App;
using VorgGates.Core;
using VorgGates.Gameplay;
using VorgGates.Presentation;

namespace VorgGates.Tests
{
    public sealed class CampaignTests
    {
        private const int TestSeed = 4242;

        private GameBootstrap game;
        private ScriptedPlayerInput input;

        private PlayerStateMachine Player => game.Player;

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

            Assert.IsTrue(game.RestartWithSeed(TestSeed));
            yield return null;
            input = new ScriptedPlayerInput { AimPoint = Player.transform.position + Vector3.forward };
            Player.SetInput(input);
        }

        [TearDown]
        public void TearDown() => Time.timeScale = 1f;

        [UnityTest]
        public IEnumerator Portal_BuildsNextPhase_WithTheme_AndKeepsWeapons()
        {
            Player.Inventory.Add(WeaponKind.Axe);
            var oldRoot = game.Director.Current.Root;
            yield return UsePortal();
            Assert.AreEqual(GameFlowState.Transition, game.Flow.State);
            Assert.AreEqual(0, game.Session.DungeonIndex, "The checkpoint must wait for the next dungeon");

            yield return WaitUntil(() => game.Flow.State == GameFlowState.Playing, 3f, "Transition did not finish");
            yield return null;
            Assert.AreEqual(1, game.Session.DungeonIndex);
            Assert.AreEqual(1, game.Theme.AppliedIndex);
            Assert.AreEqual("Forja Afundada", game.Director.Definition.DisplayName);
            Assert.IsTrue(oldRoot == null, "The previous phase must be destroyed");
            Assert.IsTrue(Player.Inventory.Owns(WeaponKind.Axe));
            Assert.AreEqual(100, Player.Health.Current);
        }

        [UnityTest]
        public IEnumerator Rule8_FailedTransition_KeepsCheckpoint()
        {
            Player.Inventory.Add(WeaponKind.Axe);
            game.Director.FailNextBuild = true;
            LogAssert.ignoreFailingMessages = true;
            yield return UsePortal();
            yield return WaitUntil(() => game.Flow.State == GameFlowState.Playing, 3f, "Game did not recover");
            LogAssert.ignoreFailingMessages = false;
            Assert.AreEqual(0, game.Session.DungeonIndex);
            Assert.IsFalse(game.Session.IsTransitioning);
            Assert.AreEqual("Cripta de Musgo", game.Director.Definition.DisplayName);
        }

        [UnityTest]
        public IEnumerator Rules1To7_DeathRestartsOnlyTheCurrentPhase([Values(0, 1, 2)] int phase)
        {
            Player.Inventory.Add(WeaponKind.Axe);
            Assert.IsTrue(game.JumpToDungeon(phase, TestSeed));
            yield return null;
            var previous = game.Director.Layout.Signature;
            Player.Inventory.Add(WeaponKind.Staff);

            Player.Health.ApplyDamage(new DamageInfo(999, Vector3.forward, 0f));
            yield return WaitUntil(() => game.Flow.State == GameFlowState.Playing, 2f, "Phase did not restart");

            Assert.AreEqual(phase, game.Session.DungeonIndex);
            Assert.AreEqual(phase, game.Director.DungeonIndex);
            Assert.AreNotEqual(previous, game.Director.Layout.Signature, "Rule 4: new layout");
            Assert.IsTrue(Player.Inventory.Owns(WeaponKind.Axe), "Rule 5: entry weapons stay");
            Assert.IsFalse(Player.Inventory.Owns(WeaponKind.Staff), "Rule 6: failed attempt weapons are lost");
            Assert.AreEqual(100, Player.Health.Current, "Rule 7: health is restored");
        }

        [UnityTest]
        public IEnumerator Bosses_UseMultipleTelegraphedPatterns_AndDamageThePlayer([Values(0, 1, 2)] int phase)
        {
            Assert.IsTrue(game.JumpToDungeon(phase, TestSeed));
            yield return null;
            var boss = default(EnemyBrain);
            yield return EnterBossRoom(b => boss = b);
            Assert.GreaterOrEqual(boss.Definition.Patterns.Length, 2);
            Assert.AreSame(boss, game.Director.ActiveBoss);

            Time.timeScale = 3f;
            var seen = new HashSet<BossPatternKind>();
            // A long-range boss can start its first telegraph on the frame it activates.
            var telegraphs = boss.State == EnemyState.Chase ? 0 : 1;
            if (telegraphs > 0) seen.Add(boss.CurrentPattern.Value);
            boss.TelegraphStarted += info =>
            {
                telegraphs++;
                seen.Add(boss.CurrentPattern.Value);
            };

            var start = Player.Health.Current;
            var damaged = false;
            var deadline = Time.realtimeSinceStartup + 14f;
            while ((seen.Count < 2 || !damaged) && Time.realtimeSinceStartup < deadline)
            {
                if (Player.Health.Current < start)
                {
                    damaged = true;
                    Assert.Greater(telegraphs, 0, "Damage must follow a telegraph");
                    Player.Health.SetLocked(true);
                }

                FollowBoss(boss);
                yield return null;
            }

            Assert.GreaterOrEqual(seen.Count, 2, "Boss must alternate patterns");
            Assert.IsTrue(damaged, "Boss attacks must be able to hit");
        }

        [UnityTest]
        public IEnumerator NavMesh_StaysOnTheFloor_InAllPhases()
        {
            for (var phase = 0; phase < RunSession.DungeonCount; phase++)
            {
                for (var seed = 0; seed < 6; seed++)
                {
                    Assert.IsTrue(game.JumpToDungeon(phase, 446248638 + seed));
                    yield return null;
                    var highest = UnityEngine.AI.NavMesh.CalculateTriangulation().vertices.Max(v => v.y);
                    Assert.Less(highest, 0.3f, $"Phase {phase}, seed {seed}: the NavMesh climbed onto a prop");
                }
            }
        }

        [UnityTest]
        public IEnumerator BossAttack_CanBeAvoidedByLeavingTheWarning()
        {
            var boss = default(EnemyBrain);
            yield return EnterBossRoom(b => boss = b);
            var room = game.Director.ActiveRoom.transform.position;
            Player.Motor.Teleport(boss.transform.position + Vector3.back * 2f + Vector3.up * 0.05f, Quaternion.identity);
            yield return WaitUntil(() => boss.State == EnemyState.Telegraph, 5f, "Boss did not telegraph");
            Assert.AreEqual(BossPatternKind.Slam, boss.CurrentPattern);

            var away = boss.transform.position - room;
            away.y = 0f;
            var escape = room - (away.sqrMagnitude > 1f ? away.normalized : Vector3.forward) * 7f;
            Player.Motor.Teleport(escape + Vector3.up * 0.05f, Quaternion.identity);
            var health = Player.Health.Current;
            yield return WaitUntil(() => boss.State == EnemyState.Recover, 3f, "Attack did not finish");
            Assert.AreEqual(health, Player.Health.Current, "Leaving the warned area must avoid the damage");
        }

        [UnityTest, Timeout(420000)]
        public IEnumerator FullCampaign_DefeatsThreeBosses_AndTakesTheSword()
        {
            Time.timeScale = 3f;
            for (var phase = 0; phase < RunSession.DungeonCount; phase++)
            {
                Assert.AreEqual(phase, game.Session.DungeonIndex);
                var layout = game.Director.Layout;
                var bot = new DungeonBot(game);
                yield return bot.CompleteDungeon(150f);
                Assert.IsTrue(bot.ReachedExit, $"Phase {phase}: {bot.Failure}");
                Assert.AreEqual(layout.Rooms.Count - 1, bot.RoomsVisited, $"Phase {phase}: not every room was explored");
                Assert.AreEqual(0, game.Director.CombatRoomsRemaining);
                if (phase < RunSession.DungeonCount - 1)
                    yield return WaitUntil(() => game.Flow.State == GameFlowState.Playing, 3f, "Transition did not finish");
            }

            Assert.AreEqual(GameFlowState.Victory, game.Flow.State);
            Assert.IsTrue(Player.Inventory.Owns(WeaponKind.Axe) && Player.Inventory.Owns(WeaponKind.Staff));

            Time.timeScale = 1f;
            yield return new WaitForSeconds(1.3f);
            Player.SetInput(new ScriptedPlayerInput { Interact = true });
            yield return WaitUntil(() => game.Flow.State == GameFlowState.Playing, 2f, "Victory did not restart the campaign");
            Assert.IsFalse(game.Menu.Current != MenuScreen.None, "No menu may stay open during play");
            Assert.AreEqual(0, game.Session.DungeonIndex);
            Assert.IsFalse(Player.Inventory.Owns(WeaponKind.Axe));
        }

        private IEnumerator UsePortal()
        {
            var portal = game.Director.Current.Portal;
            foreach (var room in game.Director.Current.Rooms.Where(r => r.HasEncounter))
            {
                Player.Motor.Teleport(room.transform.position + Vector3.up * 0.05f, Quaternion.identity);
                Player.Health.SetLocked(true);
                yield return WaitUntil(() => room.State == RoomState.Active && room.Enemies.Count == room.EnemyCount, 1.5f, "Room did not start");
                yield return new WaitForSeconds(0.65f);
                foreach (var enemy in room.Enemies.ToArray()) enemy.Health.ApplyDamage(new DamageInfo(99999, Vector3.forward, 0f));
                yield return WaitUntil(() => room.State == RoomState.Cleared, 1f, "Room did not clear");
            }

            Assert.IsTrue(portal.IsAvailable);
            Player.Motor.Teleport(portal.transform.position + new Vector3(0f, 0.05f, -1.7f), Quaternion.identity);
            yield return WaitUntil(() => ReferenceEquals(Player.Interactor.Current, portal), 1f, "Portal is out of reach");
            input.Interact = true;
            yield return WaitUntil(() => game.Flow.State != GameFlowState.Playing, 1f, "Portal did not respond");
        }

        private IEnumerator EnterBossRoom(System.Action<EnemyBrain> onReady)
        {
            var node = game.Director.Layout.Rooms.First(r => r.Kind == RoomKind.Boss);
            var room = game.Director.Current.Rooms[node.Id];
            Player.Motor.Teleport(room.transform.position + new Vector3(0f, 0.05f, -4f), Quaternion.identity);
            yield return WaitUntil(() => room.State == RoomState.Active && room.Enemies.Count == 1 && room.Enemies[0].State != EnemyState.Dormant, 3f, "Boss did not activate");
            onReady(room.Enemies[0]);
        }

        private void FollowBoss(EnemyBrain boss)
        {
            var delta = boss.transform.position - Player.transform.position;
            delta.y = 0f;
            input.AimPoint = boss.transform.position;
            if (delta.magnitude < 2.5f)
            {
                input.Move = Vector2.zero;
                return;
            }

            var right = IsoMovement.ToWorld(Vector2.right, 45f);
            var up = IsoMovement.ToWorld(Vector2.up, 45f);
            input.Move = new Vector2(Vector3.Dot(delta.normalized, right), Vector3.Dot(delta.normalized, up));
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
