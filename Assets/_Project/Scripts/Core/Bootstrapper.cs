using Project.Core.States;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Core
{
    /// <summary>
    /// Uruchamiany jako jedyny obiekt sceny Bootstrap. Tworzy GameManager z prefaba,
    /// jeśli jeszcze nie istnieje (np. przy uruchomieniu edytora bezpośrednio z tej sceny),
    /// przełącza stan gry na MainMenu i ładuje scenę MainMenu.
    /// </summary>
    public class Bootstrapper : MonoBehaviour
    {
        [SerializeField] private GameManager gameManagerPrefab;
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private void Start()
        {
            if (GameManager.Instance == null)
            {
                Instantiate(gameManagerPrefab);
            }

            GameManager.Instance.ChangeState(GameStateType.MainMenu);
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
