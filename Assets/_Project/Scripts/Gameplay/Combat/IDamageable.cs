namespace Project.Gameplay.Combat
{
    /// <summary>
    /// Kontrakt dla dowolnego obiektu sceny, który może otrzymać obrażenia w walce (na razie tylko
    /// EnemyController, docelowo także sam gracz). Implementowany przez MonoBehaviour osadzone na
    /// obiektach z colliderem na warstwie "Enemy".
    /// </summary>
    public interface IDamageable
    {
        /// <summary>Odejmuje obrażenia od aktualnego zdrowia; implementacja odpowiada za clamp do zera.</summary>
        void TakeDamage(float amount);

        /// <summary>Czy obiekt już zginął - sprawdzane przed próbą zadania kolejnych obrażeń.</summary>
        bool IsDead { get; }
    }
}
