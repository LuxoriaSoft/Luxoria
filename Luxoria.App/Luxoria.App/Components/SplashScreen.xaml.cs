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

        // Expose publicly CurrentModuleText 
        public TextBlock CurrentModuleTextBlock => CurrentModuleText;

        // Expose publicly VersionInfoText
        public TextBlock VersionInfoTextBlock => VersionInfoText;
    }
}
