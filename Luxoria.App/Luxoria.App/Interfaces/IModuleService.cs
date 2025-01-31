using Luxoria.Modules.Interfaces;
using System.Collections.Generic;

namespace Luxoria.App.Interfaces
{
    public interface IModuleService
    {
        void AddModule(IModule module);

        void RemoveModule(IModule module);

        List<IModule> GetModules();

        void InitializeModules(IModuleContext context);
    }
}
