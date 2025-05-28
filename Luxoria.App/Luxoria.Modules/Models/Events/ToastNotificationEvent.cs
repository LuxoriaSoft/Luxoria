using Luxoria.Modules.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Luxoria.Modules.Models.Events;

[ExcludeFromCodeCoverage]
public class ToastNotificationEvent : IEvent
{
    /// <summary>
    /// Gets or sets the title of the toast notification.
    /// </summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the message of the toast notification.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
