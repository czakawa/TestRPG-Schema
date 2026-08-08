using Project.Core.Events;
using Project.Gameplay.Stats;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.UI
{
    /// <summary>Którą z trzech Vitali PlayerStatsSystem dana instancja PlayerVitalBarUI wyświetla.</summary>
    public enum VitalType
    {
        Health,
        Stamina,
        Mana
    }

    /// <summary>
    /// Stały pasek jednej z Vitali gracza (Health/Stamina/Mana) w HUD, analogiczny do
    /// CurrencyDisplayUI: subskrybuje StatsChangedEvent, a przy włączeniu od razu synchronizuje
    /// pasek/tekst z aktualnym stanem, na wypadek gdyby gracz miał już zmienioną wartość zanim padnie
    /// pierwszy event. Zero pollingu w Update. Jeden komponent obsługuje wszystkie trzy Vitale
    /// (vitalType wybierany w Inspectorze per-instancja) zamiast trzech niemal identycznych skryptów -
    /// StatsChangedEvent jest wspólny dla Health/Stamina/Mana, więc i tak każda instancja musi
    /// odświeżyć się przy każdej zmianie którejkolwiek z nich (RefreshDisplay czyta tylko swój Vital,
    /// więc to tanie).
    /// </summary>
    public class PlayerVitalBarUI : MonoBehaviour
    {
        [SerializeField] private PlayerStatsBridge playerStatsBridge;
        [SerializeField] private VitalType vitalType;
        [SerializeField] private Image fillImage;
        [SerializeField] private TextMeshProUGUI healthText;

        private void OnEnable()
        {
            EventBus.Subscribe<StatsChangedEvent>(OnStatsChanged);
            RefreshDisplay();
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<StatsChangedEvent>(OnStatsChanged);
        }

        private void OnStatsChanged(StatsChangedEvent evt)
        {
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            Vital vital = GetVital();

            fillImage.fillAmount = vital.Current / (float)vital.Max;
            healthText.text = $"{vital.Current}/{vital.Max}";
        }

        private Vital GetVital()
        {
            switch (vitalType)
            {
                case VitalType.Stamina:
                    return playerStatsBridge.Stats.Stamina;
                case VitalType.Mana:
                    return playerStatsBridge.Stats.Mana;
                default:
                    return playerStatsBridge.Stats.Health;
            }
        }
    }
}
