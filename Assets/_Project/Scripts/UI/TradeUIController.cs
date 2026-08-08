using System.Collections.Generic;
using Project.Core;
using Project.Core.Events;
using Project.Data;
using Project.Gameplay.Dialogue;
using Project.Gameplay.Inventory;
using Project.Gameplay.Trade;
using UnityEngine;
using UnityEngine.UI;

namespace Project.UI
{
    /// <summary>
    /// Panel UI handlu - dwie kolumny: towary NPC (Stock, przycisk Kup) po lewej, ekwipunek gracza
    /// (przycisk Sprzedaj) po prawej. W przeciwieństwie do InventoryUIController/QuestLogUIController
    /// otwiera się automatycznie po TradeRequestedEvent, nie przez własną akcję Input - to opcja
    /// dialogowa decyduje, kiedy handel jest dostępny.
    ///
    /// Właściwy NpcMerchant jest odnajdywany przez NpcDialogueInteractable.ActiveSpeaker (statyczna
    /// referencja ustawiana w Interact(), czyszczona po DialogueEndedEvent) - CELOWA zmiana względem
    /// pierwotnego pomysłu "NpcMerchant sam subskrybuje TradeRequestedEvent i filtruje siebie": to
    /// mieszałoby wiedzę o przepływie dialogu do czysto ekonomicznego komponentu (NpcMerchant), i gdyby
    /// NpcMerchant miał publikować DRUGI event niosący samego siebie w reakcji na TradeRequestedEvent,
    /// powstałby hazard kolejności, gdyby TradeUIController odebrał swój event PRZED NpcMerchant
    /// zdążyłby opublikować swój (Unity nie gwarantuje kolejności OnEnable/subskrypcji między różnymi
    /// GameObjectami). Odczyt przez ActiveSpeaker (ustawiane dużo wcześniej, przy Interact(), nie
    /// w tym samym cyklu eventu co TradeRequestedEvent) eliminuje ten hazard całkowicie.
    ///
    /// Zamyka się WYŁĄCZNIE przyciskiem Close, NIGDY przez DialogueEndedEvent ani Escape: opcja
    /// dialogowa, która otwiera handel, zazwyczaj też kończy rozmowę (NextNodeIndex == -1) w tym samym
    /// wywołaniu SelectOption - gdyby panel zamykał się na DialogueEndedEvent, otwierałby się
    /// i natychmiast zamykał w tej samej klatce. Panel nigdy nie otwiera się przez dedykowaną akcję
    /// Input jak inne panele (Tab/C/J), więc nie ma z czym symetrycznie sparować klawisza do zamknięcia -
    /// zamiast dodawać nowy globalny bind Escape tylko dla tego jednego panelu, prostszy i pewniejszy
    /// jest jawny przycisk.
    /// </summary>
    public class TradeUIController : MonoBehaviour
    {
        private const string TradeLockReason = "Trade";

        [SerializeField] private InventoryBridge inventoryBridge;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Transform stockContainer;
        [SerializeField] private GameObject stockEntryPrefab;
        [SerializeField] private Transform sellContainer;
        [SerializeField] private GameObject sellEntryPrefab;
        [SerializeField] private Button closeButton;

        private NpcMerchant _activeMerchant;

        private void Awake()
        {
            panelRoot.SetActive(false);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<TradeRequestedEvent>(OnTradeRequested);
            EventBus.Subscribe<TradeStockChangedEvent>(OnTradeStockChanged);
            EventBus.Subscribe<InventoryChangedEvent>(OnInventoryChanged);

            closeButton.onClick.AddListener(ClosePanel);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<TradeRequestedEvent>(OnTradeRequested);
            EventBus.Unsubscribe<TradeStockChangedEvent>(OnTradeStockChanged);
            EventBus.Unsubscribe<InventoryChangedEvent>(OnInventoryChanged);

            closeButton.onClick.RemoveListener(ClosePanel);

            if (panelRoot.activeSelf)
            {
                GameplayInputLock.UnlockMovement(TradeLockReason);
                GameplayInputLock.UnlockCamera(TradeLockReason);
            }
        }

        private void OnTradeRequested(TradeRequestedEvent evt)
        {
            GameObject activeSpeaker = NpcDialogueInteractable.ActiveSpeaker;
            NpcMerchant merchant = activeSpeaker != null ? activeSpeaker.GetComponent<NpcMerchant>() : null;

            if (merchant == null)
            {
                Debug.LogError("TradeUIController: opcja dialogowa z OpensTrade wskazuje na NPC bez komponentu NpcMerchant.");
                return;
            }

            _activeMerchant = merchant;
            OpenPanel();
        }

        private void OnTradeStockChanged(TradeStockChangedEvent evt)
        {
            if (panelRoot.activeSelf)
            {
                RefreshStockColumn();
            }
        }

        private void OnInventoryChanged(InventoryChangedEvent evt)
        {
            if (panelRoot.activeSelf)
            {
                RefreshSellColumn();
            }
        }

        private void OpenPanel()
        {
            panelRoot.SetActive(true);
            GameplayInputLock.LockMovement(TradeLockReason);
            GameplayInputLock.LockCamera(TradeLockReason);
            RefreshStockColumn();
            RefreshSellColumn();
        }

        private void ClosePanel()
        {
            panelRoot.SetActive(false);
            _activeMerchant = null;
            GameplayInputLock.UnlockMovement(TradeLockReason);
            GameplayInputLock.UnlockCamera(TradeLockReason);
        }

        private void RefreshStockColumn()
        {
            ClearContainer(stockContainer);

            IReadOnlyList<TradeRuntimeSlot> stock = _activeMerchant.Stock;
            for (int i = 0; i < stock.Count; i++)
            {
                TradeRuntimeSlot slot = stock[i];
                int stockIndex = i;

                GameObject entryInstance = Instantiate(stockEntryPrefab, stockContainer);
                TradeStockEntryUI entryUI = entryInstance.GetComponent<TradeStockEntryUI>();
                entryUI.Setup(slot.Item.DisplayName, slot.Price, slot.Quantity, () => OnBuyClicked(stockIndex));
            }
        }

        private void RefreshSellColumn()
        {
            ClearContainer(sellContainer);

            IReadOnlyList<InventorySlot> slots = inventoryBridge.Inventory.Slots;
            for (int i = 0; i < slots.Count; i++)
            {
                InventorySlot slot = slots[i];
                if (slot.IsEmpty)
                {
                    continue;
                }

                ItemData item = slot.Item;
                bool canSell = item.BasePrice > 0;

                GameObject entryInstance = Instantiate(sellEntryPrefab, sellContainer);
                TradeSellEntryUI entryUI = entryInstance.GetComponent<TradeSellEntryUI>();
                entryUI.Setup(item.DisplayName, slot.Quantity, canSell, () => OnSellClicked(item));
            }
        }

        private void OnBuyClicked(int stockIndex)
        {
            _activeMerchant.TryBuy(stockIndex);
        }

        private void OnSellClicked(ItemData item)
        {
            _activeMerchant.TrySell(item);
        }

        private void ClearContainer(Transform container)
        {
            int childCount = container.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                Destroy(container.GetChild(i).gameObject);
            }
        }
    }
}
