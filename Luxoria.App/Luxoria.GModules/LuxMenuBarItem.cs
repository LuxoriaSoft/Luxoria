using Luxoria.GModules.Interfaces;
using System;
using System.Collections.Generic;

namespace Luxoria.GModules
{
    public class LuxMenuBarItem : ILuxMenuBarItem
    {
        public string Name { get; set; }
        public bool IsLeftLocated { get; set; }
        public Guid ButtonId { get; set; }
        public List<ISmartButton> SmartButtons { get; set; }

        public LuxMenuBarItem(string name, bool isLeftLocated, Guid buttonId, List<ISmartButton> smartButtons)
        {
            Name = name;
            IsLeftLocated = isLeftLocated;
            ButtonId = buttonId;
            SmartButtons = smartButtons;
        }
    }
}
