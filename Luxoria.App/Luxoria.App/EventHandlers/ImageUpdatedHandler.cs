using Luxoria.Modules.Models.Events;
using Luxoria.SDK.Interfaces;

namespace Luxoria.App.EventHandlers
{
    public class ImageUpdatedHandler
    {
        private readonly ILoggerService _loggerService;

        public ImageUpdatedHandler(ILoggerService loggerService)
        {
            _loggerService = loggerService;
        }

        public void OnImageUpdated(ImageUpdatedEvent body)
        {
            _loggerService.Log($"Image updated: {body.ImagePath}");
        }
    }
}
