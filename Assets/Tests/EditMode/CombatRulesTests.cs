using System.Linq;
using NUnit.Framework;
using VorgGates.Core;

namespace VorgGates.Tests
{
    public sealed class CombatRulesTests
    {
        [Test]
        public void Inventory_StartsWithSword_AndOnlyEquipsOwnedWeapons()
        {
            var inventory = new InventoryState();
            Assert.IsTrue(inventory.Owns(WeaponKind.Sword));
            Assert.IsFalse(inventory.TryEquip(WeaponKind.Staff));
            Assert.AreEqual(WeaponKind.Sword, inventory.Equipped);

            Assert.IsTrue(inventory.Add(WeaponKind.Staff));
            Assert.AreEqual(WeaponKind.Staff, inventory.Equipped);
            Assert.IsFalse(inventory.Add(WeaponKind.Staff));
            Assert.IsTrue(inventory.TryEquip(WeaponKind.Sword));
        }

        [Test]
        public void Inventory_CycleSkipsMissingWeapons()
        {
            var inventory = new InventoryState();
            inventory.Add(WeaponKind.Staff);
            Assert.IsTrue(inventory.Cycle(1));
            Assert.AreEqual(WeaponKind.Sword, inventory.Equipped);
            Assert.IsTrue(inventory.Cycle(-1));
            Assert.AreEqual(WeaponKind.Staff, inventory.Equipped);
        }

        [Test]
        public void Inventory_SnapshotDiscardsFailedAttempt()
        {
            var inventory = new InventoryState();
            inventory.Add(WeaponKind.Axe);
            var checkpoint = inventory.Capture();

            inventory.Add(WeaponKind.Staff);
            inventory.Restore(checkpoint);

            Assert.IsTrue(inventory.Owns(WeaponKind.Axe));
            Assert.IsFalse(inventory.Owns(WeaponKind.Staff));
            Assert.AreEqual(WeaponKind.Axe, inventory.Equipped);
            Assert.IsTrue(inventory.TryGetNextMissing(out var next));
            Assert.AreEqual(WeaponKind.Staff, next);
        }

        [Test]
        public void TokenPool_NeverExceedsCapacity()
        {
            var pool = new AttackTokenPool(3);
            Assert.IsTrue(pool.TryAcquire() && pool.TryAcquire() && pool.TryAcquire());
            Assert.IsFalse(pool.TryAcquire());
            pool.Release();
            Assert.IsTrue(pool.TryAcquire());
            Assert.AreEqual(3, pool.InUse);
        }

        [Test]
        public void Encounters_IntroduceEnemyTypesAlongThePath()
        {
            Assert.IsTrue(EncounterPlanner.Plan(0, 2, 3).All(k => k == EnemyKind.Crawler));
            Assert.Contains(EnemyKind.Archer, EncounterPlanner.Plan(1, 3, 3));
            var third = EncounterPlanner.Plan(2, 4, 3);
            Assert.Contains(EnemyKind.Brute, third);
            Assert.Contains(EnemyKind.Archer, third);
            Assert.Contains(EnemyKind.Crawler, third);
            Assert.IsFalse(EncounterPlanner.Plan(4, 5, 2).Contains(EnemyKind.Brute));
        }

        [Test]
        public void Health_HealClampsToMaximum_AndDoesNotReviveTheDead()
        {
            var health = new HealthModel(100);
            health.TakeDamage(30);
            Assert.IsTrue(health.Heal(50));
            Assert.AreEqual(100, health.Current);
            health.TakeDamage(100);
            Assert.IsFalse(health.Heal(10));
            Assert.IsTrue(health.IsDead);
        }
    }
}
