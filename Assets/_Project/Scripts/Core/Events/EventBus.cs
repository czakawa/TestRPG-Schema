using System;

namespace Project.Core.Events
{
    /// <summary>
    /// Backing store dla pojedynczego typu zdarzenia. Podział per-typ (zamiast jednego Dictionary&lt;Type, Delegate&gt;)
    /// eliminuje boxing struktur i alokacje przy każdej publikacji zdarzenia.
    /// </summary>
    internal static class EventBus<T> where T : struct
    {
        private static event Action<T> Handlers;

        internal static void Subscribe(Action<T> handler) => Handlers += handler;

        internal static void Unsubscribe(Action<T> handler) => Handlers -= handler;

        internal static void Publish(in T eventData) => Handlers?.Invoke(eventData);

        internal static void Clear() => Handlers = null;
    }

    /// <summary>
    /// Statyczna, generyczna szyna zdarzeń pub/sub używana do komunikacji między systemami/UI
    /// bez bezpośrednich referencji (np. UI nie referencjonuje bezpośrednio systemu Combat).
    /// Zdarzenia muszą być readonly struct, żeby publikacja nie alokowała na stercie.
    /// </summary>
    public static class EventBus
    {
        /// <summary>Subskrybuje handler na zdarzenia typu T.</summary>
        public static void Subscribe<T>(Action<T> handler) where T : struct
        {
            EventBus<T>.Subscribe(handler);
        }

        /// <summary>Usuwa subskrypcję handlera dla zdarzeń typu T.</summary>
        public static void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            EventBus<T>.Unsubscribe(handler);
        }

        /// <summary>Publikuje zdarzenie do wszystkich subskrybentów typu T.</summary>
        public static void Publish<T>(in T eventData) where T : struct
        {
            EventBus<T>.Publish(in eventData);
        }
    }
}
