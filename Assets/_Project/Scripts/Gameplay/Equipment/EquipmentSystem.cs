using Project.Core.Events;
using Project.Core.Systems;
using Project.Data;
using Project.Gameplay.Inventory;

namespace Project.Gameplay.Equipment
{
    /// <summary>
    /// Czysta klasa C# (nie MonoBehaviour) zarządzająca trzema slotami ekwipunku zakładanego
    /// (broń/zbroja/biżuteria). Zakładanie i zdejmowanie przechodzi przez InventorySystem tego
    /// samego gracza - założony przedmiot fizycznie opuszcza plecak, dopóki nie zostanie zdjęty.
    /// Brak bonusów statystyk z ekwipunku - to świadomie poza zakresem (przyszły Combat System).
    /// </summary>
    public class EquipmentSystem : IGameSystem
    {
        private readonly InventorySystem _inventorySystem;

        public ItemData EquippedWeapon { get; private set; }
        public ItemData EquippedArmor { get; private set; }
        public ItemData EquippedJewelry { get; private set; }

        public EquipmentSystem(InventorySystem inventorySystem)
        {
            _inventorySystem = inventorySystem;
        }

        public void Initialize()
        {
        }

        public void Tick(float deltaTime)
        {
        }

        public void FixedTick(float fixedDeltaTime)
        {
        }

        public void Shutdown()
        {
        }

        /// <summary>
        /// Próbuje założyć przedmiot z ekwipunku: zdejmuje go z InventorySystem, a jeśli docelowy
        /// slot jest już zajęty, próbuje najpierw zwrócić stary przedmiot z powrotem do ekwipunku.
        /// Jeśli zwrot się nie uda (brak miejsca), cała operacja jest wycofywana - nowy przedmiot
        /// wraca do ekwipunku, stary pozostaje założony - i metoda zwraca false.
        /// </summary>
        public bool TryEquip(ItemData item)
        {
            if (item == null)
            {
                return false;
            }

            if (item.ItemType != ItemType.Weapon && item.ItemType != ItemType.Armor && item.ItemType != ItemType.Jewelry)
            {
                return false;
            }

            if (!_inventorySystem.TryRemoveItem(item.ItemId, 1))
            {
                return false;
            }

            ItemData previousItem = GetEquippedItem(item.ItemType);
            if (previousItem != null && !_inventorySystem.TryAddItem(previousItem, 1))
            {
                // Rollback: nie ma miejsca na stary przedmiot - nie zdejmuj nowego z ekwipunku.
                _inventorySystem.TryAddItem(item, 1);
                return false;
            }

            SetEquippedItem(item.ItemType, item);
            EventBus.Publish(new EquipmentChangedEvent());
            return true;
        }

        /// <summary>
        /// Próbuje zdjąć przedmiot z danego slotu i zwrócić go do ekwipunku. Jeśli ekwipunek jest
        /// pełny, slot pozostaje założony i metoda zwraca false - gracz musi najpierw zrobić miejsce.
        /// </summary>
        public bool TryUnequip(ItemType slotType)
        {
            ItemData equippedItem = GetEquippedItem(slotType);
            if (equippedItem == null)
            {
                return false;
            }

            if (!_inventorySystem.TryAddItem(equippedItem, 1))
            {
                return false;
            }

            SetEquippedItem(slotType, null);
            EventBus.Publish(new EquipmentChangedEvent());
            return true;
        }

        private ItemData GetEquippedItem(ItemType slotType)
        {
            switch (slotType)
            {
                case ItemType.Weapon:
                    return EquippedWeapon;
                case ItemType.Armor:
                    return EquippedArmor;
                case ItemType.Jewelry:
                    return EquippedJewelry;
                default:
                    return null;
            }
        }

        private void SetEquippedItem(ItemType slotType, ItemData item)
        {
            switch (slotType)
            {
                case ItemType.Weapon:
                    EquippedWeapon = item;
                    break;
                case ItemType.Armor:
                    EquippedArmor = item;
                    break;
                case ItemType.Jewelry:
                    EquippedJewelry = item;
                    break;
            }
        }
    }
}
