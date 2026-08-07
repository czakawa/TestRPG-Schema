using System.Collections.Generic;
using UnityEngine;

namespace Project.Core.Systems
{
    /// <summary>
    /// Centralna pętla gry. Rejestruje obiekty implementujące <see cref="IGameSystem"/> i dystrybuuje do nich
    /// Update/FixedUpdate przez pojedynczą, preallokowaną listę (bez alokacji GC w trakcie działania).
    /// Pozwala bezpiecznie wstrzymać wszystkie systemy przez <see cref="SetSystemsActive"/> bez niszczenia obiektów.
    /// </summary>
    public class GameSystemsManager : MonoBehaviour
    {
        private const int InitialCapacity = 32;

        private readonly List<IGameSystem> _systems = new List<IGameSystem>(InitialCapacity);
        private bool _systemsActive = true;

        /// <summary>Rejestruje system w pętli gry i od razu go inicjalizuje.</summary>
        public void RegisterSystem(IGameSystem system)
        {
            if (system == null || _systems.Contains(system))
            {
                return;
            }

            _systems.Add(system);
            system.Initialize();
        }

        /// <summary>Wyrejestrowuje system i wywołuje jego Shutdown.</summary>
        public void UnregisterSystem(IGameSystem system)
        {
            if (system == null)
            {
                return;
            }

            if (_systems.Remove(system))
            {
                system.Shutdown();
            }
        }

        /// <summary>Włącza lub wyłącza tickowanie wszystkich zarejestrowanych systemów (np. na czas pauzy).</summary>
        public void SetSystemsActive(bool active)
        {
            _systemsActive = active;
        }

        private void Update()
        {
            if (!_systemsActive)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            int count = _systems.Count;
            for (int i = 0; i < count; i++)
            {
                _systems[i].Tick(deltaTime);
            }
        }

        private void FixedUpdate()
        {
            if (!_systemsActive)
            {
                return;
            }

            float fixedDeltaTime = Time.fixedDeltaTime;
            int count = _systems.Count;
            for (int i = 0; i < count; i++)
            {
                _systems[i].FixedTick(fixedDeltaTime);
            }
        }

        private void OnDestroy()
        {
            int count = _systems.Count;
            for (int i = 0; i < count; i++)
            {
                _systems[i].Shutdown();
            }

            _systems.Clear();
        }
    }
}
