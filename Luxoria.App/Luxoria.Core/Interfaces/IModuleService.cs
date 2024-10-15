using Luxoria.Modules.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxoria.Core.Interfaces
{
    public interface IModuleService
    {
        void AddModule(IModule module);

        void RemoveModule(IModule module);

        List<IModule> GetModules();

        void InitializeModules(IModuleContext context);
    }
}
