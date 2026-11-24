using NUnit.Framework;
using UnityEngine;
using VorgGates.Core;
using VorgGates.Gameplay;

namespace VorgGates.Tests
{
    public sealed class CoreRulesTests
    {
        [Test]
        public void Health_DiesOnce_AndRestores()
        {
            var health = new HealthModel(100);
            var deaths = 0;
            health.Died += () => deaths++;

            Assert.IsTrue(health.TakeDamage(60));
            Assert.AreEqual(40, health.Current);
            Assert.IsTrue(health.TakeDamage(60));
            Assert.AreEqual(0, health.Current);
            Assert.IsFalse(health.TakeDamage(10));
            Assert.AreEqual(1, deaths);

            health.Restore();
            Assert.AreEqual(100, health.Current);
            Assert.IsFalse(health.IsDead);
        }

        [TestCase(GameFlowState.Loading, GameFlowState.Playing, true)]
        [TestCase(GameFlowState.Playing, GameFlowState.Dying, true)]
        [TestCase(GameFlowState.Playing, GameFlowState.Victory, true)]
        [TestCase(GameFlowState.Dying, GameFlowState.Loading, true)]
        [TestCase(GameFlowState.Victory, GameFlowState.Loading, true)]
        [TestCase(GameFlowState.Dying, GameFlowState.Victory, false)]
        [TestCase(GameFlowState.Victory, GameFlowState.Dying, false)]
        [TestCase(GameFlowState.Loading, GameFlowState.Victory, false)]
        public void GameFlow_AllowsOnlyValidTransitions(GameFlowState from, GameFlowState to, bool expected)
        {
            Assert.AreEqual(expected, GameFlow.IsAllowed(from, to));
        }

        [Test]
        public void IsoMovement_MapsAllEightDirections_WithinOneDegree()
        {
            var right = new Vector3(1f, 0f, -1f).normalized;
            var up = new Vector3(1f, 0f, 1f).normalized;
            for (var x = -1; x <= 1; x++)
            for (var y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;
                var world = IsoMovement.ToWorld(new Vector2(x, y), 45f);
                var expected = (right * x + up * y).normalized;
                Assert.LessOrEqual(Vector3.Angle(world, expected), 1f, $"Input ({x}, {y})");
                Assert.AreEqual(1f, world.magnitude, 0.001f, $"Input ({x}, {y}) must not move faster on diagonals");
            }
        }
    }
}
