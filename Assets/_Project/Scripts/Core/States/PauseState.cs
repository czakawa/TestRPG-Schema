using Project.Core.Systems;

namespace Project.Core.States
{
    /// <summary>
    /// Stan pauzy. Zatrzymuje tickowanie systemów gameplayowych przez SetSystemsActive(false)
    /// bez niszczenia jakichkolwiek obiektów - powrót do GameplayState wznawia tickowanie.
    /// </summary>
    public class PauseState : IGameState
    {
        private readonly GameSystemsManager _systemsManager;

        public PauseState(GameSystemsManager systemsManager)
        {
            _systemsManager = systemsManager;
        }

        public void Enter()
        {
            _systemsManager.SetSystemsActive(false);
        }

        public void Tick(float deltaTime)
        {
        }

        public void Exit()
        {
        }
    }
}
