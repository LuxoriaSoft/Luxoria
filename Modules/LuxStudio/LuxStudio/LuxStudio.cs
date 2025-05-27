using Luxoria.GModules;
using Luxoria.GModules.Interfaces;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models.Events;
using Luxoria.SDK.Interfaces;
using Luxoria.SDK.Models;
using LuxStudio.Components;
using System.Diagnostics;
using LuxStudio.COM;

namespace LuxStudio;

public class LuxStudio : IModule, IModuleUI
{
    private IEventBus? _eventBus;
    private IModuleContext? _context;
    private ILoggerService? _logger;

    public string Name => "LuxStudio";
    public string Description => "Generic Luxoria LuxStudio Integration Module";
    public string Version => "1.0.0";

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

        // Add a menu bar item to the main menu bar
        List<ISmartButton> smartButtons = [];
        Dictionary<SmartButtonType, Object> accountMgt = new()
        {
            { SmartButtonType.Modal, new AccManagementView() }
        };
        Dictionary<SmartButtonType, Object> collectionMngt = new()
        {
            { SmartButtonType.Modal, new CollectionManagementView() }
        };

        smartButtons.Add(new SmartButton("Account", "Account Management", accountMgt));
        smartButtons.Add(new SmartButton("Collections", "View Collections", collectionMngt));
        Items.Add(new LuxMenuBarItem("LuxStudio", false, new Guid(), smartButtons));
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
}
