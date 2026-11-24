using UnityEngine;

namespace VorgGates.Presentation
{
    public enum SfxId
    {
        SwingSword,
        SwingAxe,
        CastStaff,
        Hit,
        PlayerHurt,
        Dodge,
        EnemyDeath,
        Arrow,
        Telegraph,
        DoorClose,
        DoorOpen,
        Portal,
        Pickup,
        Heal,
        BossSpawn,
        BossSlam,
        BossDeath,
        Victory,
        UiMove,
        UiClick
    }

    [CreateAssetMenu(menuName = "Vorg'Gates/Audio Library", fileName = "AudioLibrary")]
    public sealed class AudioLibrary : ScriptableObject
    {
        [SerializeField] private AudioClip[] effects;
        [SerializeField] private AudioClip[] music;

        public AudioClip[] Effects => effects;
        public AudioClip[] Music => music;
        public AudioClip Effect(SfxId id) => effects[(int)id];
    }
}
