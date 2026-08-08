using Project.Core.Events;
using Project.Core.Systems;
using Project.Data;

namespace Project.Gameplay.Dialogue
{
    /// <summary>
    /// Czysta klasa C# (nie MonoBehaviour) zarządzająca przebiegiem rozmowy z NPC. Nie jest
    /// tickowana przez pętlę systemów (Tick jest pusty) - cały przepływ jest sterowany bezpośrednio
    /// wywołaniami z DialogueBridge/UI (StartDialogue/SelectOption/EndDialogue), więc zablokowanie
    /// GameSystemsManager.SetSystemsActive(false) w trakcie stanu Dialogue nie przerywa rozmowy.
    /// </summary>
    public class DialogueSystem : IGameSystem
    {
        private DialogueTree _activeTree;
        private int _currentNodeIndex;

        public bool IsActive { get; private set; }

        public void Initialize()
        {
            IsActive = false;
            _activeTree = null;
            _currentNodeIndex = -1;
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

        /// <summary>Rozpoczyna rozmowę od StartNodeIndex danego drzewa i publikuje DialogueStartedEvent.</summary>
        public void StartDialogue(DialogueTree tree)
        {
            if (tree == null || tree.Nodes.Count == 0)
            {
                return;
            }

            _activeTree = tree;
            _currentNodeIndex = tree.StartNodeIndex;
            IsActive = true;

            EventBus.Publish(new DialogueStartedEvent(_activeTree.Nodes[_currentNodeIndex]));
        }

        /// <summary>
        /// Wybiera opcję o danym indeksie w aktualnym węźle. Jeśli opcja niesie QuestAction != None,
        /// publikuje DialogueQuestActionEvent PRZED przejściem dalej - DialogueSystem celowo nie ma
        /// twardej referencji do QuestSystem (systemy komunikują się przez EventBus), więc samo
        /// rozpoczęcie/oddanie questa wykonuje QuestBridge w reakcji na ten event. Analogicznie,
        /// jeśli opcja ma OpensTrade == true, publikuje TradeRequestedEvent - DialogueSystem nie wie
        /// nic o NpcMerchant/handlu, quest i trade mogą teoretycznie współistnieć na tej samej opcji
        /// (nie są wzajemnie wykluczające). Następnie: NextNodeIndex == -1 kończy rozmowę (patrz
        /// EndDialogue), inaczej przechodzi do wskazanego węzła i publikuje DialogueNodeChangedEvent.
        /// </summary>
        public void SelectOption(int optionIndex)
        {
            if (!IsActive)
            {
                return;
            }

            DialogueNode currentNode = _activeTree.Nodes[_currentNodeIndex];
            if (optionIndex < 0 || optionIndex >= currentNode.Options.Count)
            {
                return;
            }

            DialogueOption option = currentNode.Options[optionIndex];

            if (option.QuestAction != QuestActionType.None)
            {
                EventBus.Publish(new DialogueQuestActionEvent(option.QuestAction, option.TargetQuest));
            }

            if (option.OpensTrade)
            {
                EventBus.Publish(new TradeRequestedEvent());
            }

            if (option.NextNodeIndex == -1)
            {
                EndDialogue();
                return;
            }

            _currentNodeIndex = option.NextNodeIndex;
            EventBus.Publish(new DialogueNodeChangedEvent(_activeTree.Nodes[_currentNodeIndex]));
        }

        /// <summary>Kończy aktualną rozmowę, zeruje stan i publikuje DialogueEndedEvent.</summary>
        public void EndDialogue()
        {
            IsActive = false;
            _activeTree = null;
            _currentNodeIndex = -1;

            EventBus.Publish(new DialogueEndedEvent());
        }
    }
}
