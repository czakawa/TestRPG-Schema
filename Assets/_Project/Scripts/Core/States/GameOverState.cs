using Project.Core.Systems;
using UnityEngine;

namespace Project.Core.States
{
    /// <summary>
    /// Stan końca gry. Zatrzymuje systemy gameplayowe tak samo jak pauza, ale semantycznie
    /// oznacza zakończenie rozgrywki (np. pokazanie ekranu Game Over w UI przez subskrypcję EventBus).
    /// Dodatkowo, w przeciwieństwie do Pause, zatrzymuje globalny czas gry (Time.timeScale = 0) -
    /// ekran Game Over jest ostateczny (nie da się z niego "odpauzować"), więc nic w scenie
    /// (fizyka, animacje, przeciwnicy) nie powinno się dalej poruszać, dopóki gracz nie zrestartuje
    /// poziomu. GameOverUIController.RestartLevel() jest odpowiedzialny za jawne przywrócenie
    /// Time.timeScale = 1 przed przeładowaniem sceny - ten stan celowo nie robi tego w Exit(),
    /// bo jedyną drogą wyjścia z GameOver jest pełny restart sceny, nie zwykła tranzycja stanu.
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
            Time.timeScale = 0f;
        }

        public void Tick(float deltaTime)
        {
        }

        public void Exit()
        {
        }
    }
}
