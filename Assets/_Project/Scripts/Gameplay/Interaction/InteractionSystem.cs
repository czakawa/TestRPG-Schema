using System;
using Project.Core.Events;
using Project.Core.Systems;
using UnityEngine;

namespace Project.Gameplay.Interaction
{
    /// <summary>
    /// Czysta klasa C# (nie MonoBehaviour) wykrywająca, na jaki IInteractable patrzy gracz, i pozwalająca
    /// z nim wejść w interakcję. Pozycja i kierunek patrzenia są dostarczane z zewnątrz przez delegaty
    /// (patrz <see cref="InteractionBridge"/>), więc system nie zależy od UnityEngine.Transform ani żadnego
    /// konkretnego komponentu sceny - jedyne API silnika, którego używa, to statyczny Physics.RaycastNonAlloc.
    /// Zmiana namierzonego celu jest zgłaszana przez EventBus (InteractableFocusedEvent/InteractableFocusLostEvent),
    /// nie co klatkę - dzięki temu UI nie jest zalewane tym samym zdarzeniem.
    /// </summary>
    public class InteractionSystem : IGameSystem
    {
        private readonly Func<Vector3> _getOrigin;
        private readonly Func<Vector3> _getDirection;
        private readonly float _range;
        private readonly LayerMask _interactableLayer;
        private readonly GameObject _interactor;
        private readonly RaycastHit[] _hitBuffer = new RaycastHit[1];

        private IInteractable _focusedInteractable;

        public InteractionSystem(
            Func<Vector3> getOrigin,
            Func<Vector3> getDirection,
            float range,
            LayerMask interactableLayer,
            GameObject interactor)
        {
            _getOrigin = getOrigin;
            _getDirection = getDirection;
            _range = range;
            _interactableLayer = interactableLayer;
            _interactor = interactor;
        }

        public void Initialize()
        {
            _focusedInteractable = null;
        }

        public void Tick(float deltaTime)
        {
            Vector3 origin = _getOrigin();
            Vector3 direction = _getDirection();

            int hitCount = Physics.RaycastNonAlloc(origin, direction, _hitBuffer, _range, _interactableLayer, QueryTriggerInteraction.Ignore);

            IInteractable hitInteractable = hitCount > 0
                ? _hitBuffer[0].collider.GetComponentInParent<IInteractable>()
                : null;
            GameObject hitGameObject = hitInteractable != null ? ((Component)hitInteractable).gameObject : null;

            if (ReferenceEquals(hitInteractable, _focusedInteractable))
            {
                return;
            }

            _focusedInteractable = hitInteractable;

            if (_focusedInteractable != null)
            {
                EventBus.Publish(new InteractableFocusedEvent(_focusedInteractable.GetInteractionPrompt(), hitGameObject));
            }
            else
            {
                EventBus.Publish(new InteractableFocusLostEvent());
            }
        }

        public void FixedTick(float fixedDeltaTime)
        {
        }

        public void Shutdown()
        {
            _focusedInteractable = null;
        }

        /// <summary>Wołane przez bridge po naciśnięciu akcji Interact - próbuje wejść w interakcję z aktualnym celem.</summary>
        public void TryInteract()
        {
            if (_focusedInteractable != null && _focusedInteractable.CanInteract(_interactor))
            {
                _focusedInteractable.Interact(_interactor);
            }
        }
    }
}
