using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxoria.GModules.Interfaces
{
    public interface ISmartButton
    {
        string Name { get; }
        string Description { get; }
        Dictionary<SmartButtonType, Object> Pages { get; }
    }
}
