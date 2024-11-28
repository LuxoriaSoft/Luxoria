using Luxoria.Modules.Interfaces;
using Luxoria.SDK.Interfaces;

namespace LuxFilter;

public class LuxFilter : IModule
{
    private IEventBus? _eventBus;
    private IModuleContext? _context;
    private ILoggerService? _logger;

    public string Name => "LuxFilter";
    public string Description => "Generic Luxoria Filtering Module";
    public string Version => "1.0.0";

    private const string CATEGORY = nameof(LuxFilter);

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

    public void Execute()
    {
        throw new NotImplementedException();
    }

    public void Shutdown()
    {
        throw new NotImplementedException();
    }
}
