using Luxoria.Modules.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Luxoria.Modules.Models.Events;

[ExcludeFromCodeCoverage]
public class RequestLatestCollection(Action<ICollection<LuxAsset>> onHandleReceived) : IEvent
{
    public Action<ICollection<LuxAsset>>? OnHandleReceived { get; } = onHandleReceived;
}
