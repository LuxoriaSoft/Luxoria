using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Luxoria.App.Components
{
    public sealed partial class MainMenuBarComponent : UserControl
    {
        public MainMenuBarComponent()
        {
            InitializeComponent();
            InitializeBase();
        }

        // Initialize the base component
        private void InitializeBase()
        {
            // Create a flyout menu called "Luxoria"
            // Inside there are two items: "About" and "Exit"
            AddMenuBarItem("Luxoria", new string [] { "About", "Exit" }, new RoutedEventHandler[] { About_Click, Exit_Click });
        }

        private async void About_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create the dialog
                var dialog = new ContentDialog
                {
                    Title = "About Luxoria",
                    Content = "Luxoria v1.0.0",
                    CloseButtonText = "Close"
                };

                // Set the XamlRoot to the current window's XamlRoot
                dialog.XamlRoot = this.XamlRoot;

                // Show the dialog and await for the user to close it
                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur
                Debug.WriteLine($"Error showing dialog: {ex.Message}");
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            // Close the app
            Application.Current.Exit();
        }

        // Add a single menu item with an action
        public void AddMenuBarItem(string itemTitle, RoutedEventHandler action)
        {
            var menuBarItem = new MenuBarItem { Title = itemTitle };
            var flyoutItem = new MenuFlyoutItem { Text = itemTitle };
            flyoutItem.Click += action; // Subscribe to the click event

            menuBarItem.Items.Add(flyoutItem);
            MainMenuBar.Items.Add(menuBarItem);
        }

        // Add a menu bar item with a flyout
        public void AddMenuBarItem(string itemTitle, string[] flyoutItemTitles, RoutedEventHandler[] actions)
        {
            var menuBarItem = new MenuBarItem { Title = itemTitle };

            for (int i = 0; i < flyoutItemTitles.Length; i++)
            {
                var flyoutItem = new MenuFlyoutItem { Text = flyoutItemTitles[i] };
                flyoutItem.Click += actions[i]; // Subscribe to the specific click event
                menuBarItem.Items.Add(flyoutItem);
            }

            MainMenuBar.Items.Add(menuBarItem);
        }
    }
}
