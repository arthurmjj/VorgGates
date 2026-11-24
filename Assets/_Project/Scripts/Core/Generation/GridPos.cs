using System;

namespace VorgGates.Core
{
    public enum Side
    {
        North,
        East,
        South,
        West
    }

    public readonly struct GridPos : IEquatable<GridPos>
    {
        public GridPos(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }
        public int Y { get; }

        public GridPos Step(Side side)
        {
            switch (side)
            {
                case Side.North: return new GridPos(X, Y + 1);
                case Side.East: return new GridPos(X + 1, Y);
                case Side.South: return new GridPos(X, Y - 1);
                default: return new GridPos(X - 1, Y);
            }
        }

        public bool Equals(GridPos other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is GridPos other && Equals(other);
        public override int GetHashCode() => unchecked((X * 397) ^ Y);
        public override string ToString() => $"({X}, {Y})";
    }

    public static class SideUtil
    {
        public const int Count = 4;

        public static Side Opposite(this Side side) => (Side)(((int)side + 2) % Count);
        public static int Bit(this Side side) => 1 << (int)side;
    }
}
