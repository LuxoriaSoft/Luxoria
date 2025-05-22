using LuxFilter.Services;
using LuxFilter.Views;
using Luxoria.GModules;
using Luxoria.GModules.Interfaces;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models;
using Luxoria.Modules.Models.Events;
using Luxoria.SDK.Interfaces;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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

    private ICollection<LuxAsset> _lastImportedCollection
    {
        set
        {
            _mainFilterView?.SetCollection(value);
        }
    }

    private MainFilterView _mainFilterView;

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

        // Initialize the module UI
        _mainFilterView = new MainFilterView(_eventBus, _logger);

        // Add a menu bar item to the main menu bar.
        List<ISmartButton> smartButtons = [];
        Dictionary<SmartButtonType, Object> page = new()
        {
            { SmartButtonType.Modal, _mainFilterView }
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
            _lastImportedCollection = e.Assets;
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
