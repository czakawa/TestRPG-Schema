using Project.Core.Events;
using UnityEngine;

namespace Project.Gameplay.Interaction
{
    /// <summary>
    /// Podświetla aktualnie namierzony obiekt interaktywny przez override _BaseColor na wszystkich
    /// Renderer-ach pod nim (MaterialPropertyBlock - zero nowych instancji materiałów). Zdejmowanie
    /// podświetlenia z POPRZEDNIEGO celu jest jawnie obsłużone w OnInteractableFocused, bo
    /// InteractionSystem NIE publikuje FocusLost przy przełączeniu bezpośrednio z A na B - tylko
    /// kolejny Focused(B). Zerowanie przez pusty MaterialPropertyBlock (nie ustawianie na biało!),
    /// żeby poprawnie wrócić do oryginalnego koloru materiału niezależnie od tego jaki on był.
    /// </summary>
    public class InteractionHighlightController : MonoBehaviour
    {
        [SerializeField] private Color highlightColor = new Color(1f, 0.85f, 0.4f);

        private GameObject _currentHighlighted;
        private MaterialPropertyBlock _propertyBlock;

        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<InteractableFocusedEvent>(OnInteractableFocused);
            EventBus.Subscribe<InteractableFocusLostEvent>(OnInteractableFocusLost);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<InteractableFocusedEvent>(OnInteractableFocused);
            EventBus.Unsubscribe<InteractableFocusLostEvent>(OnInteractableFocusLost);
        }

        private void OnInteractableFocused(InteractableFocusedEvent evt)
        {
            if (_currentHighlighted != null && _currentHighlighted != evt.Target)
            {
                SetHighlighted(_currentHighlighted, false);
            }

            _currentHighlighted = evt.Target;
            SetHighlighted(_currentHighlighted, true);
        }

        private void OnInteractableFocusLost(InteractableFocusLostEvent evt)
        {
            if (_currentHighlighted != null)
            {
                SetHighlighted(_currentHighlighted, false);
            }

            _currentHighlighted = null;
        }

        private void SetHighlighted(GameObject target, bool highlighted)
        {
            if (target == null)
            {
                return;
            }

            Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                if (highlighted)
                {
                    renderer.GetPropertyBlock(_propertyBlock);
                    _propertyBlock.SetColor("_BaseColor", highlightColor);
                    renderer.SetPropertyBlock(_propertyBlock);
                }
                else
                {
                    renderer.SetPropertyBlock(null);
                }
            }
        }
    }
}
