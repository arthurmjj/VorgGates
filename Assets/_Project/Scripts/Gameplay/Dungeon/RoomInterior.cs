using UnityEngine;

namespace VorgGates.Gameplay
{
    /// <summary>Reusable room contents with their spawn and anchor points.</summary>
    public sealed class RoomInterior : MonoBehaviour
    {
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private Transform anchor;

        public Transform[] SpawnPoints => spawnPoints;
        public Transform Anchor => anchor;
    }
}
