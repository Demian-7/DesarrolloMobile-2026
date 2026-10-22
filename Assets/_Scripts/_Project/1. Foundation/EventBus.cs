using System;
using System.Collections.Generic;

namespace Game.Foundation
{
    public interface IEventBus
    {
        void Subscribe<T>(Action<T> handler);
        void Unsubscribe<T>(Action<T> handler);
        void Publish<T>(T eventMessage);
    }

    public class EventBus : IEventBus
    {
        private readonly Dictionary<Type, Delegate> _handlers = new();

        public void Subscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (_handlers.ContainsKey(type))
            {
                _handlers[type] = Delegate.Combine(_handlers[type], handler);
            }
            else
            {
                _handlers[type] = handler;
            }
        }

        public void Unsubscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (_handlers.ContainsKey(type))
            {
                var current = _handlers[type];
                _handlers[type] = Delegate.Remove(current, handler);
                if (_handlers[type] == null)
                    _handlers.Remove(type);
            }
        }

        public void Publish<T>(T eventMessage)
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var handler))
            {
                (handler as Action<T>)?.Invoke(eventMessage);
            }
        }
    }
}