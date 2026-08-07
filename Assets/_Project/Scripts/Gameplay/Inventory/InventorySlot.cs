using Project.Data;

namespace Project.Gameplay.Inventory
{
    /// <summary>
    /// Pojedynczy slot ekwipunku - czysta struktura danych, bez logiki. Instancje żyją
    /// wewnątrz prealokowanej listy w <see cref="InventorySystem"/> i są mutowane in-place
    /// (Item/Quantity), nigdy tworzone na nowo przy dodawaniu/usuwaniu przedmiotów.
    /// </summary>
    public class InventorySlot
    {
        public ItemData Item;
        public int Quantity;

        public bool IsEmpty => Item == null || Quantity <= 0;

        public void Clear()
        {
            Item = null;
            Quantity = 0;
        }
    }
}
