using Project.Core.Systems;

namespace Project.Core.States
{
    /// <summary>
    /// Stan aktywnej rozmowy z NPC. Blokuje ruch/interakcję przez SetSystemsActive(false)
    /// tak jak Pause, ale NIE zatrzymuje czasu gry (Time.timeScale zostaje 1) - dialog to nie pauza.
    /// DialogueSystem samo w sobie nie potrzebuje tickowania (jego Tick jest pusty) - jest sterowane
    /// bezpośrednio wywołaniami StartDialogue/SelectOption/EndDialogue z DialogueBridge/UI, więc
    /// wyłączenie pętli GameSystemsManager nie przerywa przebiegu rozmowy.
    /// </summary>
    public class DialogueState : IGameState
    {
        private readonly GameSystemsManager _systemsManager;

        public DialogueState(GameSystemsManager systemsManager)
        {
            _systemsManager = systemsManager;
        }

        public void Enter()
        {
            _systemsManager.SetSystemsActive(false);
        }

        public void Tick(float deltaTime)
        {
        }

        public void Exit()
        {
        }
    }
}
