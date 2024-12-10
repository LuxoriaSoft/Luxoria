using Luxoria.Core.LMGUI;
using Luxoria.Modules.Interfaces;
using System.Collections.Generic;

namespace Luxoria.App.Core.Interfaces
{
    public interface IModuleService
    {
        void AddModule(IModule module);

        void RemoveModule(IModule module);

        List<IModule> GetModules();

        void InitializeModules(GuiRenderer guiRenderer, IModuleContext context);
    }
}
