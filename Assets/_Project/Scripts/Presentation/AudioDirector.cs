using UnityEngine;
using VorgGates.Core;
using VorgGates.Gameplay;

namespace VorgGates.Presentation
{
    /// <summary>Plays sound in response to game events, so gameplay code never depends on audio.</summary>
    public sealed class AudioDirector : MonoBehaviour
    {
        private const int Voices = 12;
        private const float RepeatGuard = 0.045f;
        private const float MusicVolume = 0.34f;

        [SerializeField] private AudioLibrary library;

        private readonly AudioSource[] voices = new AudioSource[Voices];
        private float[] lastPlayed;
        private AudioSource musicSource;
        private PlayerStateMachine player;
        private DungeonDirector director;
        private MenuController menu;
        private int nextVoice;

        public int EffectsPlayed { get; private set; }
        public int CurrentMusic { get; private set; } = -1;
        public bool IsMusicPlaying => musicSource != null && musicSource.isPlaying;
        public AudioLibrary Library => library;

        private void Awake()
        {
            lastPlayed = new float[library.Effects.Length];
            for (var i = 0; i < lastPlayed.Length; i++) lastPlayed[i] = -1f;
            for (var i = 0; i < Voices; i++)
            {
                voices[i] = gameObject.AddComponent<AudioSource>();
                voices[i].playOnAwake = false;
                voices[i].spatialBlend = 0f;
            }

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.volume = MusicVolume;
        }

        public void Bind(PlayerStateMachine playerState, DungeonDirector dungeonDirector, MenuController menuController)
        {
            player = playerState;
            director = dungeonDirector;
            menu = menuController;
            player.Weapon.SwingStarted += OnSwing;
            player.Dodge.Started += OnDodge;
            player.Health.Damaged += OnPlayerHurt;
            player.Health.Healed += OnHealed;
            player.Inventory.WeaponGained += OnWeaponGained;
            director.RoomStarted += OnRoomStarted;
            director.RoomFinished += OnRoomFinished;
            director.EnemySpawned += OnEnemySpawned;
            director.PortalUsed += OnPortal;
            director.SwordTaken += OnVictory;
            menu.SelectionMoved += OnMenuMoved;
            menu.Confirmed += OnMenuConfirmed;
        }

        private void OnDestroy()
        {
            if (player == null) return;
            player.Weapon.SwingStarted -= OnSwing;
            player.Dodge.Started -= OnDodge;
            player.Health.Damaged -= OnPlayerHurt;
            player.Health.Healed -= OnHealed;
            player.Inventory.WeaponGained -= OnWeaponGained;
            director.RoomStarted -= OnRoomStarted;
            director.RoomFinished -= OnRoomFinished;
            director.EnemySpawned -= OnEnemySpawned;
            director.PortalUsed -= OnPortal;
            director.SwordTaken -= OnVictory;
            menu.SelectionMoved -= OnMenuMoved;
            menu.Confirmed -= OnMenuConfirmed;
        }

        public void Play(SfxId id, float volume = 0.8f)
        {
            var index = (int)id;
            var now = Time.unscaledTime;
            if (lastPlayed[index] >= 0f && now - lastPlayed[index] < RepeatGuard) return;
            lastPlayed[index] = now;
            var voice = voices[nextVoice];
            nextVoice = (nextVoice + 1) % Voices;
            voice.pitch = Random.Range(0.96f, 1.04f);
            voice.PlayOneShot(library.Effect(id), volume);
            EffectsPlayed++;
        }

        public void PlayMusic(int dungeonIndex)
        {
            if (CurrentMusic == dungeonIndex && musicSource.isPlaying) return;
            CurrentMusic = dungeonIndex;
            musicSource.clip = library.Music[dungeonIndex];
            musicSource.Play();
        }

        private void OnSwing()
        {
            switch (player.Weapon.Weapon.Kind)
            {
                case WeaponKind.Axe: Play(SfxId.SwingAxe); break;
                case WeaponKind.Staff: Play(SfxId.CastStaff, 0.65f); break;
                default: Play(SfxId.SwingSword, 0.7f); break;
            }
        }

        private void OnDodge() => Play(SfxId.Dodge, 0.55f);
        private void OnPlayerHurt(DamageInfo damage) => Play(SfxId.PlayerHurt);
        private void OnHealed() => Play(SfxId.Heal, 0.55f);
        private void OnWeaponGained() => Play(SfxId.Pickup);
        private void OnRoomStarted() => Play(SfxId.DoorClose);
        private void OnRoomFinished() => Play(SfxId.DoorOpen);
        private void OnPortal() => Play(SfxId.Portal);
        private void OnMenuMoved() => Play(SfxId.UiMove, 0.4f);
        private void OnMenuConfirmed() => Play(SfxId.UiClick, 0.6f);

        private void OnVictory()
        {
            musicSource.Stop();
            Play(SfxId.Victory, 1f);
        }

        private void OnEnemySpawned(EnemyBrain enemy)
        {
            if (enemy.IsBoss) Play(SfxId.BossSpawn, 1f);
            enemy.Health.Damaged += _ => Play(SfxId.Hit, 0.65f);
            enemy.Defeated += defeated => Play(defeated.IsBoss ? SfxId.BossDeath : SfxId.EnemyDeath, defeated.IsBoss ? 1f : 0.6f);
            enemy.StateChanged += state => OnEnemyState(enemy, state);
        }

        private void OnEnemyState(EnemyBrain enemy, EnemyState state)
        {
            if (state == EnemyState.Telegraph && (enemy.IsBoss || enemy.Definition.Behavior == EnemyBehavior.Charger))
                Play(SfxId.Telegraph, 0.6f);
            if (state != EnemyState.Attack) return;
            if (enemy.IsBoss) Play(SfxId.BossSlam, 0.95f);
            else if (enemy.Definition.Behavior == EnemyBehavior.Ranged) Play(SfxId.Arrow, 0.5f);
        }
    }
}
