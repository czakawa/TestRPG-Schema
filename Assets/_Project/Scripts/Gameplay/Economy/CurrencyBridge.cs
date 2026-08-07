using Project.Core;
using UnityEngine;

namespace Project.Gameplay.Economy
{
    /// <summary>
    /// MonoBehaviour "most" łączący czysty CurrencySystem ze światem silnika: rejestruje system
    /// w GameSystemsManager przy włączeniu obiektu i wyrejestrowuje przy wyłączeniu. Wystawia
    /// referencję do CurrencySystem, żeby inne mosty (np. GoldPickup) i UI mogły z niego korzystać.
    /// </summary>
    public class CurrencyBridge : MonoBehaviour
    {
        [SerializeField] private int startingGold = 0;

        private CurrencySystem _currencySystem;

        /// <summary>Referencja do systemu waluty - do użytku przez inne mosty (np. GoldPickup) i UI.</summary>
        public CurrencySystem Currency => _currencySystem;

        private void Awake()
        {
            _currencySystem = new CurrencySystem(startingGold);
        }

        private void OnEnable()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("CurrencyBridge: GameManager.Instance jest null - czy scena Bootstrap była uruchomiona jako pierwsza?");
                return;
            }

            GameManager.Instance.Systems.RegisterSystem(_currencySystem);
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.Systems.UnregisterSystem(_currencySystem);
            }
        }
    }
}
