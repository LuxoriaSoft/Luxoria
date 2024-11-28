using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Windowing;
using System;
using Microsoft.UI;

namespace Luxoria.App
{
    public sealed partial class SplashScreen : Window
    {
        public SplashScreen()
        {
            this.InitializeComponent();

            // Set the window size programmatically
            SetWindowSize(800, 450);
        }

        // Expose the TextBlock so the main app can update it during module loading
        public TextBlock CurrentModuleTextBlock => CurrentModuleText;

        private void SetWindowSize(int width, int height)
        {
            // Get the AppWindow associated with the current window
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);

            // Set the window size
            appWindow.Resize(new Windows.Graphics.SizeInt32 { Width = width, Height = height });
        }
    }
}
