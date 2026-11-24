using System.Linq;
using NUnit.Framework;
using VorgGates.Core;

namespace VorgGates.Tests
{
    public sealed class CampaignRulesTests
    {
        private static readonly object[] Phases =
        {
            new object[] { 6, 2, 1, 4, 2, 0, RoomKind.Portal },
            new object[] { 7, 3, 1, 5, 3, 0, RoomKind.Portal },
            new object[] { 7, 4, 1, 6, 3, 1, RoomKind.SwordChamber }
        };

        private static RunSession SessionAt(int dungeon, InventoryState inventory)
        {
            var session = new RunSession(inventory);
            for (var i = 0; i < dungeon; i++)
            {
                session.BeginTransition();
                session.CommitCheckpoint();
            }

            return session;
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void Rules1To3_DeathKeepsCurrentDungeon(int dungeon)
        {
            var session = SessionAt(dungeon, new InventoryState());
            Assert.IsTrue(session.RegisterDeath());
            Assert.AreEqual(dungeon, session.DungeonIndex);
            Assert.AreEqual(1, session.Deaths);
        }

        [Test]
        public void Rules5And6_KeepEntryWeapons_AndDropFailedAttemptWeapons()
        {
            var inventory = new InventoryState();
            var session = new RunSession(inventory);
            inventory.Add(WeaponKind.Axe);
            session.BeginTransition();
            session.CommitCheckpoint();

            inventory.Add(WeaponKind.Staff);
            session.RegisterDeath();

            Assert.IsTrue(inventory.Owns(WeaponKind.Axe));
            Assert.IsFalse(inventory.Owns(WeaponKind.Staff));
            Assert.AreEqual(WeaponKind.Axe, inventory.Equipped);
        }

        [Test]
        public void Rule8_CheckpointChangesOnlyAfterCommit()
        {
            var inventory = new InventoryState();
            var session = new RunSession(inventory);
            inventory.Add(WeaponKind.Axe);

            Assert.IsTrue(session.BeginTransition());
            Assert.AreEqual(0, session.DungeonIndex);
            Assert.IsFalse(session.RegisterDeath(), "Death cannot interrupt a transition");
            session.AbortTransition();

            Assert.AreEqual(0, session.DungeonIndex);
            session.RegisterDeath();
            Assert.IsFalse(inventory.Owns(WeaponKind.Axe), "An aborted transition must not save weapons");
        }

        [Test]
        public void FinalDungeon_HasNoNextPortal()
        {
            var session = SessionAt(2, new InventoryState());
            Assert.IsTrue(session.IsFinalDungeon);
            Assert.IsFalse(session.BeginTransition());
        }

        [Test]
        public void RestartCampaign_ClearsProgress()
        {
            var inventory = new InventoryState();
            var session = new RunSession(inventory);
            inventory.Add(WeaponKind.Axe);
            session.BeginTransition();
            session.CommitCheckpoint();
            session.RegisterDeath();

            session.RestartCampaign();

            Assert.AreEqual(0, session.DungeonIndex);
            Assert.AreEqual(0, session.Deaths);
            Assert.IsFalse(inventory.Owns(WeaponKind.Axe));
        }

        [TestCaseSource(nameof(Phases))]
        public void PhaseLayouts_AreValid_WithBossBeforeExit_AndRewardOnRequiredHost(int main, int min, int step, int max, int tier, int rewardHost, RoomKind exit)
        {
            var config = new DungeonConfig(main, 1, 3, min, step, max, enemyTier: tier, hasBoss: true, exitKind: exit, rewardHostIndex: rewardHost);
            for (var seed = 0; seed < 1000; seed++)
            {
                var layout = DungeonGenerator.Generate(config, seed);
                Assert.IsTrue(LayoutValidator.Validate(layout, config, out var error), $"Seed {seed}: {error}");
                var mainPath = layout.Rooms.Where(r => r.IsOnMainPath).OrderBy(r => r.Depth).ToArray();
                Assert.AreEqual(exit, mainPath[mainPath.Length - 1].Kind);
                Assert.AreEqual(RoomKind.Boss, mainPath[mainPath.Length - 2].Kind);
                Assert.AreEqual(mainPath[rewardHost + 1].Id, layout.Links.First(l => layout.Rooms[l.To].Kind == RoomKind.Reward).From);
            }
        }

        [TestCaseSource(nameof(Phases))]
        public void PhaseFallback_IsValid(int main, int min, int step, int max, int tier, int rewardHost, RoomKind exit)
        {
            var config = new DungeonConfig(main, 1, 3, min, step, max, 0, tier, true, exit, rewardHost);
            var layout = DungeonGenerator.Generate(config, 5);
            Assert.IsTrue(layout.IsFallback);
            Assert.IsTrue(LayoutValidator.Validate(layout, config, out var error), error);
        }
    }
}
