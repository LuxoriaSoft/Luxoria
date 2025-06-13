using Luxoria.GModules;
using Luxoria.Modules.Interfaces;
using System;

namespace Luxoria.GraphicalModules.Models.Events;

public class OpenEvent: IEvent
{
    SmartButtonType type { get; set; }
    Object content { get; set; }
}
