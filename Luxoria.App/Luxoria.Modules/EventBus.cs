using Luxoria.Modules.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Luxoria.Modules
{
    public class EventBus : IEventBus
    {
        // Store both synchronous and asynchronous handlers
        private readonly Dictionary<Type, List<Delegate>> _subscriptions = new();

        /// <summary>
        /// Publishes an event to all subscribed handlers.
        /// Supports both synchronous and asynchronous execution.
        /// </summary>
        public async Task Publish<TEvent>(TEvent @event)
        {
            // Check if there are subscriptions for the event type
            if (_subscriptions.TryGetValue(typeof(TEvent), out var handlers))
            {
                var tasks = new List<Task>();

                foreach (var handler in handlers)
                {
                    if (handler is Func<TEvent, Task> asyncHandler)
                    {
                        // Invoke async handler and add it to the task list
                        tasks.Add(asyncHandler.Invoke(@event));
                    }
                    else if (handler is Action<TEvent> syncHandler)
                    {
                        // Run sync handler directly
                        syncHandler.Invoke(@event);
                    }
                }

                // Await all async handlers to complete
                await Task.WhenAll(tasks);
            }
        }

        /// <summary>
        /// Subscribes a synchronous handler to the specified event type.
        /// </summary>
        public void Subscribe<TEvent>(Action<TEvent> handler)
        {
            if (!_subscriptions.ContainsKey(typeof(TEvent)))
            {
                _subscriptions[typeof(TEvent)] = new List<Delegate>();
            }

            // Add the synchronous handler to the subscription list
            _subscriptions[typeof(TEvent)].Add(handler);
        }

        /// <summary>
        /// Subscribes an asynchronous handler to the specified event type.
        /// </summary>
        public void Subscribe<TEvent>(Func<TEvent, Task> asyncHandler)
        {
            if (!_subscriptions.ContainsKey(typeof(TEvent)))
            {
                _subscriptions[typeof(TEvent)] = new List<Delegate>();
            }

            // Add the async handler to the subscription list
            _subscriptions[typeof(TEvent)].Add(asyncHandler);
        }

        /// <summary>
        /// Unsubscribes a synchronous handler from the specified event type.
        /// </summary>
        public void Unsubscribe<TEvent>(Action<TEvent> handler)
        {
            if (_subscriptions.TryGetValue(typeof(TEvent), out var handlers))
            {
                handlers.Remove(handler);
            }
        }

        /// <summary>
        /// Unsubscribes an asynchronous handler from the specified event type.
        /// </summary>
        public void Unsubscribe<TEvent>(Func<TEvent, Task> asyncHandler)
        {
            if (_subscriptions.TryGetValue(typeof(TEvent), out var handlers))
            {
                handlers.Remove(asyncHandler);
            }
        }
    }
}
