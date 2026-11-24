using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using VorgGates.Core;

namespace VorgGates.Tests
{
    public sealed class DungeonGeneratorTests
    {
        private const int SeedCount = 1000;

        private static readonly object[] Configs =
        {
            new object[] { 5, 1 },
            new object[] { 6, 1 },
            new object[] { 7, 1 }
        };

        private static DungeonConfig Config(int main, int side, int attempts = 20) => new DungeonConfig(main, side, 3, 2, 1, 5, attempts);

        [TestCaseSource(nameof(Configs))]
        public void ThousandSeeds_AreValid_WithoutFallback(int main, int side)
        {
            var config = Config(main, side);
            for (var seed = 0; seed < SeedCount; seed++)
            {
                var layout = DungeonGenerator.Generate(config, seed);
                Assert.IsTrue(LayoutValidator.Validate(layout, config, out var error), $"Seed {seed}: {error}");
                Assert.IsFalse(layout.IsFallback, $"Seed {seed} needed the fallback");
            }
        }

        [TestCaseSource(nameof(Configs))]
        public void SameSeed_ProducesSameLayout(int main, int side)
        {
            var config = Config(main, side);
            for (var seed = 0; seed < 200; seed++)
            {
                var first = DungeonGenerator.Generate(config, seed);
                var second = DungeonGenerator.Generate(config, seed);
                Assert.AreEqual(first.Signature, second.Signature);
                Assert.IsTrue(first.Rooms.Select(r => r.InteriorVariant).SequenceEqual(second.Rooms.Select(r => r.InteriorVariant)));
            }
        }

        [TestCaseSource(nameof(Configs))]
        public void ConsecutiveAttempts_AlwaysChangeTheLayout(int main, int side)
        {
            var config = Config(main, side);
            var random = new SeededRandom(99);
            var previous = DungeonGenerator.Generate(config, 12345).Signature;
            for (var i = 0; i < SeedCount; i++)
            {
                var layout = DungeonGenerator.GenerateDifferent(config, random.Next(int.MaxValue), previous);
                Assert.AreNotEqual(previous, layout.Signature, $"Attempt {i} repeated the layout");
                previous = layout.Signature;
            }
        }

        [Test]
        public void Seeds_ProduceManyDistinctLayouts()
        {
            var config = Config(5, 1);
            var distinct = Enumerable.Range(0, SeedCount).Select(seed => DungeonGenerator.Generate(config, seed).Signature).Distinct().Count();
            Assert.Greater(distinct, SeedCount / 4);
        }

        [TestCaseSource(nameof(Configs))]
        public void ForcedFailure_UsesValidFallback(int main, int side)
        {
            var config = Config(main, side, 0);
            var layout = DungeonGenerator.Generate(config, 7);
            Assert.IsTrue(layout.IsFallback);
            Assert.IsTrue(LayoutValidator.Validate(layout, config, out var error), error);
        }

        [Test]
        public void MultipleSideRooms_FallbackRemainsValid()
        {
            var config = Config(5, 8, 0);
            var layout = DungeonGenerator.Generate(config, 3);
            Assert.IsTrue(LayoutValidator.Validate(layout, config, out var error), error);
        }

        [Test]
        public void Generation_P95_IsUnderTenMilliseconds()
        {
            var config = Config(7, 1);
            var times = new double[SeedCount];
            for (var seed = 0; seed < SeedCount; seed++)
            {
                var timer = Stopwatch.StartNew();
                DungeonGenerator.Generate(config, seed);
                times[seed] = timer.Elapsed.TotalMilliseconds;
            }

            System.Array.Sort(times);
            Assert.Less(times[(int)(SeedCount * 0.95)], 10.0);
        }

        [Test]
        public void EncounterSize_GrowsAlongThePath()
        {
            var layout = DungeonGenerator.Generate(Config(7, 1), 21);
            var counts = layout.Rooms.Where(r => r.Kind == RoomKind.Combat).OrderBy(r => r.Depth).Select(r => r.EnemyCount).ToArray();
            for (var i = 1; i < counts.Length; i++) Assert.GreaterOrEqual(counts[i], counts[i - 1]);
            Assert.AreEqual(2, counts[0]);
        }
    }
}
