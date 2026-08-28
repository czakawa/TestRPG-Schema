using System.Collections.Generic;
using Project.Core;
using Project.Core.Events;
using Project.Data;
using Project.Gameplay.Economy;
using Project.Gameplay.Inventory;
using Project.Gameplay.Stats;
using UnityEngine;

namespace Project.Gameplay.Quests
{
    /// <summary>
    /// MonoBehaviour "most" łączący czysty QuestSystem ze światem silnika. Wymaga referencji do
    /// InventoryBridge i CurrencyBridge w Inspectorze (do konstrukcji QuestSystem). System jest
    /// budowany w OnEnable, nie w Awake - z tego samego powodu co EquipmentBridge: potrzebuje
    /// InventorySystem/CurrencySystem, które inne mosty tworzą we własnym Awake, a Unity nie
    /// gwarantuje kolejności Awake między różnymi komponentami na tym samym GameObjekcie (Player).
    /// OnEnable jest bezpieczne, bo Awake wszystkich obiektów sceny kończy się przed OnEnable
    /// któregokolwiek z nich.
    ///
    /// Subskrybuje InventoryChangedEvent i przy KAŻDEJ zmianie przelicza WSZYSTKIE sloty ekwipunku
    /// przez ReportItemCollected - to gwarantuje, że QuestSystem zawsze ma aktualny stan celów
    /// CollectItem bez ręcznego wołania z każdego miejsca, gdzie przedmiot mógłby trafić do
    /// ekwipunku (pickup, zwrot z EquipmentSystem przy TryUnequip, nagroda z TryTurnInQuest itd.).
    ///
    /// Integracja z rozmowami idzie w dwie strony, żadna przez twardą referencję w konstruktorze:
    /// - TalkToNpc: NpcDialogueInteractable.Interact() woła QuestBridge.Quest.ReportNpcTalkedTo
    ///   bezpośrednio (DialogueEndedEvent nie niesie informacji, jaki dialog się zakończył, a
    ///   zmiana jego sygnatury wykraczałaby poza zakres tego zlecenia).
    /// - StartQuest/TurnInQuest: DialogueSystem.SelectOption publikuje DialogueQuestActionEvent,
    ///   które QuestBridge subskrybuje tutaj i tłumaczy na wywołania QuestSystem - to zachowuje
    ///   zasadę, że systemy komunikują się przez EventBus, nie przez bezpośrednie referencje.
    /// </summary>
    public class QuestBridge : MonoBehaviour
    {
        [SerializeField] private InventoryBridge inventoryBridge;
        [SerializeField] private CurrencyBridge currencyBridge;
        [SerializeField] private PlayerStatsBridge playerStatsBridge;

        private QuestSystem _questSystem;

        /// <summary>Referencja do systemu questów - do użytku przez NPC (NpcDialogueInteractable) i przyszłe UI quest logu.</summary>
        public QuestSystem Quest => _questSystem;

        private void OnEnable()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("QuestBridge: GameManager.Instance jest null - czy scena Bootstrap była uruchomiona jako pierwsza?");
                return;
            }

            if (inventoryBridge == null || currencyBridge == null)
            {
                Debug.LogError("QuestBridge: brak referencji do InventoryBridge/CurrencyBridge w Inspectorze.");
                return;
            }

            _questSystem = new QuestSystem(inventoryBridge.Inventory, currencyBridge.Currency, playerStatsBridge.Stats);
            GameManager.Instance.Systems.RegisterSystem(_questSystem);

            EventBus.Subscribe<InventoryChangedEvent>(OnInventoryChanged);
            EventBus.Subscribe<DialogueQuestActionEvent>(OnDialogueQuestAction);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<InventoryChangedEvent>(OnInventoryChanged);
            EventBus.Unsubscribe<DialogueQuestActionEvent>(OnDialogueQuestAction);

            if (GameManager.Instance != null && _questSystem != null)
            {
                GameManager.Instance.Systems.UnregisterSystem(_questSystem);
            }
        }

        private void OnInventoryChanged(InventoryChangedEvent evt)
        {
            IReadOnlyList<InventorySlot> slots = inventoryBridge.Inventory.Slots;
            for (int i = 0; i < slots.Count; i++)
            {
                InventorySlot slot = slots[i];
                if (!slot.IsEmpty)
                {
                    _questSystem.ReportItemCollected(slot.Item.ItemId, slot.Quantity);
                }
            }
        }

        private void OnDialogueQuestAction(DialogueQuestActionEvent evt)
        {
            if (evt.Quest == null)
            {
                return;
            }

            switch (evt.Action)
            {
                case QuestActionType.StartQuest:
                    _questSystem.StartQuest(evt.Quest);
                    break;
                case QuestActionType.TurnInQuest:
                    _questSystem.TryTurnInQuest(evt.Quest.QuestId);
                    break;
            }
        }
    }
}
