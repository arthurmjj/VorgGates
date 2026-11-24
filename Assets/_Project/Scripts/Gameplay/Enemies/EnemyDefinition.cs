using UnityEngine;
using VorgGates.Core;

namespace VorgGates.Gameplay
{
    public enum EnemyBehavior
    {
        MeleeLunge,
        Ranged,
        Charger,
        Boss
    }

    [CreateAssetMenu(menuName = "Vorg'Gates/Enemy", fileName = "Enemy")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        [SerializeField] private EnemyKind kind;
        [SerializeField] private EnemyBehavior behavior;
        [SerializeField] private string displayName = "Rastejante";
        [SerializeField, Min(1)] private int maxHealth = 40;
        [SerializeField, Min(1)] private int damage = 10;
        [SerializeField, Min(0.1f)] private float moveSpeed = 3.6f;
        [SerializeField, Min(0.1f)] private float attackRange = 1.9f;
        [SerializeField, Min(0f)] private float preferredMinRange;
        [SerializeField, Min(0.05f)] private float telegraph = 0.4f;
        [SerializeField, Min(0f)] private float lungeDistance = 2.2f;
        [SerializeField, Min(0.02f)] private float lungeDuration = 0.14f;
        [SerializeField, Min(0.1f)] private float hitRadius = 1.6f;
        [SerializeField, Min(0.05f)] private float recovery = 0.75f;
        [SerializeField, Range(0f, 1f)] private float knockbackResistance;
        [SerializeField] private Projectile projectile;
        [SerializeField, Min(0f)] private float projectileSpeed = 11f;
        [SerializeField, Min(0f)] private float barHeight = 3.4f;
        [SerializeField] private BossPattern[] patterns = System.Array.Empty<BossPattern>();

        public EnemyKind Kind => kind;
        public EnemyBehavior Behavior => behavior;
        public string DisplayName => displayName;
        public int MaxHealth => maxHealth;
        public int Damage => damage;
        public float MoveSpeed => moveSpeed;
        public float AttackRange => attackRange;
        public float PreferredMinRange => preferredMinRange;
        public float Telegraph => telegraph;
        public float LungeDistance => lungeDistance;
        public float LungeDuration => lungeDuration;
        public float HitRadius => hitRadius;
        public float Recovery => recovery;
        public float KnockbackResistance => knockbackResistance;
        public Projectile Projectile => projectile;
        public float ProjectileSpeed => projectileSpeed;
        public float BarHeight => barHeight;
        public BossPattern[] Patterns => patterns;

#if UNITY_EDITOR
        public void EditorSetPatterns(BossPattern[] newPatterns) => patterns = newPatterns;
#endif
    }
}
