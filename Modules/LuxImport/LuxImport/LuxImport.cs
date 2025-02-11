using LuxImport.Interfaces;
using LuxImport.Services;
using LuxImport.Views;
using Luxoria.GModules;
using Luxoria.GModules.Interfaces;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models.Events;
using Luxoria.SDK.Interfaces;
using Luxoria.SDK.Models;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;

namespace LuxImport;

public class LuxImport : IModule, IModuleUI
{
    private IEventBus? _eventBus;
    private IModuleContext? _context;
    private ILoggerService? _logger;

    public string Name => "LuxImport";
    public string Description => "Generic Luxoria Importation Module";
    public string Version => "1.0.2";

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
        _logger?.Log("LuxImport initialized", "Mods/LuxImport", LogLevel.Info);

        // Subscribe to mandatory events with an async handler
        _eventBus.Subscribe<OpenCollectionEvent>(HandleOnOpenCollectionAsync);

        // Add a menu bar item to the main menu bar
        List<ISmartButton> smartButtons = new List<ISmartButton>();
        List<ISmartButton> smartButtons2 = new List<ISmartButton>();
        Dictionary<SmartButtonType, Page> Pages1 = new Dictionary<SmartButtonType, Page>();
        Dictionary<SmartButtonType, Page> Pages2 = new Dictionary<SmartButtonType, Page>();
        Dictionary<SmartButtonType, Page> Pages3 = new Dictionary<SmartButtonType, Page>();
        Dictionary<SmartButtonType, Page> Pages4 = new Dictionary<SmartButtonType, Page>();

        Pages1.Add(SmartButtonType.MainPanel, new ImportView());
        Pages2.Add(SmartButtonType.Window, new ImportView());
        Pages3.Add(SmartButtonType.Modal, new ImportView());

        Pages4.Add(SmartButtonType.LeftPanel, new ImportView());

        //smartButtons.Add(new SmartButton("Main Panel", "I'm just a button of TestItem", Pages1));
        //smartButtons.Add(new SmartButton("Window", "I'm just a button of TestItem", Pages2));
        smartButtons.Add(new SmartButton("Modal", "I'm just a button of TestItem", Pages3));

        smartButtons2.Add(new SmartButton("Left Panel", "I'm just a button of TestItem", Pages4));


        Items.Add(new LuxMenuBarItem("1TestItem", true, new Guid(), smartButtons));

        Items.Add(new LuxMenuBarItem("1TestItem2", false, new Guid(), smartButtons2));


        //Items.Add(new LuxMenuBarItem("Import", true, new Guid(), new { new SmartButton("Import", "Import module", ) }));
    }

    /// <summary>
    /// Executes the module logic. This can be called to trigger specific actions.
    /// </summary>
    public void Execute()
    {
        _logger?.Log("LuxImport executed", "Mods/LuxImport", LogLevel.Info);
        // Additional logic can be added here if needed
    }

    /// <summary>
    /// Cleans up resources and subscriptions when the module is shut down.
    /// </summary>
    public void Shutdown()
    {
        // Unsubscribe from events if necessary to avoid memory leaks
        _logger?.Log("LuxImport shutdown", "Mods/LuxImport", LogLevel.Info);
    }

    /// <summary>
    /// Asynchronous handler for the OpenCollectionEvent by importing the collection at the specified path.
    /// </summary>
    /// <param name="event">The OpenCollectionEvent containing the path to the collection.</param>
    public async Task HandleOnOpenCollectionAsync(OpenCollectionEvent @event)
    {
        _logger?.Log($"Importing collection [{@event.CollectionName}] at path: {@event.CollectionPath}", "Mods/LuxImport", LogLevel.Info);

        // Send a message back through the event tunnel
        SendProgressMessage(@event, "Initiating import process...");

        await Task.Delay(100);

        Stopwatch totalStopwatch = Stopwatch.StartNew();
        Stopwatch stepStopwatch = new Stopwatch();

        try
        {
            stepStopwatch.Start();

            // Initialize the import service with the collection name and path
            _logger?.Log("Initializing ImportService...", "Mods/LuxImport", LogLevel.Info);
            IImportService importService = new ImportService(@event.CollectionName, @event.CollectionPath);
            importService.ProgressMessageSent += (messageTuple) =>
            {
                SendProgressMessage(@event, messageTuple.message, messageTuple.progress);
            };

            stepStopwatch.Stop();
            _logger?.Log($"ImportService initialized in {stepStopwatch.ElapsedMilliseconds} ms", "Mods/LuxImport", LogLevel.Debug);

            stepStopwatch.Restart();

            // Check if the collection is already initialized
            SendProgressMessage(@event, "Checking collection initialization...");
            if (importService.IsInitialized())
            {
                SendProgressMessage(@event, "Collection is already initialized.", 10);
            }
            else
            {
                // Initializing collection's database
                SendProgressMessage(@event, "Initializing collection's database...", 20);
                importService.InitializeDatabase();
            }

            stepStopwatch.Stop();
            _logger?.Log($"Collection initialization checked in {stepStopwatch.ElapsedMilliseconds} ms", "Mods/LuxImport", LogLevel.Debug);

            stepStopwatch.Restart();

            // Update indexing files
            SendProgressMessage(@event, "Updating indexing files...", 25);
            importService.BaseProgressPercent = 25;
            await importService.IndexCollectionAsync();

            stepStopwatch.Stop();
            _logger?.Log($"Indexing completed in {stepStopwatch.ElapsedMilliseconds} ms", "Mods/LuxImport", LogLevel.Debug);

            stepStopwatch.Restart();

            SendProgressMessage(@event, "Loading in memory...");

            // Load assets into memory
            _logger?.Log("Loading assets into memory...", "Mods/LuxImport", LogLevel.Info);
            var assets = importService.LoadAssets();

            stepStopwatch.Stop();
            _logger?.Log($"Loaded {assets.Count} assets into memory in {stepStopwatch.ElapsedMilliseconds} ms", "Mods/LuxImport", LogLevel.Debug);

            SendProgressMessage(@event, "Assets loaded into memory.", 100);
            _eventBus?.Publish(new CollectionUpdatedEvent(@event.CollectionName, @event.CollectionPath, assets));

            // Mark the collection as imported
            SendProgressMessage(@event, "Collection imported successfully.", 100);
            @event.CompleteSuccessfully();
        }
        catch (Exception ex)
        {
            _logger?.Log($"Error importing collection: {ex.Message}", "Mods/LuxImport", LogLevel.Error);
            SendProgressMessage(@event, $"Error importing collection: {ex.Message}");
            @event.MarkAsFailed();
        }
        finally
        {
            totalStopwatch.Stop();
            _logger?.Log($"Import process completed in {totalStopwatch.ElapsedMilliseconds} ms", "Mods/LuxImport", LogLevel.Info);
        }
    }


    /// <summary>
    /// Sends a progress message to the logger and the event tunnel.
    /// </summary>
    private void SendProgressMessage(OpenCollectionEvent @event, string message, int? progress = null)
    {
        // Log the message
        _logger?.Log(message, "Mods/LuxImport", LogLevel.Info);
        // Send the message through the event tunnel
        if (progress.HasValue) @event.SendProgressMessage(message, progress.Value);
        else @event.SendProgressMessage(message);
    }
}
