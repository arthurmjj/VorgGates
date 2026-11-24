using UnityEngine;
using VorgGates.Core;

namespace VorgGates.Gameplay
{
    [CreateAssetMenu(menuName = "Vorg'Gates/Weapon", fileName = "Weapon")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        [SerializeField] private WeaponKind kind;
        [SerializeField] private string displayName = "Espada";
        [SerializeField, Min(1)] private int damage = 20;
        [SerializeField, Min(0.01f)] private float windup = 0.08f;
        [SerializeField, Min(0.01f)] private float active = 0.08f;
        [SerializeField, Min(0.01f)] private float recovery = 0.19f;
        [SerializeField, Min(0.1f)] private float range = 2.2f;
        [SerializeField, Range(1f, 360f)] private float arcDegrees = 110f;
        [SerializeField, Min(0f)] private float knockback = 5f;
        [SerializeField, Min(0)] private int comboEvery;
        [SerializeField, Min(0)] private int comboDamage;
        [SerializeField] private Projectile projectile;
        [SerializeField, Min(0f)] private float projectileSpeed = 18f;

        public WeaponKind Kind => kind;
        public string DisplayName => displayName;
        public int Damage => damage;
        public float Windup => windup;
        public float Active => active;
        public float Recovery => recovery;
        public float Interval => windup + active + recovery;
        public float Range => range;
        public float ArcDegrees => arcDegrees;
        public float Knockback => knockback;
        public int ComboEvery => comboEvery;
        public int ComboDamage => comboDamage;
        public Projectile Projectile => projectile;
        public float ProjectileSpeed => projectileSpeed;
        public bool IsRanged => projectile != null;
    }
}
