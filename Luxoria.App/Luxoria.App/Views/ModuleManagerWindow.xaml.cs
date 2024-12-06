using Luxoria.App.ViewModels;
using Luxoria.Core.Interfaces;
using Luxoria.Modules;
using Luxoria.Modules.Interfaces;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Luxoria.App.Views
{
    /// <summary>
    /// Represents the module manager window that allows adding and removing modules.
    /// </summary>
    public sealed partial class ModuleManagerWindow : Page
    {
        private readonly IModuleService _moduleService;
        private readonly Window _mainWindow;

        /// <summary>
        /// Gets a non-generic list of modules for UI compatibility.
        /// </summary>
        public IList Modules { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleManagerWindow"/> class.
        /// </summary>
        /// <param name="moduleService">The service to manage modules.</param>
        /// <param name="mainWindow">The main application window.</param>
        public ModuleManagerWindow(IModuleService moduleService, Window mainWindow)
        {
            InitializeComponent();
            _moduleService = moduleService ?? throw new ArgumentNullException(nameof(moduleService));
            _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));

            // Initialize Modules with a non-generic representation
            Modules = ConvertToNonGeneric(_moduleService.GetModules());
            ModuleListView.ItemsSource = Modules;
        }

        /// <summary>
        /// Converts a collection of <see cref="IModule"/> to a non-generic list for UI binding.
        /// </summary>
        /// <param name="modules">The collection of modules.</param>
        /// <returns>A non-generic list of modules wrapped in <see cref="ModuleViewModel"/>.</returns>
        private IList ConvertToNonGeneric(IEnumerable<IModule> modules)
        {
            return modules.Select(m => new ModuleViewModel(m)).ToList();
        }

        /// <summary>
        /// Handles the "Add Module" button click event.
        /// Allows the user to add a new module to the application.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private async void AddModule_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            picker.FileTypeFilter.Add(".dll");

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(_mainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                try
                {
                    var modulePath = file.Path;
                    IModule module = new ModuleLoader().LoadModule(modulePath);

                    if (module != null)
                    {
                        // Check if the module already exists
                        if (Modules.Cast<ModuleViewModel>().Any(m => m.Name == module.Name && m.Version == module.Version))
                        {
                            ShowError("This module is already loaded.");
                            return;
                        }

                        _moduleService.AddModule(module);
                        Modules.Add(new ModuleViewModel(module));
                        RefreshModuleList();
                        Debug.WriteLine($"Module '{module.Name}' added successfully.");
                    }
                    else
                    {
                        ShowError("The selected file is not a valid module.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error while adding the module: {ex}");
                    ShowError("An error occurred while adding the module.");
                }
            }
        }

        /// <summary>
        /// Handles the "Remove Module" button click event.
        /// Allows the user to remove a selected module from the application.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void RemoveModule_Click(object sender, RoutedEventArgs e)
        {
            if (ModuleListView.SelectedItem is ModuleViewModel selectedModule)
            {
                _moduleService.RemoveModule(selectedModule.Module);
                Modules.Remove(selectedModule);
                RefreshModuleList();
                Debug.WriteLine($"Module '{selectedModule.Name}' removed successfully.");
            }
            else
            {
                ShowError("Please select a module to remove.");
            }
        }

        /// <summary>
        /// Refreshes the module list to update the UI.
        /// </summary>
        private void RefreshModuleList()
        {
            ModuleListView.ItemsSource = null;
            ModuleListView.ItemsSource = Modules;
        }

        /// <summary>
        /// Displays an error message to the user.
        /// </summary>
        /// <param name="message">The error message to display.</param>
        private async void ShowError(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }
    }

}
