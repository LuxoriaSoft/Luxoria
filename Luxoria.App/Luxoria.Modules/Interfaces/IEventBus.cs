using System;
using System.Threading.Tasks;

namespace Luxoria.Modules.Interfaces
{
    public interface IEventBus
    {
        // Publishes an event to all subscribed handlers, supporting both async and sync.
        Task Publish<TEvent>(TEvent @event);

        // Subscribes a synchronous handler to the event type.
        void Subscribe<TEvent>(Action<TEvent> handler);

        // Subscribes an asynchronous handler to the event type.
        void Subscribe<TEvent>(Func<TEvent, Task> asyncHandler);

        // Unsubscribes a synchronous handler from the event type.
        void Unsubscribe<TEvent>(Action<TEvent> handler);

        // Unsubscribes an asynchronous handler from the event type.
        void Unsubscribe<TEvent>(Func<TEvent, Task> asyncHandler);
    }
}
