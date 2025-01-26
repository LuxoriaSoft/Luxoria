using Luxoria.Modules.Interfaces;
using System.Collections.Generic;

namespace Luxoria.GModules.Interfaces;

public interface IModuleUI : IModule
{
    List<ILuxMenuBarItem> Items { get; set; }
}
