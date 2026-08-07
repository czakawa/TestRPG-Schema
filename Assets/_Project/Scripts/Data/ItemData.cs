using UnityEngine;

namespace Project.Data
{
    public enum ItemType
    {
        Weapon,
        Armor,
        Consumable,
        QuestItem,
        Material,
        Misc
    }

    /// <summary>
    /// Statyczna definicja przedmiotu w grze. Instancje tworzone jako assety przez
    /// menu Gothic/Item, referencjonowane z Inventory/Interaction, nigdy nie modyfikowane
    /// w runtime (stan posiadanej ilości trzyma <see cref="Project.Gameplay.Inventory.InventorySlot"/>).
    /// </summary>
    [CreateAssetMenu(fileName = "NewItem", menuName = "Gothic/Item")]
    public class ItemData : ScriptableObject
    {
        [SerializeField] private string itemId;
        [SerializeField] private string displayName;
        [TextArea]
        [SerializeField] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField] private bool isStackable;
        [SerializeField] private int maxStackSize = 1;
        [SerializeField] private ItemType itemType;

        /// <summary>Unikalny identyfikator przedmiotu (np. "iron_sword") - niezależny od nazwy assetu.</summary>
        public string ItemId => itemId;

        public string DisplayName => displayName;

        public string Description => description;

        public Sprite Icon => icon;

        public bool IsStackable => isStackable;

        /// <summary>Ignorowane, jeśli IsStackable == false.</summary>
        public int MaxStackSize => maxStackSize;

        public ItemType ItemType => itemType;
    }
}
