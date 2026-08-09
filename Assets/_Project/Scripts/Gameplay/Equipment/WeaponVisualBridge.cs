using Project.Core.Events;
using Project.Data;
using UnityEngine;

namespace Project.Gameplay.Equipment
{
    /// <summary>
    /// MonoBehaviour nasłuchujący EquipmentChangedEvent i podpinający/odpinający wizualny model
    /// broni pod kość RightHand Animatora gracza. Czyta EquipmentSystem.EquippedWeapon reaktywnie
    /// (event niesie tylko sygnał "coś się zmieniło", nie sam przedmiot) - dokładnie tak jak
    /// TradeUIController odczytuje dane z NpcMerchant po TradeStockChangedEvent zamiast nosić je
    /// w evencie. Śledzi ostatnio zinstancjonowany ItemData, żeby nie tworzyć nowej instancji
    /// modelu przy każdym evencie, jeśli broń się nie zmieniła (np. equip zbroi obok już
    /// założonej broni też publikuje EquipmentChangedEvent).
    /// </summary>
    public class WeaponVisualBridge : MonoBehaviour
    {
        [SerializeField] private EquipmentBridge equipmentBridge;
        [SerializeField] private Animator playerAnimator;

        private ItemData _currentWeaponItem;
        private GameObject _currentWeaponInstance;

        private void OnEnable()
        {
            EventBus.Subscribe<EquipmentChangedEvent>(OnEquipmentChanged);
            RefreshWeaponVisual();
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<EquipmentChangedEvent>(OnEquipmentChanged);
        }

        private void OnEquipmentChanged(EquipmentChangedEvent evt)
        {
            RefreshWeaponVisual();
        }

        private void RefreshWeaponVisual()
        {
            if (equipmentBridge == null || equipmentBridge.Equipment == null)
            {
                return;
            }

            ItemData equippedWeapon = equipmentBridge.Equipment.EquippedWeapon;

            if (ReferenceEquals(equippedWeapon, _currentWeaponItem))
            {
                return;
            }

            if (_currentWeaponInstance != null)
            {
                Destroy(_currentWeaponInstance);
                _currentWeaponInstance = null;
            }

            _currentWeaponItem = equippedWeapon;

            if (equippedWeapon == null || equippedWeapon.WeaponPrefab == null)
            {
                return;
            }

            Transform handSocket = playerAnimator.GetBoneTransform(HumanBodyBones.RightHand);
            if (handSocket == null)
            {
                Debug.LogError("WeaponVisualBridge: nie znaleziono kości RightHand - czy Animator na pewno ma rig Humanoid?");
                return;
            }

            _currentWeaponInstance = Instantiate(equippedWeapon.WeaponPrefab, handSocket);
            _currentWeaponInstance.transform.localPosition = Vector3.zero;
            _currentWeaponInstance.transform.localRotation = Quaternion.identity;
        }
    }
}
