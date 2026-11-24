using UnityEngine;
using UnityEngine.UIElements;
using VorgGates.Core;
using VorgGates.Gameplay;

namespace VorgGates.Presentation
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class PrototypeHud : MonoBehaviour
    {

        private static readonly string[] PhaseTaglines =
        {
            "Os mortos guardam o primeiro portão",
            "O ferro ainda lembra o fogo de Vorg",
            "Só cinzas restam diante da lâmina"
        };

        private readonly VisualElement[] slots = new VisualElement[InventoryState.SlotCount];
        private readonly Label[] slotNames = new Label[InventoryState.SlotCount];
        private VisualElement healthFill;
        private VisualElement dodgeFill;
        private VisualElement overlay;
        private VisualElement bossBar;
        private VisualElement bossFill;
        private Label bossName;
        private Label bossPercent;
        private VisualElement hudRoot;
        private VisualElement phaseCard;
        private Label phaseNumber;
        private Label phaseName;
        private Label phaseTagline;
        private IVisualElementScheduledItem phaseCardHide;
        private Label healthText;
        private Label objectiveTitle;
        private Label objective;
        private Label prompt;
        private Label overlayTitle;
        private Label overlaySubtitle;

        private GameFlow flow;
        private PlayerStateMachine player;
        private DungeonDirector director;
        private EnemyBrain shownBoss;
        private int shownHealth = -1;
        private int shownBossHealth = -1;
        private int shownObjectiveKey = int.MinValue;
        private string shownPrompt;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            healthFill = root.Q("health-fill");
            dodgeFill = root.Q("dodge-fill");
            overlay = root.Q("overlay");
            bossBar = root.Q("boss-bar");
            bossFill = root.Q("boss-fill");
            bossName = root.Q<Label>("boss-name");
            bossPercent = root.Q<Label>("boss-percent");
            hudRoot = root.Q("hud-root");
            phaseCard = root.Q("phase-card");
            phaseNumber = root.Q<Label>("phase-number");
            phaseName = root.Q<Label>("phase-name");
            phaseTagline = root.Q<Label>("phase-tagline");
            healthText = root.Q<Label>("health-text");
            objectiveTitle = root.Q<Label>("objective-title");
            objective = root.Q<Label>("objective");
            prompt = root.Q<Label>("prompt");
            overlayTitle = root.Q<Label>("overlay-title");
            overlaySubtitle = root.Q<Label>("overlay-subtitle");
            for (var i = 0; i < InventoryState.SlotCount; i++)
            {
                slots[i] = root.Q($"slot-{i}");
                slotNames[i] = root.Q<Label>($"slot-name-{i}");
            }
        }

        private void OnDestroy()
        {
            if (flow != null) flow.Changed -= OnFlowChanged;
            if (player != null) player.Inventory.Changed -= RefreshSlots;
        }

        public void Bind(GameFlow gameFlow, PlayerStateMachine playerState, DungeonDirector dungeonDirector)
        {
            flow = gameFlow;
            player = playerState;
            director = dungeonDirector;
            flow.Changed += OnFlowChanged;
            player.Inventory.Changed += RefreshSlots;
            OnFlowChanged(flow.State);
            RefreshSlots();
        }

        public bool IsPhaseCardVisible => phaseCard.ClassListContains("phase-card--visible");
        public string PhaseCardTitle => phaseName.text;

        public void ShowPhaseCard(int dungeonIndex, string displayName)
        {
            phaseCardHide?.Pause();
            phaseNumber.text = $"FASE {dungeonIndex + 1}";
            phaseName.text = displayName;
            phaseTagline.text = PhaseTaglines[dungeonIndex];
            phaseCard.AddToClassList("phase-card--visible");
        }

        public void HidePhaseCard(int delayMilliseconds)
        {
            phaseCardHide?.Pause();
            if (delayMilliseconds <= 0)
            {
                phaseCard.RemoveFromClassList("phase-card--visible");
                return;
            }

            phaseCardHide = phaseCard.schedule.Execute(() => phaseCard.RemoveFromClassList("phase-card--visible")).StartingIn(delayMilliseconds);
        }

        public void SetHudVisible(bool visible) => hudRoot.EnableInClassList("hud-root--hidden", !visible);

        public void ShowVictory(float seconds, int deaths)
        {
            var total = Mathf.RoundToInt(seconds);
            ShowOverlay("A Lâmina de Vorg é sua", $"Tempo: {total / 60:00}:{total % 60:00}   ·   Mortes: {deaths}   ·   Pressione E para jogar novamente");
        }

        private void RefreshSlots()
        {
            var inventory = player.Inventory;
            for (var i = 0; i < slots.Length; i++)
            {
                var kind = (WeaponKind)i;
                var owned = inventory.Owns(kind);
                slots[i].EnableInClassList("slot--owned", owned);
                slots[i].EnableInClassList("slot--equipped", owned && inventory.Equipped == kind);
                slotNames[i].text = owned ? inventory.Definition(kind).DisplayName : "---";
            }
        }

        private void Update()
        {
            if (flow == null || director.Current == null) return;

            if (player.Health.Current != shownHealth)
            {
                shownHealth = player.Health.Current;
                healthFill.style.width = Length.Percent(player.Health.Normalized * 100f);
                healthText.text = $"{shownHealth} / {player.Health.Max}";
            }

            dodgeFill.style.width = Length.Percent(player.Dodge.CooldownProgress * 100f);
            UpdateObjective();

            var interactable = flow.State == GameFlowState.Playing ? player.Interactor.Current : null;
            var promptText = interactable?.Prompt;
            if (promptText == shownPrompt) return;
            shownPrompt = promptText;
            prompt.text = promptText ?? string.Empty;
            prompt.style.display = promptText == null ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private void LateUpdate()
        {
            if (flow == null) return;
            var boss = flow.State == GameFlowState.Playing ? director.ActiveBoss : null;
            if (boss != shownBoss)
            {
                shownBoss = boss;
                shownBossHealth = -1;
                bossBar.style.display = boss == null ? DisplayStyle.None : DisplayStyle.Flex;
                if (boss != null) bossName.text = boss.Definition.DisplayName;
            }

            if (boss == null || bossBar.panel == null) return;
            if (boss.Health.Current != shownBossHealth)
            {
                shownBossHealth = boss.Health.Current;
                bossFill.style.width = Length.Percent(boss.Health.Normalized * 100f);
                bossPercent.text = $"{Mathf.CeilToInt(boss.Health.Normalized * 100f)}%";
                bossFill.EnableInClassList("boss-fill--enraged", boss.IsEnraged);
            }
        }

        private void UpdateObjective()
        {
            var active = director.ActiveRoom;
            var isBoss = director.ActiveBoss != null;
            var enemies = active != null ? 1000 + active.AliveEnemies : director.CombatRoomsRemaining;
            var key = director.DungeonIndex * 10000 + (isBoss ? 5000 : 0) + enemies;
            if (key == shownObjectiveKey) return;
            shownObjectiveKey = key;

            objectiveTitle.text = $"FASE {director.DungeonIndex + 1}  ·  {director.Definition.DisplayName.ToUpperInvariant()}";
            if (isBoss) objective.text = $"Derrote: {director.ActiveBoss.Definition.DisplayName}";
            else if (active != null) objective.text = $"Derrote os inimigos: {active.AliveEnemies}";
            else if (director.CombatRoomsRemaining > 1) objective.text = $"Avance: {director.CombatRoomsRemaining - 1} combates até o chefão";
            else if (director.CombatRoomsRemaining == 1) objective.text = "Encontre o chefão";
            else objective.text = director.Current.Sword != null ? "Retire a Lâmina de Vorg" : "Atravesse o portal";
        }

        private void OnFlowChanged(GameFlowState state)
        {
            switch (state)
            {
                case GameFlowState.Dying:
                    ShowOverlay("Você caiu", "A masmorra se reorganiza...");
                    break;
                case GameFlowState.Transition:
                case GameFlowState.Victory:
                    break;
                default:
                    overlay.style.display = DisplayStyle.None;
                    break;
            }
        }

        private void ShowOverlay(string title, string subtitle)
        {
            overlayTitle.text = title;
            overlaySubtitle.text = subtitle;
            overlay.style.display = DisplayStyle.Flex;
        }
    }
}
