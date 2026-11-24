using System;
using System.Collections.Generic;

namespace VorgGates.Core
{
    /// <summary>Small deterministic generator, so any layout can be reproduced from its seed.</summary>
    public sealed class SeededRandom
    {
        private uint state;

        public SeededRandom(int seed)
        {
            var mixed = unchecked((uint)seed * 0x9E3779B9u + 0x6C078965u);
            mixed ^= mixed >> 15;
            mixed = unchecked(mixed * 0x85EBCA77u);
            mixed ^= mixed >> 13;
            state = mixed == 0u ? 0xA3C59AC3u : mixed;
            for (var i = 0; i < 4; i++) NextUInt();
        }

        public int Next(int maxExclusive)
        {
            if (maxExclusive <= 0) throw new ArgumentOutOfRangeException(nameof(maxExclusive));
            return (int)(NextUInt() % (uint)maxExclusive);
        }

        public void Shuffle<T>(IList<T> items)
        {
            for (var i = items.Count - 1; i > 0; i--)
            {
                var j = Next(i + 1);
                var value = items[i];
                items[i] = items[j];
                items[j] = value;
            }
        }

        private uint NextUInt()
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return state;
        }
    }
}
