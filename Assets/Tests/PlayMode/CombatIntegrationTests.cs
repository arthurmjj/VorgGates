using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VorgGates.App;
using VorgGates.Core;
using VorgGates.Gameplay;

namespace VorgGates.Tests
{
    public sealed class CombatIntegrationTests
    {
        private const int TestSeed = 4242;

        private GameBootstrap game;
        private ScriptedPlayerInput input;

        private PlayerStateMachine Player => game.Player;
        private RoomController[] CombatRooms => game.Director.Layout.Rooms
            .Where(r => r.Kind == RoomKind.Combat).OrderBy(r => r.Depth)
            .Select(r => game.Director.Current.Rooms[r.Id]).ToArray();

        [UnitySetUp]
        public IEnumerator SetUp()
        {
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

        [Test]
        public void Weapons_HaveDistinctDamageRangeAndSpeed()
        {
            var inventory = Player.Inventory;
            var sword = inventory.Definition(WeaponKind.Sword);
            var axe = inventory.Definition(WeaponKind.Axe);
            var staff = inventory.Definition(WeaponKind.Staff);

            Assert.AreEqual(20, sword.Damage);
            Assert.AreEqual(45, axe.Damage);
            Assert.AreEqual(14, staff.Damage);
            Assert.AreEqual(0.35f, sword.Interval, 0.02f);
            Assert.AreEqual(0.85f, axe.Interval, 0.05f);
            Assert.AreEqual(0.45f, staff.Interval, 0.03f);
            Assert.Less(sword.Range, axe.Range);
            Assert.Greater(staff.Range, axe.Range * 4f);
            Assert.Greater(axe.ArcDegrees, sword.ArcDegrees);
            Assert.IsTrue(staff.IsRanged);
            Assert.IsFalse(sword.IsRanged || axe.IsRanged);
        }

        [UnityTest]
        public IEnumerator Slots_OnlySelectOwnedWeapons()
        {
            input.WeaponSlot = 2;
            yield return null;
            Assert.AreEqual(WeaponKind.Sword, Player.Inventory.Equipped);

            Player.Inventory.Add(WeaponKind.Axe);
            Player.Inventory.Add(WeaponKind.Staff);
            input.WeaponSlot = 1;
            yield return null;
            Assert.AreEqual(WeaponKind.Axe, Player.Weapon.Weapon.Kind);
            input.WeaponSlot = 2;
            yield return null;
            Assert.AreEqual(WeaponKind.Staff, Player.Weapon.Weapon.Kind);
            input.WeaponSlot = 0;
            yield return null;
            Assert.AreEqual(WeaponKind.Sword, Player.Weapon.Weapon.Kind);
        }

        [UnityTest]
        public IEnumerator Sword_ThirdHitDealsComboDamage()
        {
            var enemy = default(EnemyBrain);
            yield return EnterRoom(CombatRooms[0], e => enemy = e[0]);
            foreach (var other in CombatRooms[0].Enemies.Where(e => e != enemy).ToArray()) Kill(other);
            enemy.Health.Configure(200);
            var before = enemy.Health.Current;
            for (var swing = 0; swing < 3; swing++)
            {
                Player.Motor.Teleport(enemy.transform.position - enemy.transform.forward * 1.2f + Vector3.up * 0.05f, Quaternion.identity);
                input.AimPoint = enemy.transform.position;
                yield return null;
                input.AttackHeld = true;
                yield return null;
                input.AttackHeld = false;
                yield return WaitUntil(() => !Player.Weapon.IsAttacking, 1f, "Swing did not finish");
            }

            Assert.AreEqual(before - 70, enemy.Health.Current, "Sword combo must deal 20 + 20 + 30");
        }

        [UnityTest]
        public IEnumerator Axe_DealsHeavyDamage()
        {
            var enemy = default(EnemyBrain);
            yield return EnterRoom(CombatRooms[0], e => enemy = e[0]);
            Player.Inventory.Add(WeaponKind.Axe);
            Player.Motor.Teleport(enemy.transform.position - enemy.transform.forward * 1.3f + Vector3.up * 0.05f, Quaternion.identity);
            input.AimPoint = enemy.transform.position;
            yield return null;
            input.AttackHeld = true;
            yield return null;
            input.AttackHeld = false;
            yield return WaitUntil(() => !Player.Weapon.IsAttacking, 1.5f, "Swing did not finish");
            Assert.IsTrue(enemy == null || enemy.Health.IsDead, "45 damage must defeat a 40 HP crawler");
        }

        [UnityTest]
        public IEnumerator Staff_HitsDistantEnemy_AndWallsStopProjectiles()
        {
            var enemy = default(EnemyBrain);
            var room = CombatRooms[0];
            yield return EnterRoom(room, e => enemy = e[0]);
            Player.Inventory.Add(WeaponKind.Staff);
            var before = enemy.Health.Current;
            var direction = (room.transform.position - enemy.transform.position).normalized;
            direction.y = 0f;
            Player.Motor.Teleport(enemy.transform.position + direction.normalized * 5f + Vector3.up * 0.05f, Quaternion.identity);
            input.AimPoint = enemy.transform.position;
            yield return null;
            input.AttackHeld = true;
            yield return null;
            input.AttackHeld = false;
            yield return WaitUntil(() => enemy.Health.Current < before, 1.5f, "Staff projectile did not hit");
            Assert.AreEqual(before - 14, enemy.Health.Current);

            yield return WaitUntil(() => !Player.Weapon.IsAttacking, 1f, "Cast did not finish");
            yield return WaitUntil(() => game.Director.Projectiles.ActiveCount == 0, 1f, "Hit projectile stayed active");

            Player.Motor.Teleport(room.transform.position + new Vector3(0f, 0.05f, 6f), Quaternion.identity);
            var wall = room.transform.position + new Vector3(4f, 1f, 12f);
            input.AimPoint = wall;
            yield return null;
            input.AttackHeld = true;
            yield return WaitUntil(() => game.Director.Projectiles.ActiveCount > 0, 1f, "Staff did not fire");
            input.AttackHeld = false;
            yield return WaitUntil(() => game.Director.Projectiles.ActiveCount == 0, 0.6f, "Wall did not stop the projectile");
        }

        [UnityTest]
        public IEnumerator Archer_ShootsAndDamagesPlayer()
        {
            var room = CombatRooms[1];
            EnemyBrain archer = null;
            yield return EnterRoom(room, e => archer = e.First(x => x.Definition.Kind == EnemyKind.Archer));
            foreach (var other in room.Enemies.Where(e => e != archer).ToArray()) Kill(other);
            Assert.AreEqual(1, room.AliveEnemies);
            var start = Player.Health.Current;
            yield return WaitUntil(() => Player.Health.Current < start, 9f, "Archer never damaged the player");
            Assert.AreEqual(start - archer.Definition.Damage, Player.Health.Current);
        }

        [UnityTest]
        public IEnumerator Brute_ChargesAndDamagesPlayer()
        {
            Assert.IsTrue(game.JumpToDungeon(1, TestSeed), "The Brute first appears in phase 2");
            yield return null;
            var room = CombatRooms[2];
            EnemyBrain brute = null;
            yield return EnterRoom(room, e => brute = e.First(x => x.Definition.Kind == EnemyKind.Brute));
            foreach (var other in room.Enemies.Where(e => e != brute).ToArray()) Kill(other);
            var start = Player.Health.Current;
            var sawTelegraph = false;
            yield return WaitUntil(() =>
            {
                sawTelegraph |= brute.State == EnemyState.Telegraph;
                return Player.Health.Current < start;
            }, 10f, "Brute never damaged the player");
            Assert.IsTrue(sawTelegraph, "Brute must telegraph before hitting");
            Assert.AreEqual(start - brute.ScaledDamage, Player.Health.Current);
        }

        [UnityTest]
        public IEnumerator RewardRoom_GrantsNextWeapon_AndDeathRemovesIt()
        {
            var reward = game.Director.Current.Reward;
            Assert.IsNotNull(reward);
            Assert.IsTrue(reward.OffersWeapon);
            Assert.AreEqual(WeaponKind.Axe, reward.Weapon);

            Player.Motor.Teleport(reward.transform.position + new Vector3(0f, 0.05f, -1.8f), Quaternion.identity);
            yield return WaitUntil(() => ReferenceEquals(Player.Interactor.Current, reward), 1f, "Reward is not interactable");
            input.Interact = true;
            yield return WaitUntil(() => reward.IsCollected, 1f, "Reward was not collected");
            Assert.IsTrue(Player.Inventory.Owns(WeaponKind.Axe));
            Assert.AreEqual(WeaponKind.Axe, Player.Inventory.Equipped);

            Player.Health.ApplyDamage(new DamageInfo(999, Vector3.forward, 0f));
            yield return WaitUntil(() => game.Flow.State == GameFlowState.Playing, 2f, "Dungeon did not restart");
            Assert.IsFalse(Player.Inventory.Owns(WeaponKind.Axe), "Weapons from a failed attempt must be lost");
            Assert.AreEqual(WeaponKind.Sword, Player.Inventory.Equipped);
            Assert.AreEqual(WeaponKind.Axe, game.Director.Current.Reward.Weapon);
        }

        private IEnumerator EnterRoom(RoomController room, System.Action<EnemyBrain[]> onReady)
        {
            Player.Health.SetLocked(false);
            Player.Motor.Teleport(room.transform.position + Vector3.up * 0.05f, Quaternion.identity);
            yield return WaitUntil(() => room.State == RoomState.Active, 1f, "Room did not activate");
            yield return WaitUntil(() => room.Enemies.Count == room.EnemyCount && room.Enemies.All(e => e.State != EnemyState.Dormant), 2f, "Enemies did not activate");
            onReady(room.Enemies.ToArray());
        }

        private static void Kill(EnemyBrain enemy) => enemy.Health.ApplyDamage(new DamageInfo(9999, Vector3.forward, 0f));

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
