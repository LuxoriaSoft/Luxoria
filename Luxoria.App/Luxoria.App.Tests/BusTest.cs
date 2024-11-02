using Luxoria.Core.Interfaces;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models;
using Luxoria.Modules.Models.Events;
using Moq;
using Xunit;

namespace Luxoria.Modules
{
    public class ModuleContext : IModuleContext
    {
        private readonly IEventBus _eventBus;
        private ImageData _currentImage;

        public ModuleContext(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public ImageData GetCurrentImage()
        {
            return _currentImage;
        }

        public void UpdateImage(ImageData image)
        {
            _currentImage = image;
        }

        public void LogMessage(string message)
        {
            _eventBus.Publish(new LogEvent(message));
        }
    }
}