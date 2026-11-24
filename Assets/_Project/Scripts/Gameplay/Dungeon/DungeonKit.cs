using UnityEngine;

namespace VorgGates.Gameplay
{
    /// <summary>All prefabs that give one campaign phase its own look.</summary>
    [CreateAssetMenu(menuName = "Vorg'Gates/Dungeon Kit", fileName = "Kit")]
    public sealed class DungeonKit : ScriptableObject
    {
        [SerializeField] private RoomView roomPrefab;
        [SerializeField] private GameObject corridorPrefab;
        [SerializeField] private RoomInterior entranceInterior;
        [SerializeField] private RoomInterior exitInterior;
        [SerializeField] private RoomInterior rewardInterior;
        [SerializeField] private RoomInterior bossInterior;
        [SerializeField] private RoomInterior[] combatInteriors;
        [SerializeField] private DoorController tallDoor;
        [SerializeField] private DoorController lowDoor;
        [SerializeField] private PrototypePortal portalPrefab;
        [SerializeField] private EnemyBrain[] enemyPrefabs;

        public RoomView RoomPrefab => roomPrefab;
        public GameObject CorridorPrefab => corridorPrefab;
        public RoomInterior EntranceInterior => entranceInterior;
        public RoomInterior ExitInterior => exitInterior;
        public RoomInterior RewardInterior => rewardInterior;
        public RoomInterior BossInterior => bossInterior;
        public RoomInterior[] CombatInteriors => combatInteriors;
        public DoorController TallDoor => tallDoor;
        public DoorController LowDoor => lowDoor;
        public PrototypePortal PortalPrefab => portalPrefab;
        public EnemyBrain[] EnemyPrefabs => enemyPrefabs;
    }
}
