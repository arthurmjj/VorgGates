using System;

namespace VorgGates.Core
{
    public enum GameFlowState
    {
        Loading,
        Playing,
        Dying,
        Victory,
        Transition,
        Menu,
        Paused
    }

    /// <summary>Single owner of the global game state. Invalid transitions are rejected.</summary>
    public sealed class GameFlow
    {
        public GameFlowState State { get; private set; } = GameFlowState.Loading;

        public event Action<GameFlowState> Changed;

        public bool TryEnter(GameFlowState next)
        {
            if (!IsAllowed(State, next)) return false;
            State = next;
            Changed?.Invoke(next);
            return true;
        }

        public static bool IsAllowed(GameFlowState from, GameFlowState to)
        {
            switch (from)
            {
                case GameFlowState.Loading: return to == GameFlowState.Playing || to == GameFlowState.Menu;
                case GameFlowState.Menu: return to == GameFlowState.Loading;
                case GameFlowState.Playing: return to == GameFlowState.Dying || to == GameFlowState.Victory || to == GameFlowState.Transition || to == GameFlowState.Paused;
                case GameFlowState.Paused: return to == GameFlowState.Playing || to == GameFlowState.Menu;
                case GameFlowState.Dying: return to == GameFlowState.Loading;
                case GameFlowState.Victory: return to == GameFlowState.Loading || to == GameFlowState.Menu;
                case GameFlowState.Transition: return to == GameFlowState.Playing || to == GameFlowState.Loading;
                default: return false;
            }
        }
    }
}
