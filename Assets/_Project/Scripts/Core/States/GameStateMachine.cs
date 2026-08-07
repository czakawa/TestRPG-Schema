using System;
using System.Collections.Generic;
using Project.Core.Events;

namespace Project.Core.States
{
    /// <summary>Wszystkie stany, między którymi porusza się <see cref="GameStateMachine"/>.</summary>
    public enum GameStateType
    {
        MainMenu,
        Gameplay,
        Pause,
        GameOver,
        Dialogue
    }

    /// <summary>
    /// Czysta klasa C# (celowo NIE MonoBehaviour) zarządzająca aktualnym stanem gry.
    /// Tranzycje są zabezpieczone przed reentrancy - próba wywołania ChangeState w trakcie
    /// trwającej już tranzycji (np. z Enter/Exit innego stanu) rzuca wyjątek zamiast psuć stan wewnętrzny.
    /// </summary>
    public class GameStateMachine
    {
        private readonly Dictionary<GameStateType, IGameState> _states = new Dictionary<GameStateType, IGameState>();
        private bool _isTransitioning;

        public GameStateType CurrentStateType { get; private set; }
        public IGameState CurrentState { get; private set; }

        /// <summary>Rejestruje instancję stanu pod danym typem. Musi być wywołane przed ChangeState.</summary>
        public void RegisterState(GameStateType type, IGameState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            _states[type] = state;
        }

        /// <summary>Przełącza na nowy stan, wywołując Exit poprzedniego i Enter nowego.</summary>
        public void ChangeState(GameStateType newType)
        {
            if (_isTransitioning)
            {
                throw new InvalidOperationException(
                    $"GameStateMachine: próba zmiany stanu na {newType} w trakcie trwającej już tranzycji.");
            }

            if (!_states.TryGetValue(newType, out IGameState nextState))
            {
                throw new InvalidOperationException($"GameStateMachine: stan {newType} nie został zarejestrowany.");
            }

            if (CurrentState == nextState)
            {
                return;
            }

            _isTransitioning = true;
            try
            {
                GameStateType previousType = CurrentState != null ? CurrentStateType : newType;

                CurrentState?.Exit();

                CurrentState = nextState;
                CurrentStateType = newType;
                CurrentState.Enter();

                EventBus.Publish(new GameStateChangedEvent(previousType, newType));
            }
            finally
            {
                _isTransitioning = false;
            }
        }

        /// <summary>Tickuje aktualnie aktywny stan. Wołane co klatkę przez GameManager.</summary>
        public void Tick(float deltaTime)
        {
            CurrentState?.Tick(deltaTime);
        }
    }
}
