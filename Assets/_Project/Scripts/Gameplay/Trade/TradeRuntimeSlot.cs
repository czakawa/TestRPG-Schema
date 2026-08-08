using Project.Data;

namespace Project.Gameplay.Trade
{
    /// <summary>
    /// Pojedynczy slot bieżącego zapasu NPC - czysta struktura danych, bez logiki, mutowalna
    /// w runtime (Quantity spada przy zakupie). Zwykła klasa, NIE ScriptableObject: to świeża
    /// kopia stworzona przez NpcMerchant.Awake z TraderData.Stock, żeby nigdy nie mutować samego
    /// assetu (analogicznie do InventorySlot względem ItemData).
    /// </summary>
    public class TradeRuntimeSlot
    {
        public ItemData Item;
        public int Price;
        public int Quantity;
    }
}
