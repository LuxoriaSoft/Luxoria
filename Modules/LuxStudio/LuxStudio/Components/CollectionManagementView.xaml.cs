using Luxoria.Modules;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models.Events;
using LuxStudio.COM.Auth;
using LuxStudio.COM.Models;
using LuxStudio.COM.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using static LuxStudio.COM.Services.CollectionService;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.



namespace LuxStudio.Components;

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

public class EmailItem
{
    public string Value { get; set; }
    public EmailItem(string value) => Value = value;
}


/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class CollectionManagementView : Page, INotifyPropertyChanged
{
    public ObservableCollection<CollectionItem> CollectionItems { get; set; } = new ObservableCollection<CollectionItem>();
    private CollectionService? _collectionService;
    public Action<LuxStudioConfig, AuthManager>? Authenticated;

    public event Action NoCollectionSelected;

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

    private bool _isCreatingCollection = false;
    public Visibility CollectionListVisibility => _isCreatingCollection ? Visibility.Collapsed : Visibility.Visible;
    public Visibility CreateFormVisibility => _isCreatingCollection ? Visibility.Visible : Visibility.Collapsed;
    
    private ObservableCollection<EmailItem> _allowedEmails = new();
    public ObservableCollection<EmailItem> AllowedEmails => _allowedEmails;

    private IEventBus _eventBus;

    private LuxStudioConfig? _config;

    private CollectionItem? _collectionBeingEdited = null;


    public CollectionManagementView(IEventBus eventBus)
    {
        InitializeComponent();
        _eventBus = eventBus;

        Authenticated += async (config, authManager) =>
        {
            _config = config;
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

        CollectionItem item = (CollectionItem)((ListView)sender).SelectedItem;
        if (item == null)
        {
            Debug.WriteLine("No collection selected.");
            return;
        }
        OnCollectionItemSelected?.Invoke(item);
    }

    private void SetCreateMode(bool isCreating, bool isEdit = false)
    {
        _isCreatingCollection = isCreating;
        _collectionBeingEdited = isEdit ? _collectionBeingEdited : null;

        ConfirmCreateOrEditButton.Content = isEdit ? "Update" : "Create";
        OnPropertyChanged(nameof(CollectionListVisibility));
        OnPropertyChanged(nameof(CreateFormVisibility));
    }


    private void StartCreateCollection_Click(object sender, RoutedEventArgs e)
    {
        CollectionNameTextBox.Text = "";
        CollectionDescriptionTextBox.Text = "";
        EmailInputTextBox.Text = "";
        _allowedEmails.Clear();
        SetCreateMode(true);
    }

    private void CancelCreateCollection_Click(object sender, RoutedEventArgs e)
    {
        SetCreateMode(false);
    }

    private async void ConfirmCreateOrEdit_Click(object sender, RoutedEventArgs e)
    {
        if (_collectionService == null || _authManager == null)
            return;

        var name = CollectionNameTextBox.Text.Trim();
        var description = CollectionDescriptionTextBox.Text.Trim();
        var emails = _allowedEmails.Select(e => e.Value).ToList();

        if (string.IsNullOrWhiteSpace(name))
            return;

        if (_collectionBeingEdited == null)
        {
            var created = await _collectionService.CreateCollectionAsync(
                await _authManager.GetAccessTokenAsync(),
                name,
                description,
                emails
            );

            if (created != null)
            {
                var newItem = new CollectionItem(created.Name, created.Description, created.Id, _authManager, _config);
                CollectionItems.Add(newItem);
                CollectionListView.SelectedItem = newItem;
                OnCollectionItemSelected?.Invoke(newItem);

                await _eventBus.Publish(new ToastNotificationEvent
                {
                    Title = "LuxStudio",
                    Message = "Collection created successfully"
                });

                SetCreateMode(false);
            }
            else
            {
                Debug.WriteLine("Failed to create collection.");
            }
        }
        else
        {
            var updateDto = new UpdateCollectionDto
            {
                Name = name,
                Description = description,
                AllowedEmails = emails
            };

            var success = await _collectionService.UpdateCollectionAsync(
                await _authManager.GetAccessTokenAsync(),
                _collectionBeingEdited.Id,
                updateDto
            );

            if (success)
            {
                var updatedItem = new CollectionItem(name, description, _collectionBeingEdited.Id, _authManager, _config);

                var index = CollectionItems.IndexOf(CollectionItems.First(c => c.Id == _collectionBeingEdited.Id));
                if (index >= 0)
                {
                    CollectionItems[index] = updatedItem;
                    CollectionListView.SelectedItem = updatedItem;
                    OnCollectionItemSelected?.Invoke(updatedItem);
                }

                _collectionBeingEdited = null;

                await _eventBus.Publish(new ToastNotificationEvent
                {
                    Title = "LuxStudio",
                    Message = "Collection updated successfully"
                });

                SetCreateMode(false);
            }
            else
            {
                Debug.WriteLine("Failed to update collection.");
            }
        }
    }


    private void EmailInputTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            var email = EmailInputTextBox.Text.Trim();
            if (IsValidEmail(email) && !_allowedEmails.Any(e => e.Value == email))
            {
                _allowedEmails.Add(new EmailItem(email));
                EmailInputTextBox.Text = "";
            }
        }
    }

    private void RemoveEmail_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string email)
        {
            var itemToRemove = _allowedEmails.FirstOrDefault(e => e.Value == email);
            if (itemToRemove != null)
                _allowedEmails.Remove(itemToRemove);
        }
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch { return false; }
    }

    private async void EditCollection_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem item || item.Tag is not CollectionItem selected)
            return;

        if (_collectionService == null || _authManager == null)
            return;

        CollectionNameTextBox.Text = selected.Name;
        CollectionDescriptionTextBox.Text = "";
        _allowedEmails.Clear();

        try
        {
            var fullCollection = await _collectionService.GetAsync(
                await _authManager.GetAccessTokenAsync(),
                selected.Id
            );

            CollectionDescriptionTextBox.Text = fullCollection.Description ?? "";

            foreach (var email in fullCollection.allowedEmails.Where(e => !string.IsNullOrWhiteSpace(e)))
            {
                _allowedEmails.Add(new EmailItem(email));
            }

            _collectionBeingEdited = selected;
            SetCreateMode(true, isEdit: true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[EditCollection] Failed to load collection: {ex.Message}");
            await _eventBus.Publish(new ToastNotificationEvent
            {
                Title = "LuxStudio",
                Message = $"Failed to load collection: {selected.Name}"
            });
        }
    }


    private async void DeleteCollection_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem item || item.Tag is not CollectionItem selected)
            return;

        if (_authManager == null || _collectionService == null)
            return;

        var success = await _collectionService.DeleteCollectionAsync(
            await _authManager.GetAccessTokenAsync(),
            selected.Id
        );

        if (success)
        {
            CollectionItems.Remove(selected);
            NoCollectionSelected.Invoke();
            await _eventBus.Publish(new ToastNotificationEvent
            {
                Title = "LuxStudio",
                Message = $"Collection '{selected.Name}' deleted successfully"
            });
        }
        else
        {
            Debug.WriteLine("Failed to delete collection.");
        }
    }



}
