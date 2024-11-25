using Luxoria.Modules.Models.Events;
using Luxoria.SDK.Interfaces;

namespace Luxoria.App.EventHandlers;

/// <summary>
/// Handles the ImageUpdatedEvent.
/// </summary>
public class ImageUpdatedHandler
{
    /// <summary>
    /// The logger service.
    /// </summary>
    private readonly ILoggerService _loggerService;

    /// <summary>
    /// Constructor for the ImageUpdatedHandler.
    /// </summary>
    public ImageUpdatedHandler(ILoggerService loggerService)
    {
        _loggerService = loggerService;
    }

    /// <summary>
    /// Handles the ImageUpdatedEvent.
    /// </summary>
    public void OnImageUpdated(ImageUpdatedEvent body)
    {
        _loggerService.Log($"Image updated: {body.ImagePath}");
    }
}
