using System;
using UnityEngine;

namespace Project.Data
{
    /// <summary>
    /// Pojedyncza opcja odpowiedzi gracza w węźle dialogowym. Zwykła serializowalna klasa
    /// (nie ScriptableObject) - zagnieżdżona w liście wewnątrz DialogueNode.
    /// </summary>
    [Serializable]
    public class DialogueOption
    {
        [SerializeField] private string optionText;
        [SerializeField] private int nextNodeIndex = -1;

        public string OptionText => optionText;

        /// <summary>Indeks w DialogueTree.Nodes, do którego przechodzi rozmowa. -1 = koniec rozmowy.</summary>
        public int NextNodeIndex => nextNodeIndex;
    }
}
