using Project.Core;
using Project.Core.Events;
using Project.Core.States;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Project.UI
{
    /// <summary>
    /// Panel UI ekranu Game Over. Subskrybuje PlayerDiedEvent bezpośrednio (nie GameStateChangedEvent)
    /// i pokazuje panel od razu przy śmierci - dzięki temu reakcja UI nie zależy od kolejności OnEnable
    /// między tym kontrolerem a PlayerDeathHandler (który dopiero na podstawie tego samego eventu
    /// przełącza GameManager w stan GameOver). Restart zawsze wraca do świeżej sceny Level_01 -
    /// brak zapisu/checkpointów to świadome uproszczenie tego etapu.
    /// </summary>
    public class GameOverUIController : MonoBehaviour
    {
        private const string Level01SceneName = "Level_01";

        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button restartButton;

        private void OnEnable()
        {
            panelRoot.SetActive(false);

            EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
            restartButton.onClick.AddListener(RestartLevel);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
            restartButton.onClick.RemoveListener(RestartLevel);
        }

        private void OnPlayerDied(PlayerDiedEvent evt)
        {
            panelRoot.SetActive(true);
        }

        /// <summary>
        /// Time.timeScale MUSI wrócić do 1 przed załadowaniem sceny - GameOverState ustawił je na 0,
        /// a inaczej nowa scena wystartowałaby z zamrożonym czasem. LoadScene(Single) jest synchroniczne:
        /// niszczy starą scenę (stare instancje Playera/UI) i tworzy nową przez normalny Awake/OnEnable
        /// zanim ta metoda wróci, więc ChangeState(Gameplay) woła się dopiero PO tym, na już odtworzonym
        /// GameSystemsManager. GameManager przetrwa przeładowanie dzięki DontDestroyOnLoad.
        /// </summary>
        private void RestartLevel()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(Level01SceneName, LoadSceneMode.Single);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameStateType.Gameplay);
            }
        }
    }
}
