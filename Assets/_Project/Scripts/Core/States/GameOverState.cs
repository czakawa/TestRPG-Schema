using Project.Core.Systems;
 
namespace Project.Core.States
{
    /// <summary>
    /// Stan końca gry. Zatrzymuje systemy gameplayowe tak samo jak pauza, ale semantycznie
    /// oznacza zakończenie rozgrywki (np. pokazanie ekranu Game Over w UI przez subskrypcję EventBus).
    /// </summary>
    public class GameOverState : IGameState
    {
        private readonly GameSystemsManager _systemsManager;

        public GameOverState(GameSystemsManager systemsManager)
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
