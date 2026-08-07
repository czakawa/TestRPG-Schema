using System.Collections.Generic;
using UnityEngine;

namespace Project.Data
{
    /// <summary>
    /// Statyczna definicja drzewa rozmowy z NPC. Instancje tworzone jako assety przez
    /// menu Gothic/Dialogue Tree, referencjonowane z NpcDialogueInteractable.
    /// </summary>
    [CreateAssetMenu(fileName = "NewDialogue", menuName = "Gothic/Dialogue Tree")]
    public class DialogueTree : ScriptableObject
    {
        [SerializeField] private string dialogueId;
        [SerializeField] private List<DialogueNode> nodes = new List<DialogueNode>();
        [SerializeField] private int startNodeIndex = 0;

        public string DialogueId => dialogueId;

        public IReadOnlyList<DialogueNode> Nodes => nodes;

        public int StartNodeIndex => startNodeIndex;
    }
}
