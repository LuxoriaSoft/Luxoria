using Luxoria.GModules.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Luxoria.App
{
    public sealed partial class SplashScreen : Window
    {
        public SplashScreen()
        {
            this.InitializeComponent();

            // Load App Icon
            WindowHelper.SetCaption(AppWindow, "Luxoria_icon");

            // Set the window size programmatically
            WindowHelper.SetSize(AppWindow, 800, 450);
        }

        // Expose the TextBlock so the main app can update it during module loading
        public TextBlock CurrentModuleTextBlock => CurrentModuleText;
    }
}
