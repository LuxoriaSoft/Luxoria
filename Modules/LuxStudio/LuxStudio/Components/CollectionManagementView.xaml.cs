using Luxoria.Modules.Interfaces;
using LuxStudio.COM.Auth;
using LuxStudio.COM.Models;
using LuxStudio.COM.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LuxStudio.Components
{
    public class CollectionItem
    {
        public string Name { get; private set; }
        public string Description { get; private set; }

        public Guid Id { get; private set; }

        public AuthManager? AuthManager { get; private set; }

        public LuxStudioConfig? Config { get; private set; }
        public CollectionItem(string name, string description, Guid id, AuthManager? authManager, LuxStudioConfig? config)
        {
            Name = name;
            Description = description;
            Id = id;
            AuthManager = authManager;
            Config = config;
        }
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CollectionManagementView : Page, INotifyPropertyChanged
    {
        public ObservableCollection<CollectionItem> CollectionItems { get; set; } = new ObservableCollection<CollectionItem>();
        private CollectionService? _collectionService;
        public Action<LuxStudioConfig, AuthManager>? Authenticated;
        
        private bool _isAuthenticated = false;
        private bool _isNotAuthenticated => !_isAuthenticated;
        private AuthManager? _authManager;

        public event Action<AuthManager>? OnAuthenticated;

        public Visibility AuthenticatedVisibility => _isAuthenticated ? Visibility.Visible : Visibility.Collapsed;
        public Visibility NotAuthenticatedVisibility => _isAuthenticated ? Visibility.Collapsed : Visibility.Visible;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public event Action<CollectionItem> OnCollectionItemSelected;

        public CollectionManagementView(IEventBus eventBus)
        {
            InitializeComponent();

            Authenticated += async (config, authManager) =>
            {
                _authManager = authManager;
                _collectionService = new CollectionService(config, eventBus);
                if (authManager == null) return;
                ICollection<LuxCollection> allCollections = await _collectionService.GetAllAsync(await authManager!.GetAccessTokenAsync());
                CollectionItems.Clear();
                foreach (var collection in allCollections)
                {
                    CollectionItems.Add(new CollectionItem(collection.Name, collection.Description, collection.Id, _authManager, config));
                }
                _isAuthenticated = true;
            };
        }

        private async void Authenticate(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var cfgSrv = new ConfigService("https://studio.pluto.luxoria.bluepelicansoft.com/");
            var config = await cfgSrv.GetConfigAsync();
            _authManager = new(config ?? throw new Exception("Config service not correctly configurated"));

            try
            {
                await _authManager.GetAccessTokenAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during authentication: {ex.Message}");
                return;
            }

            OnAuthenticated?.Invoke(_authManager);
            Authenticated?.Invoke(config, _authManager);
            _isAuthenticated = true;
            OnPropertyChanged(nameof(AuthenticatedVisibility));
            OnPropertyChanged(nameof(NotAuthenticatedVisibility));
        }

        private void CollectionListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("Collection changed!");
            OnCollectionItemSelected?.Invoke((CollectionItem)((ListView)sender).SelectedItem);
        }
    }
}
