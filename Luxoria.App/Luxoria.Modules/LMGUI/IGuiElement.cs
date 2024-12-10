using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxoria.Modules.LMGUI;

public interface IGuiElement
{
    string ElementType { get; }
    string Identifier { get; }
    string TargetRegion { get; }
    Dictionary<string, object> Properties { get; }
    Action? OnEvent { get; }
}
