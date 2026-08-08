using Project.Core.Events;
using Project.Core.States;
using UnityEngine;

namespace Project.Core
{
    /// <summary>
    /// MonoBehaviour "most" o pojedynczym zadaniu: subskrybuje PlayerDiedEvent i przełącza grę
    /// w stan GameOver. Umieszczony na Player, obok innych mostów - potrzebuje wyłącznie
    /// GameManager.Instance, żadnej referencji do innego mostu na tym obiekcie, więc nie wymaga
    /// wpisu w Script Execution Order.
    /// </summary>
    public class PlayerDeathHandler : MonoBehaviour
    {
        private void OnEnable()
        {
            EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
        }

        private void OnPlayerDied(PlayerDiedEvent evt)
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("PlayerDeathHandler: GameManager.Instance jest null - czy scena Bootstrap była uruchomiona jako pierwsza?");
                return;
            }

            GameManager.Instance.ChangeState(GameStateType.GameOver);
        }
    }
}
