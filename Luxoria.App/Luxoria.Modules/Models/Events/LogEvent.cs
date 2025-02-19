using Luxoria.Modules.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Luxoria.Modules.Models.Events;

/// <summary>
/// Event used to log messages within the system.
/// This event can be published to capture and process log messages.
/// </summary>
[ExcludeFromCodeCoverage]
public class LogEvent : IEvent
{
    /// <summary>
    /// Gets the log message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogEvent"/> class.
    /// </summary>
    /// <param name="message">The log message to be recorded.</param>
    public LogEvent(string message)
    {
        Message = message;
    }
}
