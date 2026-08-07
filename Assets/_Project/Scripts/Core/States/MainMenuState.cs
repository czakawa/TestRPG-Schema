using Project.Core.Systems;

namespace Project.Core.States
{
    /// <summary>
    /// Stan menu głównego. Upewnia się, że systemy gameplayowe (ruch gracza, walka itd.) nie tickują,
    /// dopóki gracz nie wejdzie do rozgrywki.
    /// </summary>
    public class MainMenuState : IGameState
    {
        private readonly GameSystemsManager _systemsManager;

        public MainMenuState(GameSystemsManager systemsManager)
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
