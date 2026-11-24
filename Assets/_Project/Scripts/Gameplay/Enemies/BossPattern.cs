using System;
using UnityEngine;

namespace VorgGates.Gameplay
{
    public enum BossPatternKind
    {
        Slam,
        Cone,
        Charge,
        Volley,
        Nova,
        Bursts,
        BlinkSlam
    }

    [Serializable]
    public sealed class BossPattern
    {
        public BossPatternKind kind;
        public int damage = 20;
        public float telegraph = 0.9f;
        public float recovery = 1f;
        public float duration = 0.2f;
        public float range = 5f;
        public float radius = 3.5f;
        public float arcDegrees = 90f;
        public float distance = 8f;
        public int count = 5;
        public float projectileSpeed = 10f;
    }

    public enum TelegraphShape
    {
        Circle,
        Line,
        Cone,
        Bursts
    }

    /// <summary>The real danger area of a boss attack, drawn on the floor before it lands.</summary>
    public sealed class TelegraphInfo
    {
        public TelegraphShape Shape;
        public Vector3[] Points = Array.Empty<Vector3>();
        public float Radius;
        public float Length;
        public float Width;
        public float ArcDegrees;
        public float Duration;
    }
}
