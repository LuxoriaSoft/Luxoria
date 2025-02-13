using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
