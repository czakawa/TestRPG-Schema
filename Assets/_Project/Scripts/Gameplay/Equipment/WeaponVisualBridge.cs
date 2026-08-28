using Project.Core;
using Project.Core.Events;
using Project.Data;
using Project.Gameplay.Combat;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Gameplay.Equipment
{
    /// <summary>
    /// MonoBehaviour nasłuchujący EquipmentChangedEvent i podpinający/odpinający wizualny model
    /// broni pod kość RightHand (dobyta) albo pod ręcznie ustawiony backSocket (schowana na
    /// plecach). Przełączanie stanu Dobyta/Schowana przez akcję Draw (R) - natychmiastowe, bez
    /// animacji (Draw/Sheathe animacje to osobne zlecenie na przyszłość). backSocket MUSI być
    /// ręcznie umieszczonym pustym Transformem pod kością Spine w Hierarchy - w przeciwieństwie
    /// do RightHand, Humanoid Avatar nie ma wbudowanego punktu na "pochwę na plecach".
    /// </summary>
    public class WeaponVisualBridge : MonoBehaviour
    {
        private const string DrawSheatheLockReason = "DrawSheathe";

        [SerializeField] private EquipmentBridge equipmentBridge;
        [SerializeField] private Animator playerAnimator;
        [SerializeField] private Transform backSocket;
        [SerializeField] private InputActionReference drawAction;
        [SerializeField] private float drawSheatheLockDuration = 0.8f;

        private ItemData _currentWeaponItem;
        private GameObject _currentWeaponInstance;
        private Transform _currentWeaponTip;
        private MeleeHitbox _currentWeaponHitbox;
        private bool _isDrawn;

        public bool IsDrawn => _isDrawn;

        /// <summary>Transform aktualnie zainstancjonowanego modelu broni (null gdy nic nie jest
        /// założone/broń bez WeaponPrefab) - do użytku przez CombatBridge jako dynamiczny punkt
        /// startowy ataku, żeby cios wychodził z faktycznej pozycji broni w dłoni, nie ze środka postaci.</summary>
        public Transform CurrentWeaponTransform => _currentWeaponInstance != null ? _currentWeaponInstance.transform : null;

        /// <summary>Transform końca ostrza (WeaponTipMarker) jeśli prefab go ma, w przeciwnym razie
        /// pozycja roota broni jako fallback - do użytku przez CombatBridge jako origin ataku.</summary>
        public Transform CurrentWeaponTip => _currentWeaponTip != null ? _currentWeaponTip : CurrentWeaponTransform;

        /// <summary>MeleeHitbox znaleziony w prefabie aktualnie założonej broni (null jeśli broń nie ma
        /// jeszcze skonfigurowanego hitboxa) - do użytku przez CombatBridge/PlayerAnimationEventRelay.</summary>
        public MeleeHitbox CurrentWeaponHitbox => _currentWeaponHitbox;

        private void OnEnable()
        {
            EventBus.Subscribe<EquipmentChangedEvent>(OnEquipmentChanged);

            drawAction.action.Enable();
            drawAction.action.performed += OnDrawPerformed;

            RefreshWeaponVisual();
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<EquipmentChangedEvent>(OnEquipmentChanged);

            drawAction.action.performed -= OnDrawPerformed;
            drawAction.action.Disable();

            CancelInvoke(nameof(UnlockMovementAfterDrawSheathe));
            GameplayInputLock.UnlockMovement(DrawSheatheLockReason);
        }

        private void OnEquipmentChanged(EquipmentChangedEvent evt)
        {
            RefreshWeaponVisual();
        }

        /// <summary>Guard na IsMovementLocked reużyty z tych samych powodów co w CombatBridge -
        /// z otwartym UI/w trakcie dialogu gracz nie powinien móc przełączać broni.</summary>
        private void OnDrawPerformed(InputAction.CallbackContext context)
        {
            if (GameplayInputLock.IsMovementLocked || _currentWeaponInstance == null)
            {
                return;
            }

            SetDrawn(!_isDrawn);
        }

        private void SetDrawn(bool isDrawn)
        {
            if (_isDrawn == isDrawn)
            {
                return;
            }

            _isDrawn = isDrawn;
            EventBus.Publish(new WeaponDrawStateChangedEvent(_isDrawn));

            GameplayInputLock.LockMovement(DrawSheatheLockReason);
            CancelInvoke(nameof(UnlockMovementAfterDrawSheathe));
            Invoke(nameof(UnlockMovementAfterDrawSheathe), drawSheatheLockDuration);
        }

        /// <summary>Wołane przez PlayerAnimationEventRelay w konkretnej klatce animacji Draw/Sheathe -
        /// dopiero teraz miecz fizycznie zmienia socket, zsynchronizowane z ruchem dłoni w animacji.</summary>
        public void OnWeaponSocketSwapAnimationEvent()
        {
            AttachToCurrentSocket();
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
                _currentWeaponTip = null;
                _currentWeaponHitbox = null;
            }

            _currentWeaponItem = equippedWeapon;
            _isDrawn = false;

            if (equippedWeapon == null || equippedWeapon.WeaponPrefab == null)
            {
                return;
            }

            _currentWeaponInstance = Instantiate(equippedWeapon.WeaponPrefab);
            WeaponTipMarker tipMarker = _currentWeaponInstance.GetComponentInChildren<WeaponTipMarker>();
            _currentWeaponTip = tipMarker != null ? tipMarker.transform : null;
            _currentWeaponHitbox = _currentWeaponInstance.GetComponentInChildren<MeleeHitbox>();
            AttachToCurrentSocket();
        }

        private void AttachToCurrentSocket()
        {
            if (_currentWeaponInstance == null)
            {
                return;
            }

            Transform socket = _isDrawn ? GetHandSocket() : backSocket;
            if (socket == null)
            {
                Debug.LogError("WeaponVisualBridge: brak socketu docelowego (handSocket lub backSocket) - sprawdź Animator/przypisanie w Inspectorze.");
                return;
            }

            _currentWeaponInstance.transform.SetParent(socket, false);
            _currentWeaponInstance.transform.localPosition = Vector3.zero;
            _currentWeaponInstance.transform.localRotation = Quaternion.identity;
        }

        private Transform GetHandSocket()
        {
            Transform handSocket = playerAnimator.GetBoneTransform(HumanBodyBones.RightHand);
            if (handSocket == null)
            {
                Debug.LogError("WeaponVisualBridge: nie znaleziono kości RightHand - czy Animator na pewno ma rig Humanoid?");
            }
            return handSocket;
        }

        private void UnlockMovementAfterDrawSheathe()
        {
            GameplayInputLock.UnlockMovement(DrawSheatheLockReason);
        }
    }
}
