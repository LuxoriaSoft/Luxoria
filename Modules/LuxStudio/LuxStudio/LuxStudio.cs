using Luxoria.GModules;
using Luxoria.GModules.Interfaces;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models.Events;
using Luxoria.SDK.Interfaces;
using Luxoria.SDK.Models;
using LuxStudio.COM.Auth;
using LuxStudio.COM.Models;
using LuxStudio.COM.Services;
using LuxStudio.Components;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;

namespace LuxStudio;

public class LuxStudio : IModule, IModuleUI
{
    private IEventBus? _eventBus;
    private IModuleContext? _context;
    private ILoggerService? _logger;

    public string Name => "LuxStudio";
    public string Description => "Generic Luxoria LuxStudio Integration Module";
    public string Version => "1.0.0";

    private LuxStudioConfig? _config;
    private AuthManager? _authManager;

    private AccManagementView? _accManagementView;
    private CollectionManagementView? _collectionManagementView;
    private Chat? _chat;

    private CollectionItem? _selectedCollection;

    /// <summary>
    /// The list of menu bar items to be added to the main menu bar.
    /// </summary>
    public List<ILuxMenuBarItem> Items { get; set; } = new List<ILuxMenuBarItem>();

    /// <summary>
    /// Initializes the module with the provided EventBus and ModuleContext.
    /// </summary>
    /// <param name="eventBus">The event bus for publishing and subscribing to events.</param>
    /// <param name="context">The context for managing module-specific data.</param>
    public void Initialize(IEventBus eventBus, IModuleContext context, ILoggerService logger)
    {
        _eventBus = eventBus;
        _context = context;
        _logger = logger;
        _logger?.Log("LuxStudio initialized", "Mods/LuxStudio", LogLevel.Info);
        
        _eventBus.Subscribe<RequestExportOnlineEvent>(OnExportRequest);

        // Add a menu bar item to the main menu bar
        List<ISmartButton> smartButtons = [];
        _accManagementView = new AccManagementView(ref _authManager);
        _accManagementView.OnAuthenticated += AccManagementView_OnAuthenticated;

        _collectionManagementView = new CollectionManagementView(_eventBus);

        _collectionManagementView.OnAuthenticated += (authManager) =>
        {
            _authManager = authManager;
        };

        _collectionManagementView.OnCollectionItemSelected += async (item) =>
        {
            _selectedCollection = item;
            _chat.ChatURLUpdated?.Invoke(new Uri(new Uri(item.Config?.Url), $"/collections/{item.Id}/chat") ?? new Uri(string.Empty), item.AuthManager);
        };

        _collectionManagementView.NoCollectionSelected += () =>
        {
            _chat.NoCollectionSelected?.Invoke();
            _selectedCollection = null;
        };

        _chat = new Chat();

        Dictionary<SmartButtonType, Object> accountMgt = new()
        {
            { SmartButtonType.Modal, _accManagementView }
        };
        Dictionary<SmartButtonType, Object> collectionMngt = new()
        {
            { SmartButtonType.Modal, _collectionManagementView }
        };
        Dictionary<SmartButtonType, Object> chat = new()
        {
            { SmartButtonType.Window, _chat }
        };

        smartButtons.Add(new SmartButton("Account", "Account Management", accountMgt));
        smartButtons.Add(new SmartButton("Collections", "View Collections", collectionMngt));
        smartButtons.Add(new SmartButton("Chat", "Open Chat", chat));
        Items.Add(new LuxMenuBarItem("LuxStudio", false, new Guid(), smartButtons));
    }

    private void AccManagementView_OnAuthenticated(LuxStudioConfig arg1, AuthManager arg2)
    {
        Debug.WriteLine("Acc manageement view on authenticated called");
        _config = arg1;
        _authManager = arg2;
        _collectionManagementView?.Authenticated?.Invoke(_config, _authManager);

        _logger?.Log("LuxStudio authenticated successfully", "Mods/LuxStudio", LogLevel.Info);
    }

    /// <summary>
    /// Executes the module logic. This can be called to trigger specific actions.
    /// </summary>
    public void Execute()
    {
        _logger?.Log("LuxStudio executed", "Mods/LuxStudio", LogLevel.Info);
        // Additional logic can be added here if needed
    }

    /// <summary>
    /// Cleans up resources and subscriptions when the module is shut down.
    /// </summary>
    public void Shutdown()
    {
        // Unsubscribe from events if necessary to avoid memory leaks
        _logger?.Log("LuxStudio shutdown", "Mods/LuxStudio", LogLevel.Info);
    }

    StreamContent CreateStreamContent(string filePath,
                                        string contentType = "application/octet-stream")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found.", filePath);

        var stream = new FileStream(
            path: filePath,
            mode: FileMode.Open,
            access: FileAccess.Read,
            share: FileShare.Read,
            bufferSize: 4096,
            useAsync: true);

        var content = new StreamContent(stream);
        content.Headers.ContentType =
            new MediaTypeHeaderValue(contentType);

        content.Headers.ContentDisposition =
            new ContentDispositionHeaderValue("form-data")
            {
                Name = "\"file\"",
                FileName = $"\"{Path.GetFileName(filePath)}\""
            };

        return content;
    }

    private async void OnExportRequest(RequestExportOnlineEvent evt)
    {
        Debug.WriteLine("Export request received for asset");
        if (_selectedCollection == null || _selectedCollection.AuthManager == null || _selectedCollection.Config == null)
        {
            return;
        }

        try
        {
            var cs = new CollectionService(_selectedCollection.Config ?? throw new InvalidOperationException("Configuration cannot be null. Ensure the config service is properly initialized."), _eventBus);

            StreamContent strm = CreateStreamContent(evt.AssetPath, "image/jpeg");
            var token = await _selectedCollection.AuthManager.GetAccessTokenAsync();
            var response = await cs.UploadAssetAsync(evt.Asset.Id, token, _selectedCollection.Id, $"tbe{evt.Asset.Id}.jpeg", strm, evt.Asset.MetaData.LastUploadId);
        }
        catch (Exception ex)
        {
        }
    }
}
