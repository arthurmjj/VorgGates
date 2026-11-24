using System.Collections;
using UnityEngine;
using VorgGates.Core;
using VorgGates.Gameplay;
using VorgGates.Presentation;

namespace VorgGates.App
{
    /// <summary>Composition root: wires the systems and owns the menu and campaign loop.</summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        private const float VictoryInputDelay = 1.2f;
        private const float TransitionDelay = 1.4f;
        private const int TransitionCardMilliseconds = 700;
        private const int FirstCardMilliseconds = 1500;

        [SerializeField] private PlayerStateMachine player;
        [SerializeField] private DungeonDirector director;
        [SerializeField] private CameraRig cameraRig;
        [SerializeField] private ThemeDirector themeDirector;
        [SerializeField] private PrototypeHud hud;
        [SerializeField] private MenuController menu;
        [SerializeField] private AudioDirector audioDirector;
        [SerializeField] private float deathDelay = 0.7f;

        private float victoryTime;
        private float campaignSeconds;
        private float timeScaleBeforePause = 1f;
        private bool campaignInProgress;

        public GameFlow Flow { get; } = new GameFlow();
        public RunSession Session { get; private set; }
        public PlayerStateMachine Player => player;
        public DungeonDirector Director => director;
        public ThemeDirector Theme => themeDirector;
        public MenuController Menu => menu;
        public AudioDirector Audio => audioDirector;
        public PrototypeHud Hud => hud;
        public bool IsReady { get; private set; }

        private void Start()
        {
            Time.timeScale = 1f;
            Session = new RunSession(player.Inventory.State);
            player.Health.Died += OnPlayerDied;
            director.PortalUsed += OnPortalUsed;
            director.SwordTaken += OnSwordTaken;
            director.Initialize(player);
            hud.Bind(Flow, player, director);
            audioDirector.Bind(player, director, menu);
            menu.NewGameRequested += StartNewCampaign;
            menu.ContinueRequested += ContinueCampaign;
            menu.ResumeRequested += ResumeGame;
            menu.BackToMenuRequested += ReturnToMenu;
            menu.QuitRequested += Application.Quit;

            if (!BuildDungeon(0, null) || !Flow.TryEnter(GameFlowState.Menu))
            {
                Debug.LogError("Vorg'Gates: failed to build the dungeon.");
                return;
            }

            EnterMenu();
            IsReady = true;
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
            player.Health.Died -= OnPlayerDied;
            director.PortalUsed -= OnPortalUsed;
            director.SwordTaken -= OnSwordTaken;
            menu.NewGameRequested -= StartNewCampaign;
            menu.ContinueRequested -= ContinueCampaign;
            menu.ResumeRequested -= ResumeGame;
            menu.BackToMenuRequested -= ReturnToMenu;
            menu.QuitRequested -= Application.Quit;
        }

        private void Update()
        {
            if (Flow.State == GameFlowState.Playing)
            {
                campaignSeconds += Time.unscaledDeltaTime;
                if (player.Input.ConsumePause() && Time.frameCount != menu.LastCloseFrame) PauseGame();
                return;
            }

            if (Flow.State != GameFlowState.Victory || Time.unscaledTime < victoryTime + VictoryInputDelay) return;
            if (!player.Input.ConsumeInteract()) return;
            Session.RestartCampaign();
            player.Inventory.Refresh();
            campaignSeconds = 0f;
            campaignInProgress = true;
            if (Flow.TryEnter(GameFlowState.Loading)) BeginAttempt(null);
        }

        public void StartNewCampaign()
        {
            if (Flow.State != GameFlowState.Menu) return;
            Session.RestartCampaign();
            player.Inventory.Refresh();
            campaignSeconds = 0f;
            campaignInProgress = true;
            if (!Flow.TryEnter(GameFlowState.Loading) || !BeginAttempt(null)) return;
            ShowPhaseCard(0, FirstCardMilliseconds);
        }

        public void ContinueCampaign()
        {
            if (Flow.State != GameFlowState.Menu || !campaignInProgress) return;
            Session.RestoreCheckpoint();
            player.Inventory.Refresh();
            if (!Flow.TryEnter(GameFlowState.Loading) || !BeginAttempt(null)) return;
            ShowPhaseCard(Session.DungeonIndex, FirstCardMilliseconds);
        }

        public void PauseGame()
        {
            if (!Flow.TryEnter(GameFlowState.Paused)) return;
            timeScaleBeforePause = Time.timeScale;
            Time.timeScale = 0f;
            player.SetControlsEnabled(false);
            menu.Show(MenuScreen.Pause);
        }

        public void ResumeGame()
        {
            if (!Flow.TryEnter(GameFlowState.Playing)) return;
            Time.timeScale = timeScaleBeforePause;
            menu.Hide();
            player.SetControlsEnabled(true);
        }

