using Luxoria.Modules.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Luxoria.Modules.Models.Events;

/// <summary>
/// Event used to request a window handle.
/// The provided callback will be invoked with the retrieved window handle.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="RequestWindowHandleEvent"/> class.
/// </remarks>
/// <param name="onHandleReceived">Callback that receives the window handle as a parameter.</param>
[ExcludeFromCodeCoverage]
public class RequestWindowHandleEvent(Action<nint> onHandleReceived) : IEvent
{
    /// <summary>
    /// Gets the callback action that will be invoked when the window handle is received.
    /// </summary>
    public Action<nint>? OnHandleReceived { get; } = onHandleReceived;
}
