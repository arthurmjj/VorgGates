using System;

namespace VorgGates.Core
{
    public enum EnemyKind
    {
        Crawler,
        Archer,
        Brute,
        Boss
    }

    /// <summary>Chooses enemy types: each new type appears later on the path and at higher tiers.</summary>
    public static class EncounterPlanner
    {
        public static EnemyKind[] Plan(int combatIndex, int count, int tier)
        {
            var encounter = new EnemyKind[count];
            var brutes = tier >= 3 && combatIndex >= 2 ? (count >= 6 ? 2 : 1) : 0;
            var archers = tier >= 2 && combatIndex >= 1 ? Math.Max(1, count / 3) : 0;
            brutes = Math.Min(brutes, count);
            archers = Math.Min(archers, count - brutes);

            var cursor = 0;
            for (var i = 0; i < brutes; i++) encounter[cursor++] = EnemyKind.Brute;
            for (var i = 0; i < archers; i++) encounter[cursor++] = EnemyKind.Archer;
            while (cursor < count) encounter[cursor++] = EnemyKind.Crawler;
            return encounter;
        }
    }
}
