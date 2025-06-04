using LuxFilter.Components;
using LuxFilter.Services;
using Luxoria.GModules;
using Luxoria.GModules.Interfaces;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models;
using Luxoria.Modules.Models.Events;
using Luxoria.SDK.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LuxFilter;

public class LuxFilter : IModule, IModuleUI
{
    private IEventBus _eventBus;
    private IModuleContext _context;
    private ILoggerService _logger;

    public string Name => "LuxFilter";
    public string Description => "Generic Luxoria Filtering Module";
    public string Version => "1.0.2";

    private const string CATEGORY = nameof(LuxFilter);

    /// <summary>
    /// The list of menu bar items to be added to the main menu bar.
    /// </summary>
    public List<ILuxMenuBarItem> Items { get; set; } = [];

    private ICollection<LuxAsset> _lastImportedCollection = [];
    private string _lastImportedCName = string.Empty;
    private string _lastImportedCPath = string.Empty;

    private CollectionExplorer? _cExplorer;
    private AssetViewer? _viewer;
    private ToolBox? _toolbox;
    private FilterToolBox? _filterToolBox;

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

        // Attach events
        AttachEventHandlers();

        _cExplorer = new CollectionExplorer();
        _viewer = new AssetViewer();
        _toolbox = new ToolBox();
        _filterToolBox = new FilterToolBox(_eventBus, _logger);

        _cExplorer.OnImageSelected += (asset) =>
        {
            _viewer?.SetImage(asset.Data.Bitmap);
            _toolbox?.SetSelectedAsset(asset);
            _logger.Log($"Image selected: {asset.Id}");
        };

        _toolbox.OnRatingChanged += (asset) =>
        {
            _logger.Log($"Rating changed for asset {asset.Id}: {asset.FilterData.Rating}");
            var existingAsset = _lastImportedCollection.FirstOrDefault(a => a.Id == asset.Id);
            if (existingAsset != null)
            {
                existingAsset.FilterData.Rating = asset.FilterData.Rating;
            }
        };

        _toolbox.OnFlagUpdated += (asset) =>
        {
            _logger.Log($"Flag updated for asset {asset.Id}: ");
            var existingAsset = _lastImportedCollection.FirstOrDefault(a => a.Id == asset.Id);
            if (existingAsset != null)
            {
                existingAsset.FilterData.SetFlag(asset.FilterData.GetFlag());
            }
        };

        _filterToolBox.OnScoreUpdated += (score) =>
        {
            _logger.Log($"FINAL Score updated for asset {score.Item1}/{score.Item2}: {score.Item3}");
            var existingAsset = _lastImportedCollection.FirstOrDefault(a => a.Id == score.Item2);
            if (existingAsset != null)
            {
                existingAsset.FilterData.SetScore(score.Item1, score.Item3);
            }
        };

        _filterToolBox.OnSaveClicked += async () =>
        {
            _logger.Log("FilterToolBox save clicked, saving collection...");

            await _eventBus.Publish(new CollectionUpdatedEvent(_lastImportedCName, _lastImportedCPath, _lastImportedCollection));
            _logger.Log("Collection updated event published.");
        };

        // Add a menu bar item to the main menu bar.
        List<ISmartButton> smartButtons = [];
        Dictionary<SmartButtonType, Object> page = new()
        {
            { SmartButtonType.BottomPanel, _cExplorer },
            { SmartButtonType.MainPanel, _viewer },
            { SmartButtonType.RightPanel, _toolbox },
            { SmartButtonType.LeftPanel, _filterToolBox }
        };

        smartButtons.Add(new SmartButton("Filter", "Filter", page));
        Items.Add(new LuxMenuBarItem("Filter", false, new Guid(), smartButtons));

        _logger?.Log("LuxFilter module initialized.", CATEGORY);
    }

    /// <summary>
    /// Attaches event handlers to the EventBus.
    /// </summary>
    private void AttachEventHandlers()
    {
        // Gather the filter catalog
        _eventBus.Subscribe<FilterCatalogEvent>(e =>
        {
            e.Response.SetResult([.. FilterService.Catalog.Select(x => (x.Key, x.Value.Description, "1.0"))]);
        });

        // Update the last imported collection for LuxFilter
        _eventBus.Subscribe<CollectionUpdatedEvent>(e =>
        {
            _logger.Log($"LuxFilter received {e.Assets.Count} assets.");
            _lastImportedCName = e.CollectionName;
            _lastImportedCPath = e.CollectionPath;
            _lastImportedCollection = e.Assets;

            _cExplorer?.SetImages(e.Assets);
            _filterToolBox?.SetImages(e.Assets);
        });
    }

    /// <summary>
    /// Executes the module logic.
    /// </summary>
    public void Execute()
    {
        _logger?.Log("LuxFilter module executed.", CATEGORY);
    }

    /// <summary>
    /// Shuts down the module and releases any resources.
    /// </summary>
    public void Shutdown()
    {
        _logger?.Log("LuxFilter module shutdown.", CATEGORY);
    }
}
