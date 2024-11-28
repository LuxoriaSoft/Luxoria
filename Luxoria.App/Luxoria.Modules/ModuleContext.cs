using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models;
using System;

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
        if (image == null)
        {
            throw new ArgumentNullException(nameof(image), "Image cannot be null.");
        }

        _currentImage = image;
    }

    public void LogMessage(string message)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message), "Log message cannot be null.");
        }

        Console.WriteLine(message);
    }
}