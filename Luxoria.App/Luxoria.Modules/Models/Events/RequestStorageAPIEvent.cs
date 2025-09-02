using Luxoria.Modules.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Luxoria.Modules.Models.Events;

[ExcludeFromCodeCoverage]
public class RequestStorageAPIEvent(string vaultName, Action<IStorageAPI> onHandleReceived) : IEvent
{
    public string VaultName { get; } = vaultName;
    public Action<IStorageAPI>? OnHandleReceived { get; } = onHandleReceived;
}
