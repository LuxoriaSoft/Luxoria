using Microsoft.UI.Windowing;
using System;
using System.IO;

namespace Luxoria.GModules.Helpers;

/// <summary>
/// Window Helper for AppWindow
/// </summary>
public class WindowHelper
{
    /// <summary>
    /// Loads the application icon from the specified file path.
    /// </summary>
    /// <param name="iconName">Icon Name without extension e.g Luxoria_logo</param>
    /// <exception cref="FileNotFoundException">To be thrown if file do NOT exist</exception>
    public static void SetCaption(AppWindow target, string iconName)
    {
        string path = Path.Combine(AppContext.BaseDirectory, $"Assets/{iconName}.ico");

        if (!Path.Exists(path)) throw new FileNotFoundException($"Icon file not found at path: {path}");

        target.SetIcon(path);
    }

    /// <summary>
    /// Resizes the application window to the specified width and height.
    /// </summary>
    /// <param name="target">Target AppWindow</param>
    /// <param name="width">Width (px)</param>
    /// <param name="height">Height (px)</param>
    public static void SetSize(AppWindow target, int width, int height) =>
        // Set the window size
        target.Resize(new Windows.Graphics.SizeInt32 { Width = width, Height = height });


    /// <summary>
    /// Set title of the application window.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="title"></param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static void SetTitle(AppWindow target, string title) => target.Title = title;
}
