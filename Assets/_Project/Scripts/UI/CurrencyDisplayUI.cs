using Project.Core.Events;
using Project.Gameplay.Economy;
using TMPro;
using UnityEngine;

namespace Project.UI
{
    /// <summary>
    /// Prosty, zawsze widoczny licznik złota w HUD (poza panelem ekwipunku). Subskrybuje
    /// CurrencyChangedEvent, a przy włączeniu od razu synchronizuje tekst z aktualnym stanem,
    /// na wypadek gdyby gracz miał już złoto zanim padnie pierwszy event.
    /// </summary>
    public class CurrencyDisplayUI : MonoBehaviour
    {
        [SerializeField] private CurrencyBridge currencyBridge;
        [SerializeField] private TextMeshProUGUI goldText;

        private void OnEnable()
        {
            EventBus.Subscribe<CurrencyChangedEvent>(OnCurrencyChanged);
            goldText.text = "Złoto: " + currencyBridge.Currency.Gold;
            //Debug.Log($"CurrencyDisplayUI OnEnable, instancja systemu: {currencyBridge.Currency.GetHashCode()}");
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<CurrencyChangedEvent>(OnCurrencyChanged);
        }

        private void OnCurrencyChanged(CurrencyChangedEvent evt)
        {
            goldText.text = "Złoto: " + evt.NewAmount;
            // Debug.Log($"OnCurrencyChanged odebrany, NewAmount: {evt.NewAmount}");
        }
    }
}
