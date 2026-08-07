using TMPro;
using UnityEngine;

namespace Project.UI
{
    /// <summary>
    /// Widok pojedynczej linii celu questa w panelu szczegółów (prawa strona quest logu).
    /// Czysto informacyjny wpis - bez interakcji, w przeciwieństwie do QuestListEntryUI.
    /// </summary>
    public class QuestObjectiveEntryUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;

        public void Setup(string text)
        {
            this.text.text = text;
        }
    }
}
