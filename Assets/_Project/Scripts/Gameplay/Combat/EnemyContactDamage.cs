using Project.Gameplay.Stats;
using UnityEngine;

namespace Project.Gameplay.Combat
{
    /// <summary>
    /// Osobny komponent na wrogu odpowiedzialny wyłącznie za obrażenia zadawane graczowi przy
    /// kontakcie - celowo rozdzielony od EnemyController (pojedyncza odpowiedzialność każdej klasy).
    /// Używa OnTriggerStay, nie OnCollisionStay: CharacterController gracza nie ma własnego Rigidbody,
    /// a Unity wysyła OnCollisionEnter/Stay tylko dla par colliderów, z których przynajmniej jeden ma
    /// Rigidbody - taka para tu nie istnieje, więc OnCollisionStay nigdy by się nie wywołał.
    /// CharacterController natomiast jest udokumentowanym wyjątkiem dla triggerów: poprawnie generuje
    /// OnTriggerEnter/Stay na colliderach oznaczonych jako IsTrigger, w które wchodzi, bez potrzeby
    /// Rigidbody po żadnej ze stron. Dlatego wróg potrzebuje DODATKOWEGO collidera z IsTrigger = true
    /// obok solidnego collidera używanego przez CombatSystem.PerformAttack i fizyczne blokowanie
    /// gracza (patrz sekcja konfiguracji w opisie zlecenia).
    /// </summary>
    public class EnemyContactDamage : MonoBehaviour
    {
        [SerializeField] private float damagePerHit = 5f;
        [SerializeField] private float damageCooldown = 1f;

        private float _lastDamageTime = -999f;

        private void OnTriggerStay(Collider other)
        {
            if (Time.time - _lastDamageTime < damageCooldown)
            {
                return;
            }

            PlayerStatsBridge playerStatsBridge = other.GetComponent<PlayerStatsBridge>();

            if (playerStatsBridge == null)
            {
                return;
            }

            playerStatsBridge.TakeDamage(damagePerHit);
            _lastDamageTime = Time.time;
        }
    }
}
