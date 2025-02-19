using LuxFilter.Views;
using Luxoria.GModules;
using Luxoria.GModules.Interfaces;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models.Events;
using Luxoria.SDK.Interfaces;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;

namespace LuxFilter;

public class LuxFilter : IModule, IModuleUI
{
    private IEventBus _eventBus;
    private IModuleContext _context;
    private ILoggerService _logger;

    public string Name => "LuxFilter";
    public string Description => "Generic Luxoria Filtering Module";
    public string Version => "1.0.0";

    private const string CATEGORY = nameof(LuxFilter);

    /// <summary>
    /// The list of menu bar items to be added to the main menu bar.
    /// </summary>
    public List<ILuxMenuBarItem> Items { get; set; } = [];

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

        // Add a menu bar item to the main menu bar.
        List<ISmartButton> smartButtons = [];
        Dictionary<SmartButtonType, Page> page = new()
        {
            { SmartButtonType.Modal, new MainFilterView(_eventBus) }
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
        _eventBus.Subscribe<FilterCatalogEvent>(e =>
        {
            var filters = new List<(string Name, string Description, string Version)>
            {
                ("Brisque", "Assesses image quality based on natural scene statistics", "1.0"),
                ("Sharpness", "Measures the clarity and detail of the image", "1.2"),
                ("Contrast", "Analyzes the differences in light and dark areas", "1.1"),
                ("Brightness", "Adjusts the overall lightness of the image", "1.3"),
                ("Color Balance", "Corrects color imbalances in an image", "1.4")
            };

            e.Response.SetResult(filters); // Send the filters back
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
