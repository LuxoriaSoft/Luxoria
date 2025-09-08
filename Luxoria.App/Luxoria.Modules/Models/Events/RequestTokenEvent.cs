using Luxoria.Modules.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Luxoria.Modules.Models.Events;

[ExcludeFromCodeCoverage]
public class RequestTokenEvent(Action<string> onHandleReceived) : IEvent
{
    public Action<string>? OnHandleReceived { get; } = onHandleReceived;
}
