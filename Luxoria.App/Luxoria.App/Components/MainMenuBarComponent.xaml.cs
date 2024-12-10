using Luxoria.App.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;

namespace Luxoria.App.Components
{
    public sealed partial class MainMenuBarComponent : UserControl
    {
        public MainMenuBarComponent()
        {
            InitializeComponent();
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Navigating to Home");
            // Add navigation logic to Home here
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Starting Import");
            // Add import start logic here
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Starting Edit");
            // Add editing start logic here
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Starting Export");
            // Add export start logic here
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Opening Settings");
            // Add logic to open settings here
        }
        private void Modules_Click(object sender, RoutedEventArgs e)
        {
            //var moduleService = (Application.Current as App).ModuleService;

            //var newWindow = new Microsoft.UI.Xaml.Window();
            //var moduleManagerPage = new ModuleManagerWindow(moduleService, newWindow);
            //newWindow.Content = moduleManagerPage;
            //newWindow.Activate();
        }



    }
}
