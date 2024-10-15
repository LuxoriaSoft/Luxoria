using Luxoria.Core.Interfaces;
using Luxoria.Modules.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxoria.Core.Services
{
    public class ModuleService : IModuleService
    {
        private List<IModule> _modules = new List<IModule>();
        private IEventBus _eventBus;

        public ModuleService(IEventBus eventBus)
        {
            _eventBus = eventBus;
            // Load modules
        }

        public void AddModule(IModule module)
        {
            _modules.Add(module);
        }

        public void RemoveModule(IModule module)
        {
            _modules.Remove(module);
        }

        public List<IModule> GetModules() => _modules;

        public void InitializeModules(IModuleContext context)
        {
            foreach (IModule module in _modules)
            {
                module.Initialize(_eventBus, context);
            }
        }
    }
}
