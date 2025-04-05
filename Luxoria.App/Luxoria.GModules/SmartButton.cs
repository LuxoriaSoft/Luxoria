using Luxoria.GModules.Interfaces;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
