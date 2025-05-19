using System;
using System.Collections.Generic;

namespace Luxoria.GModules.Interfaces
{
    public interface ILuxMenuBarItem
    {
        string Name { get; set; }
        bool IsLeftLocated { get; set; }
        Guid ButtonId { get; set; }
        List<ISmartButton> SmartButtons { get; set; }
    }
}
