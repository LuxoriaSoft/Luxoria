using Luxoria.App.Interfaces;
using Luxoria.Modules;
using Luxoria.Modules.Interfaces;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Luxoria.App.Views
{
    public sealed partial class ModuleManagerView : Page
    {
        private readonly IModuleService _moduleService;
        private readonly Window _mainWindow;

        // Utilisation d'ObservableCollection pour les mises à jour automatiques de l'interface utilisateur
        public ObservableCollection<IModule> Modules { get; private set; }

        public ModuleManagerView(IModuleService moduleService, Window mainWindow)
        {
            InitializeComponent();
            _moduleService = moduleService;
            _mainWindow = mainWindow;

            // Initialise la collection avec les modules actuels
            Modules = new ObservableCollection<IModule>(_moduleService.GetModules());
            ModuleListView.ItemsSource = Modules;  // Lier la ListView à l'ObservableCollection
        }

        private async void AddModule_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add(".dll");

            // Obtenir le handle de la fenêtre principale
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(_mainWindow);

            // Initialiser le FileOpenPicker avec le handle
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
                        // Vérification de la présence du module dans la collection
                        if (Modules.Any(m => m.Name == module.Name && m.Version == module.Version))
                        {
                            ShowError("Ce module est déjà chargé.");
                            return;
                        }

                        // Ajoute le module s'il n'existe pas encore
                        _moduleService.AddModule(module);
                        Modules.Add(module);  // Ajoute le module à l'ObservableCollection
                        Debug.WriteLine($"Module '{module.Name}' ajouté avec succès.");
                    }
                    else
                    {
                        ShowError("Le fichier sélectionné n'est pas un module valide.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Erreur lors de l'ajout du module : {ex.Message}");
                    ShowError("Erreur lors de l'ajout du module.");
                }
            }
        }


        private void RemoveModule_Click(object sender, RoutedEventArgs e)
        {
            if (ModuleListView.SelectedItem is IModule selectedModule)
            {
                _moduleService.RemoveModule(selectedModule);
                Modules.Remove(selectedModule);  // Supprime le module de l'ObservableCollection
                Debug.WriteLine($"Module supprimé avec succès.");
            }
            else
            {
                ShowError("Veuillez sélectionner un module à supprimer.");
            }
        }

        private void ShowError(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Erreur",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            _ = dialog.ShowAsync();
        }
    }
}
