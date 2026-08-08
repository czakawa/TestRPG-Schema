using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Project.Gameplay.Combat
{
    /// <summary>
    /// Lokalny, wizualny komponent per-instancja wroga - pasek HP nad głową, na world-space Canvasie
    /// będącym dzieckiem wroga. Zwykły MonoBehaviour z Update/LateUpdate, nie IGameSystem, dokładnie
    /// tak jak EnemyAI: to stan czysto wizualny jednego konkretnego obiektu, nie coś co
    /// GameSystemsManager musi tickować globalnie. Update() zamiast subskrypcji EventBus jest tu
    /// świadomym wyborem - to prosty, lokalny odczyt stanu tego samego obiektu przez referencję,
    /// nie wymaga globalnej komunikacji między systemami jak EnemyDamagedEvent.
    /// </summary>
    public class EnemyHealthBarUI : MonoBehaviour
    {
        [SerializeField] private EnemyController enemyController;
        [SerializeField] private UnityEngine.UI.Image fillImage;
        [SerializeField] private GameObject visualsRoot;

        private UnityEngine.Camera _mainCamera;

        private void Awake()
        {
            // GetComponentInParent jako fallback, nie jedyna droga: Canvas paska HP jest dzieckiem
            // wroga w hierarchii, więc referencja jest zawsze dostępna bez ręcznego podpinania na
            // każdej instancji/prefabie wroga - to samo podejście co GetComponentInParent<IDamageable>
            // w CombatSystem. Pole zostaje też przeciągalne w Inspectorze, gdyby ktoś wolał to
            // podpiąć jawnie.
            if (enemyController == null)
            {
                enemyController = GetComponentInParent<EnemyController>();
            }

            _mainCamera = UnityEngine.Camera.main;
        }

        private void Update()
        {
            float currentHealth = enemyController.CurrentHealth;
            float maxHealth = enemyController.MaxHealth;

            fillImage.fillAmount = currentHealth / maxHealth;
            visualsRoot.SetActive(currentHealth < maxHealth && currentHealth > 0f);
        }

        private void LateUpdate()
        {
            if (_mainCamera == null)
            {
                return;
            }

            transform.rotation = _mainCamera.transform.rotation;
        }
    }
}
