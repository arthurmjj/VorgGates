using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace VorgGates.Presentation
{
    public enum MenuScreen
    {
        None,
        Main,
        Pause,
        Controls
    }

    /// <summary>Main, pause and controls screens, driven directly by mouse and keyboard.</summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class MenuController : MonoBehaviour
    {
        private readonly List<(VisualElement element, Action action)> items = new List<(VisualElement, Action)>();
        private VisualElement mainScreen;
        private VisualElement pauseScreen;
        private VisualElement controlsScreen;
        private VisualElement continueButton;
        private VisualElement playButton;
        private VisualElement controlsButton;
        private VisualElement quitButton;
        private VisualElement resumeButton;
        private VisualElement pauseControlsButton;
        private VisualElement menuButton;
        private VisualElement backButton;
        private VisualElement credits;
        private MenuScreen returnScreen = MenuScreen.Main;
        private Vector2 lastPointer;
        private bool canContinue;
        private int selected;

        public MenuScreen Current { get; private set; } = MenuScreen.None;
        public int LastCloseFrame { get; private set; } = -1;
        public VisualElement Root { get; private set; }

        public event Action NewGameRequested;
        public event Action ContinueRequested;
        public event Action ResumeRequested;
        public event Action BackToMenuRequested;
        public event Action QuitRequested;
        public event Action SelectionMoved;
        public event Action Confirmed;

        private void OnEnable()
        {
            Root = GetComponent<UIDocument>().rootVisualElement;
            mainScreen = Root.Q("main-menu");
            pauseScreen = Root.Q("pause-menu");
            controlsScreen = Root.Q("controls-menu");
            continueButton = Root.Q("btn-continue");
            playButton = Root.Q("btn-play");
            controlsButton = Root.Q("btn-controls");
            quitButton = Root.Q("btn-quit");
            resumeButton = Root.Q("btn-resume");
            pauseControlsButton = Root.Q("btn-pause-controls");
            menuButton = Root.Q("btn-menu");
            backButton = Root.Q("btn-back");
            credits = Root.Q("menu-credits");
        }

        public void Show(MenuScreen screen, bool allowContinue = false)
        {
            if (screen == MenuScreen.Main) canContinue = allowContinue;
            if (screen != MenuScreen.Controls) returnScreen = screen;
            Current = screen;
            mainScreen.style.display = screen == MenuScreen.Main ? DisplayStyle.Flex : DisplayStyle.None;
            pauseScreen.style.display = screen == MenuScreen.Pause ? DisplayStyle.Flex : DisplayStyle.None;
            controlsScreen.style.display = screen == MenuScreen.Controls ? DisplayStyle.Flex : DisplayStyle.None;
            continueButton.style.display = canContinue ? DisplayStyle.Flex : DisplayStyle.None;

            // Credits belong to the main menu and its controls page, never to a paused game.
            var showCredits = screen == MenuScreen.Main || (screen == MenuScreen.Controls && returnScreen == MenuScreen.Main);
            credits.style.display = showCredits ? DisplayStyle.Flex : DisplayStyle.None;

            items.Clear();
            switch (screen)
            {
                case MenuScreen.Main:
                    if (canContinue) items.Add((continueButton, () => ContinueRequested?.Invoke()));
                    items.Add((playButton, () => NewGameRequested?.Invoke()));
                    items.Add((controlsButton, () => Show(MenuScreen.Controls)));
                    items.Add((quitButton, () => QuitRequested?.Invoke()));
                    break;
                case MenuScreen.Pause:
                    items.Add((resumeButton, () => ResumeRequested?.Invoke()));
                    items.Add((pauseControlsButton, () => Show(MenuScreen.Controls)));
                    items.Add((menuButton, () => BackToMenuRequested?.Invoke()));
                    break;
                case MenuScreen.Controls:
                    items.Add((backButton, () => Show(returnScreen)));
                    break;
            }

            Select(0);
        }

        public void Hide()
        {
            if (Current != MenuScreen.None) LastCloseFrame = Time.frameCount;
            Current = MenuScreen.None;
            items.Clear();
            mainScreen.style.display = DisplayStyle.None;
            pauseScreen.style.display = DisplayStyle.None;
            controlsScreen.style.display = DisplayStyle.None;
            credits.style.display = DisplayStyle.None;
        }

        private void Update()
        {
            if (Current == MenuScreen.None || items.Count == 0) return;
            if (HandleMouse()) return;

            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame) Move(-1);
            if (keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame) Move(1);
            if (keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)
            {
                Confirm(selected);
                return;
            }

            if (!keyboard.escapeKey.wasPressedThisFrame) return;
            if (Current == MenuScreen.Controls) Confirm(0);
            else if (Current == MenuScreen.Pause) Confirm(0);
        }

        private bool HandleMouse()
        {
            var mouse = Mouse.current;
            var panel = Root.panel;
            if (mouse == null || panel == null) return false;

            var pointer = mouse.position.ReadValue();
            var panelPoint = RuntimePanelUtils.ScreenToPanel(panel, new Vector2(pointer.x, Screen.height - pointer.y));
            var hovered = -1;
            for (var i = 0; i < items.Count; i++)
            {
                if (items[i].element.worldBound.Contains(panelPoint)) hovered = i;
            }

            if (hovered >= 0 && pointer != lastPointer && hovered != selected)
            {
                Select(hovered);
                SelectionMoved?.Invoke();
            }

            lastPointer = pointer;
            if (hovered < 0 || !mouse.leftButton.wasPressedThisFrame) return false;
            Confirm(hovered);
            return true;
        }

        private void Move(int step)
        {
            Select((selected + step + items.Count) % items.Count);
            SelectionMoved?.Invoke();
        }

        private void Select(int index)
        {
            selected = index;
            for (var i = 0; i < items.Count; i++) items[i].element.EnableInClassList("menu-btn--selected", i == index);
        }

        private void Confirm(int index)
        {
            Confirmed?.Invoke();
            items[index].action();
        }
    }
}
