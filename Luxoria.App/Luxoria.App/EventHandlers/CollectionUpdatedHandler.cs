using Luxoria.Modules.Models.Events;
using Luxoria.SDK.Interfaces;
using Windows.UI.Notifications;

namespace Luxoria.App.EventHandlers;

/// <summary>
/// Handles the CollectionUpdatedEvent.
/// </summary>
public class CollectionUpdatedHandler
{
    /// <summary>
    /// The logger service.
    /// </summary>
    private readonly ILoggerService _loggerService;

    /// <summary>
    /// Constructor for the CollectionUpdatedHandler.
    /// </summary>
    /// <param name="loggerService"></param>
    public CollectionUpdatedHandler(ILoggerService loggerService)
    {
        _loggerService = loggerService;
    }

    /// <summary>
    /// Handles the CollectionUpdatedEvent.
    /// </summary>
    public void OnCollectionUpdated(CollectionUpdatedEvent body)
    {
        _loggerService.Log($"Collection updated: {body.CollectionName}");
        _loggerService.Log($"Collection path: {body.CollectionPath}");

        var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
        var textNodes = toastXml.GetElementsByTagName("text");
        textNodes[0].AppendChild(toastXml.CreateTextNode($"Updated Collection: {body.CollectionName}"));
        var toast = new ToastNotification(toastXml);
        ToastNotificationManager.CreateToastNotifier("Luxoria").Show(toast);

        /*
        for (int i = 0; i < body.Assets.Count; i++)
        {
            ImageData imageData = body.Assets.ElementAt(i).Data;
            //_loggerService.Log($"Asset {i}: {body.Assets.ElementAt(i).MetaData.Id}");
            //_loggerService.Log($"Asset info {i} : {imageData.Height}x{imageData.Width}, pixels : {imageData.Height * imageData.Width}, exif : {imageData.EXIF}");
        }
        */
    }
}
