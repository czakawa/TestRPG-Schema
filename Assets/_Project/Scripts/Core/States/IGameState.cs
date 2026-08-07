namespace Project.Core.States
{
    /// <summary>
    /// Kontrakt pojedynczego stanu gry zarządzanego przez <see cref="GameStateMachine"/>.
    /// </summary>
    public interface IGameState
    {
        /// <summary>Wywoływane przy wejściu w stan.</summary>
        void Enter();

        /// <summary>Wywoływane co klatkę, dopóki stan jest aktywny.</summary>
        void Tick(float deltaTime);

        /// <summary>Wywoływane przy opuszczeniu stanu.</summary>
        void Exit();
    }
}
