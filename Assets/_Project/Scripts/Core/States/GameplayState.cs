using Project.Core.Systems;

namespace Project.Core.States
{
    /// <summary>
    /// Stan właściwej rozgrywki. Włącza tickowanie wszystkich zarejestrowanych IGameSystem
    /// (ruch gracza, w przyszłości walka, AI itd.).
    /// </summary>
    public class GameplayState : IGameState
    {
        private readonly GameSystemsManager _systemsManager;

        public GameplayState(GameSystemsManager systemsManager)
        {
            _systemsManager = systemsManager;
        }

        public void Enter()
        {
            _systemsManager.SetSystemsActive(true);
        }

        public void Tick(float deltaTime)
        {
        }

        public void Exit()
        {
        }
    }
}
