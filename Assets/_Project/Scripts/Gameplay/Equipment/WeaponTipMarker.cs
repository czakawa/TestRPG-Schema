using UnityEngine;

namespace Project.Gameplay.Equipment
{
    /// <summary>
    /// Pusty komponent-znacznik oznaczający transform końca ostrza broni - WeaponVisualBridge
    /// szuka go przez GetComponentInChildren po zainstancjonowaniu prefabu broni, żeby wiedzieć
    /// skąd faktycznie ma wychodzić origin ataku (CombatBridge). Brak tego komponentu w prefabie
    /// to bezpieczny fallback do pozycji roota broni, nie błąd - pozwala dodawać nowe bronie
    /// stopniowo, bez natychmiastowego wymogu dorobienia tipa każdej z nich.
    /// </summary>
    public class WeaponTipMarker : MonoBehaviour
    {
    }
}
