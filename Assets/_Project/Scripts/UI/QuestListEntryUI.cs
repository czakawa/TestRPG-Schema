using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Project.UI
{
    /// <summary>
    /// Widok pojedynczej pozycji na liście questów (lewa kolumna quest logu). Czysty "widok" -
    /// nie zna QuestData/QuestProgress, tylko wyświetla przekazany tytuł i przekazuje kliknięcie
    /// przez podpięty callback, tak samo jak DialogueOptionButtonUI.
    /// </summary>
    public class QuestListEntryUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Button button;

        public void Setup(string title, UnityAction onClick)
        {
            titleText.text = title;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(onClick);
        }
    }
}
