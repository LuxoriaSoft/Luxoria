using Luxoria.GModules.Interfaces;
using Luxoria.Modules.Interfaces;
using Luxoria.SDK.Interfaces;
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

        _logger?.Log("LuxFilter module initialized.", CATEGORY);
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
