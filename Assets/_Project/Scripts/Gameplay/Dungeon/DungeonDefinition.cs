using UnityEngine;
using VorgGates.Core;

namespace VorgGates.Gameplay
{
    [CreateAssetMenu(menuName = "Vorg'Gates/Dungeon", fileName = "Dungeon")]
    public sealed class DungeonDefinition : ScriptableObject
    {
        [SerializeField] private string displayName = "Cripta de Musgo";
        [SerializeField] private DungeonKit kit;
        [SerializeField, Min(4)] private int mainPathRooms = 6;
        [SerializeField, Min(0)] private int sideRooms = 1;
        [SerializeField, Min(1)] private int minEnemies = 2;
        [SerializeField, Min(0)] private int enemyStep = 1;
        [SerializeField, Min(1)] private int maxEnemies = 5;
        [SerializeField, Range(1, 3)] private int enemyTier = 2;
        [SerializeField, Min(0)] private int rewardHostIndex;
        [SerializeField] private WeaponKind rewardWeapon = WeaponKind.Axe;
        [SerializeField, Min(0.1f)] private float healthScale = 1f;
        [SerializeField, Min(0.1f)] private float damageScale = 1f;
        [SerializeField] private bool endsWithSword;

        public string DisplayName => displayName;
        public DungeonKit Kit => kit;
        public WeaponKind RewardWeapon => rewardWeapon;
        public float HealthScale => healthScale;
        public float DamageScale => damageScale;
        public bool EndsWithSword => endsWithSword;

        public DungeonConfig ToConfig(int combatVariants) =>
            new DungeonConfig(mainPathRooms, sideRooms, combatVariants, minEnemies, enemyStep, maxEnemies,
                enemyTier: enemyTier, hasBoss: true, exitKind: endsWithSword ? RoomKind.SwordChamber : RoomKind.Portal, rewardHostIndex: rewardHostIndex);
    }
}
