using System;
using UnityEngine;

namespace Project.Data
{
    /// <summary>
    /// Typ celu questa. CollectItem sprawdza stan ekwipunku (ItemData.itemId), TalkToNpc
    /// sprawdza czy gracz odbył rozmowę z NPC o danym dialogueId.
    /// </summary>
    public enum ObjectiveType
    {
        CollectItem,
        TalkToNpc
    }

    /// <summary>
    /// Pojedynczy cel questa. Zwykła serializowalna klasa (nie ScriptableObject) - zagnieżdżona
    /// w liście wewnątrz QuestData.
    /// </summary>
    [Serializable]
    public class QuestObjective
    {
        [SerializeField] private ObjectiveType type;
        [SerializeField] private string targetId;
        [SerializeField] private int requiredAmount = 1;
        [SerializeField] private string description;

        /// <summary>Dla CollectItem: ItemData.itemId. Dla TalkToNpc: dialogueId drzewa rozmowy NPC.</summary>
        public ObjectiveType Type => type;

        public string TargetId => targetId;

        /// <summary>Dla CollectItem: wymagana ilość. Dla TalkToNpc: ignorowane, zawsze traktowane jako 1.</summary>
        public int RequiredAmount => requiredAmount;

        /// <summary>Tekst wyświetlany w quest logu, np. "Zbierz 5 żelaznej rudy".</summary>
        public string Description => description;
    }
}
