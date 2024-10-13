using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models;

namespace Luxoria.Modules;

public class ModuleContext : IModuleContext
{
    private ImageData _currentImage;

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
        // Log the message
    }
}
