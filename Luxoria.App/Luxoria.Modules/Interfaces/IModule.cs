

namespace Luxoria.Modules.Interfaces;

public interface IModule
{
    string Name { get; }
    string Description { get; }
    string Version { get; }
    void Initialize(IEventBus eventBus, IModuleContext context);
    void Execute();
    void Shutdown();
}