        public void ReturnToMenu()
        {
            if (!Flow.TryEnter(GameFlowState.Menu)) return;
            StopAllCoroutines();
            Time.timeScale = 1f;
            Session.RestoreCheckpoint();
            player.Inventory.Refresh();
            BuildDungeon(Session.DungeonIndex, null);
            EnterMenu();
        }

        private void EnterMenu()
        {
            player.SetControlsEnabled(false);
            player.Health.SetLocked(true);
            hud.SetHudVisible(false);
            hud.HidePhaseCard(0);
            audioDirector.PlayMusic(Session.DungeonIndex);
            menu.Show(MenuScreen.Main, campaignInProgress);
        }

        /// <summary>Restarts the current phase on a specific layout, so a reported seed can be reproduced.</summary>
        public bool RestartWithSeed(int seed)
        {
            StopAllCoroutines();
            if (Session.IsTransitioning) Session.AbortTransition();
            if (Flow.State == GameFlowState.Paused)
            {
                Time.timeScale = timeScaleBeforePause;
                Flow.TryEnter(GameFlowState.Menu);
            }

            if (Flow.State == GameFlowState.Playing) Flow.TryEnter(GameFlowState.Dying);
            if (Flow.State != GameFlowState.Loading && !Flow.TryEnter(GameFlowState.Loading)) return false;
            campaignInProgress = true;
            return BeginAttempt(seed);
        }

        /// <summary>Moves the session to a phase with the current weapons, for tests and trailer capture.</summary>
        public bool JumpToDungeon(int dungeonIndex, int seed)
        {
            Session.JumpTo(dungeonIndex);
            return RestartWithSeed(seed);
        }

        private bool BeginAttempt(int? seed)
        {
            player.SetControlsEnabled(false);
            if (!BuildDungeon(Session.DungeonIndex, seed)) return false;
            menu.Hide();
            hud.SetHudVisible(true);
            audioDirector.PlayMusic(Session.DungeonIndex);
            Flow.TryEnter(GameFlowState.Playing);
            player.SetControlsEnabled(true);
            return true;
        }

        private bool BuildDungeon(int dungeonIndex, int? seed)
        {
            if (!director.Rebuild(dungeonIndex, seed)) return false;
            themeDirector.Apply(dungeonIndex);
            player.ResetTo(director.Current.PlayerStart);
            cameraRig.Snap();
            return true;
        }

        private void ShowPhaseCard(int dungeonIndex, int holdMilliseconds)
        {
            hud.ShowPhaseCard(dungeonIndex, director.DefinitionAt(dungeonIndex).DisplayName);
            hud.HidePhaseCard(holdMilliseconds);
        }

        private void OnPlayerDied()
        {
            if (!Flow.TryEnter(GameFlowState.Dying)) return;
            player.SetControlsEnabled(false);
            StartCoroutine(RestartAfterDeath());
        }

        private IEnumerator RestartAfterDeath()
        {
            yield return new WaitForSeconds(deathDelay);
            if (!Flow.TryEnter(GameFlowState.Loading)) yield break;
            Session.RegisterDeath();
            player.Inventory.Refresh();
            BeginAttempt(null);
        }

        private void OnPortalUsed()
        {
            if (Session.IsFinalDungeon || !Flow.TryEnter(GameFlowState.Transition)) return;
            Session.BeginTransition();
            player.Health.SetLocked(true);
            player.SetControlsEnabled(false);
            director.Current.Portal.SetAvailable(false);
            hud.ShowPhaseCard(Session.PendingDungeon, director.DefinitionAt(Session.PendingDungeon).DisplayName);
            StartCoroutine(TransitionToNextDungeon());
        }

        private IEnumerator TransitionToNextDungeon()
        {
            yield return new WaitForSeconds(TransitionDelay);
            if (BuildDungeon(Session.PendingDungeon, null))
            {
                // The checkpoint moves only after the next dungeon exists and the player is inside it.
                Session.CommitCheckpoint();
                audioDirector.PlayMusic(Session.DungeonIndex);
                Flow.TryEnter(GameFlowState.Playing);
                player.SetControlsEnabled(true);
                hud.HidePhaseCard(TransitionCardMilliseconds);
                yield break;
            }

            Session.AbortTransition();
            hud.HidePhaseCard(0);
            if (Flow.TryEnter(GameFlowState.Loading)) BeginAttempt(null);
        }

        private void OnSwordTaken()
        {
            if (!Flow.TryEnter(GameFlowState.Victory)) return;
            victoryTime = Time.unscaledTime;
            campaignInProgress = false;
            player.Health.SetLocked(true);
            player.SetControlsEnabled(false);
            hud.ShowVictory(campaignSeconds, Session.Deaths);
        }
    }
}
