using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxoria.Modules.LMGUI;

public interface IGuiExtension
{
    IEnumerable<IGuiElement> GetGuiElements();
}
