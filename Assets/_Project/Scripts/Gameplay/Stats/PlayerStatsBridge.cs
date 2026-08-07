using Project.Core;
using UnityEngine;

namespace Project.Gameplay.Stats
{
    /// <summary>
    /// MonoBehaviour "most" łączący czysty PlayerStatsSystem ze światem silnika: rejestruje system
    /// w GameSystemsManager przy włączeniu obiektu i wyrejestrowuje przy wyłączeniu. Wystawia
    /// referencję do PlayerStatsSystem, żeby UI (CharacterPanelUI) i przyszłe systemy
    /// (Combat, rozwój postaci) mogły z niego korzystać.
    /// </summary>
    public class PlayerStatsBridge : MonoBehaviour
    {
        [SerializeField] private int startingHealth = 100;
        [SerializeField] private int startingStamina = 100;
        [SerializeField] private int startingMana = 100;
        [SerializeField] private int startingStrength = 10;
        [SerializeField] private int startingDexterity = 10;
        [SerializeField] private int startingEndurance = 10;
        [SerializeField] private int startingWisdom = 10;

        private PlayerStatsSystem _statsSystem;

        /// <summary>Referencja do systemu statystyk gracza - do użytku przez UI (CharacterPanelUI) i przyszłe systemy.</summary>
        public PlayerStatsSystem Stats => _statsSystem;

        private void Awake()
        {
            _statsSystem = new PlayerStatsSystem(
                startingHealth,
                startingStamina,
                startingMana,
                startingStrength,
                startingDexterity,
                startingEndurance,
                startingWisdom);
        }

        private void OnEnable()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("PlayerStatsBridge: GameManager.Instance jest null - czy scena Bootstrap była uruchomiona jako pierwsza?");
                return;
            }

            GameManager.Instance.Systems.RegisterSystem(_statsSystem);
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.Systems.UnregisterSystem(_statsSystem);
            }
        }
    }
}
