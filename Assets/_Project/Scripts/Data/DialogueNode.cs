using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Data
{
    /// <summary>
    /// Pojedynczy węzeł rozmowy: kwestia NPC + dostępne opcje odpowiedzi gracza. Zwykła
    /// serializowalna klasa (nie ScriptableObject) - zagnieżdżona w liście wewnątrz DialogueTree.
    /// </summary>
    [Serializable]
    public class DialogueNode
    {
        [TextArea]
        [SerializeField] private string npcText;
        [SerializeField] private string speakerName;
        [SerializeField] private List<DialogueOption> options = new List<DialogueOption>();

        public string NpcText => npcText;

        public string SpeakerName => speakerName;

        public IReadOnlyList<DialogueOption> Options => options;
    }
}
