using Luxoria.GModules.Interfaces;
using System;
using System.Collections.Generic;

namespace Luxoria.GModules
{
    public class SmartButton : ISmartButton
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public Dictionary<SmartButtonType, Object> Pages { get; private set; }

        public SmartButton(string name, string description, Dictionary<SmartButtonType, Object> dic)
        {
            Name = name;
            Description = description;
            Pages = dic;
        }
    }
}
