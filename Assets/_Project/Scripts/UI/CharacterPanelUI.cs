using Project.Core;
using Project.Core.Events;
using Project.Core.States;
using Project.Gameplay.Stats;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.UI
{
    /// <summary>
    /// Panel postaci - podgląd statystyk (Vitale + atrybuty + XP/Poziom/Punkty Nauki). Przełączany
    /// akcją ToggleCharacterPanel (klawisz C), odświeżany po StatsChangedEvent oraz przy otwarciu,
    /// dokładnie jak InventoryUIController. Sloty założonego ekwipunku (broń/zbroja/biżuteria) żyją
    /// w InventoryUIController, nie tutaj.
    ///
    /// Blokuje ruch/kamerę własnym kluczem "CharacterPanel" w GameplayInputLock, niezależnym od
    /// klucza "Inventory" - panel postaci i panel ekwipunku (Tab) mogą być otwarte jednocześnie
    /// (sensowny use case: gracz porównuje założony sprzęt z zawartością plecaka), każdy panel
    /// niezależnie zakłada i zdejmuje własną blokadę. Ten sam guard na stan Dialogue i ten sam
    /// wzorzec auto-zamknięcia po DialogueStartedEvent co InventoryUIController.
    /// </summary>
    public class CharacterPanelUI : MonoBehaviour
    {
        private const string CharacterPanelLockReason = "CharacterPanel";

        [SerializeField] private PlayerStatsBridge playerStatsBridge;

        [SerializeField] private GameObject characterPanelRoot;
        [SerializeField] private InputActionReference toggleCharacterPanelAction;

        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI staminaText;
        [SerializeField] private TextMeshProUGUI manaText;

        [SerializeField] private TextMeshProUGUI strengthText;
        [SerializeField] private TextMeshProUGUI dexterityText;
        [SerializeField] private TextMeshProUGUI enduranceText;
        [SerializeField] private TextMeshProUGUI wisdomText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI experienceText;
        [SerializeField] private TextMeshProUGUI learningPointsText;

        private void Awake()
        {
            characterPanelRoot.SetActive(false);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<StatsChangedEvent>(OnStatsChanged);
            EventBus.Subscribe<DialogueStartedEvent>(OnDialogueStarted);

            toggleCharacterPanelAction.action.Enable();
            toggleCharacterPanelAction.action.performed += OnTogglePerformed;
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<StatsChangedEvent>(OnStatsChanged);
            EventBus.Unsubscribe<DialogueStartedEvent>(OnDialogueStarted);

            toggleCharacterPanelAction.action.performed -= OnTogglePerformed;
            toggleCharacterPanelAction.action.Disable();

            if (characterPanelRoot.activeSelf)
            {
                GameplayInputLock.UnlockMovement(CharacterPanelLockReason);
                GameplayInputLock.UnlockCamera(CharacterPanelLockReason);
            }
        }

        private void OnStatsChanged(StatsChangedEvent evt)
        {
            if (characterPanelRoot.activeSelf)
            {
                RefreshStats();
            }
        }

        private void OnTogglePerformed(InputAction.CallbackContext context)
        {
            if (characterPanelRoot.activeSelf)
            {
                ClosePanel();
                return;
            }

            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameStateType.Dialogue)
            {
                Debug.Log("Panel postaci jest niedostępny podczas rozmowy.");
                return;
            }

            OpenPanel();
        }

        private void OnDialogueStarted(DialogueStartedEvent evt)
        {
            if (characterPanelRoot.activeSelf)
            {
                ClosePanel();
            }
        }

        private void OpenPanel()
        {
            characterPanelRoot.SetActive(true);
            GameplayInputLock.LockMovement(CharacterPanelLockReason);
            GameplayInputLock.LockCamera(CharacterPanelLockReason);
            RefreshStats();
        }

        private void ClosePanel()
        {
            characterPanelRoot.SetActive(false);
            GameplayInputLock.UnlockMovement(CharacterPanelLockReason);
            GameplayInputLock.UnlockCamera(CharacterPanelLockReason);
        }

        private void RefreshStats()
        {
            PlayerStatsSystem stats = playerStatsBridge.Stats;

            healthText.text = $"Zdrowie: {stats.Health.Current}/{stats.Health.Max}";
            staminaText.text = $"Kondycja: {stats.Stamina.Current}/{stats.Stamina.Max}";
            manaText.text = $"Mana: {stats.Mana.Current}/{stats.Mana.Max}";

            strengthText.text = $"Siła: {stats.Strength}";
            dexterityText.text = $"Zręczność: {stats.Dexterity}";
            enduranceText.text = $"Wytrzymałość: {stats.Endurance}";
            wisdomText.text = $"Mądrość: {stats.Wisdom}";
            levelText.text = $"Poziom: {stats.Level}";
            experienceText.text = $"Doświadczenie: {stats.Experience}/{stats.XpRequiredForNextLevel}";
            learningPointsText.text = $"Punkty Nauki: {stats.LearningPoints}";
        }
    }
}
