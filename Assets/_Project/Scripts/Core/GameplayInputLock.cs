using System.Collections.Generic;

namespace Project.Core
{
    /// <summary>
    /// Statyczny, globalny stan blokad inputu gameplayowego - CELOWY wyjątek od wzorca IGameSystem
    /// (podobnie jak EventBus). Mosty czytające input bezpośrednio w Update() (PlayerMotorBridge,
    /// CameraOrbitBridge) nie przechodzą przez GameSystemsManager.SetSystemsActive, więc potrzebują
    /// osobnego, jawnie sprawdzanego sygnału. Ruch i kamera mają niezależne zbiory blokad, każda
    /// pod kluczem "reason" (np. "Dialogue", "Inventory") - wiele jednoczesnych źródeł blokady
    /// nie psuje się nawzajem przy niedopasowanym Lock/Unlock, dopóki reason jest spójny.
    /// </summary>
    public static class GameplayInputLock
    {
        private static readonly HashSet<string> _movementLocks = new HashSet<string>();
        private static readonly HashSet<string> _cameraLocks = new HashSet<string>();

        public static bool IsMovementLocked => _movementLocks.Count > 0;

        public static bool IsCameraLocked => _cameraLocks.Count > 0;

        public static void LockMovement(string reason)
        {
            _movementLocks.Add(reason);
        }

        public static void UnlockMovement(string reason)
        {
            _movementLocks.Remove(reason);
        }

        public static void LockCamera(string reason)
        {
            _cameraLocks.Add(reason);
        }

        public static void UnlockCamera(string reason)
        {
            _cameraLocks.Remove(reason);
        }

        /// <summary>Czyści obie blokady - do wywołania np. przy restarcie/zmianie sceny, żeby nie zostały "zawieszone" blokady.</summary>
        public static void ClearAll()
        {
            _movementLocks.Clear();
            _cameraLocks.Clear();
        }
    }
}
