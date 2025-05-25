using System;
using System.Collections.Generic;

namespace Luxoria.GModules.Interfaces
{
    public interface ISmartButton
    {
        string Name { get; }
        string Description { get; }
        Dictionary<SmartButtonType, Object> Pages { get; }
    }
}
