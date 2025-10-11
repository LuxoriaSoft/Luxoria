using Luxoria.Modules.Interfaces;

namespace Luxoria.Modules.Models.Events;

/// <summary>
/// Application Ready Event
/// This record signals that the application has completed its startup process and is fully operational
/// </summary>
public record ApplicationReadyEvent : IEvent;