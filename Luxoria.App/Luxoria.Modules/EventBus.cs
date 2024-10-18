using Luxoria.Modules.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxoria.Modules;

public class EventBus : IEventBus
{
    private readonly Dictionary<Type, List<Delegate>> _subscriptions = new();

    public void Publish<TEvent>(TEvent @event)
    {
        if (_subscriptions.ContainsKey(typeof(TEvent)))
        {
            foreach (var handler in _subscriptions[typeof(TEvent)].OfType<Action<TEvent>>())
            {
                handler.Invoke(@event);
            }
        }
    }

    public void Subscribe<TEvent>(Action<TEvent> handler)
    {
        if (!_subscriptions.ContainsKey(typeof(TEvent)))
        {
            _subscriptions[typeof(TEvent)] = new List<Delegate>();
        }

        _subscriptions[typeof(TEvent)].Add(handler);
    }

    public void Unsubscribe<TEvent>(Action<TEvent> handler)
    {
        if (_subscriptions.ContainsKey(typeof(TEvent)))
        {
            _subscriptions[typeof(TEvent)].Remove(handler);
        }
    }
}
