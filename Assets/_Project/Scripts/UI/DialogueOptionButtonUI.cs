using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Project.UI
{
    /// <summary>
    /// Widok pojedynczego przycisku opcji dialogowej. Czysty "widok" - nie zna DialogueSystem,
    /// tylko wyświetla tekst i przekazuje kliknięcie przez podpięty callback.
    /// </summary>
    public class DialogueOptionButtonUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI optionText;
        [SerializeField] private Button button;

        public void Setup(string text, UnityAction onClick)
        {
            optionText.text = text;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(onClick);
        }
    }
}
