using Luxoria.Modules.Models.Events;
using Luxoria.SDK.Interfaces;
using System.Linq;
using Windows.UI.Notifications;

namespace Luxoria.App.EventHandlers
{
    public class CollectionUpdatedHandler
    {
        private readonly ILoggerService _loggerService;

        public CollectionUpdatedHandler(ILoggerService loggerService)
        {
            _loggerService = loggerService;
        }

        public void OnCollectionUpdated(CollectionUpdatedEvent body)
        {
            _loggerService.Log($"Collection updated: {body.CollectionName}");
            _loggerService.Log($"Collection path: {body.CollectionPath}");

            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
            var textNodes = toastXml.GetElementsByTagName("text");
            textNodes[0].AppendChild(toastXml.CreateTextNode($"Updated Collection: {body.CollectionName}"));
            var toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier("Luxoria").Show(toast);

            for (int i = 0; i < body.Assets.Count; i++)
            {
                _loggerService.Log($"Asset {i}: {body.Assets.ElementAt(i).MetaData.Id}");
            }
        }
    }
}
