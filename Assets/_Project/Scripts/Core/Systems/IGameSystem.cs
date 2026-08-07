namespace Project.Core.Systems
{
    /// <summary>
    /// Kontrakt dla wszystkich systemów gameplayowych zarządzanych przez <see cref="GameSystemsManager"/>.
    /// Implementacje powinny być czystymi klasami C# (bez dziedziczenia po MonoBehaviour),
    /// aby pozostać testowalne i niezależne od cyklu życia silnika.
    /// </summary>
    public interface IGameSystem
    {
        /// <summary>Wywoływane raz po zarejestrowaniu systemu, przed pierwszym Tick.</summary>
        void Initialize();

        /// <summary>Wywoływane co klatkę (odpowiednik Update) przez GameSystemsManager.</summary>
        void Tick(float deltaTime);

        /// <summary>Wywoływane co krok fizyki (odpowiednik FixedUpdate) przez GameSystemsManager.</summary>
        void FixedTick(float fixedDeltaTime);

        /// <summary>Wywoływane przy wyrejestrowaniu systemu lub zamknięciu gry, do zwolnienia zasobów.</summary>
        void Shutdown();
    }
}
