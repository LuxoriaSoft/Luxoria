using Luxoria.App;
using Luxoria.Modules.LMGUI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Luxoria.Core.LMGUI
{
    public class GuiRenderer
    {
        private readonly MainWindow _mainWindow;

        public GuiRenderer(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public void Render(IEnumerable<IGuiElement> elements)
        {
            Debug.WriteLine("Rendering GUI elements");
            foreach (var element in elements)
            {
                Debug.WriteLine($"Rendering element {element.Identifier} to {element.TargetRegion}");
                var targetPanel = GetTargetPanel(element.TargetRegion);
                if (targetPanel != null)
                {
                    Debug.WriteLine($"Rendering element {element.Identifier} to {element.TargetRegion}");
                    var uiElement = CreateUiElement(element);
                    targetPanel.Children.Add(uiElement);
                }
            }
        }

        private Panel? GetTargetPanel(string targetRegion)
        {
            return targetRegion switch
            {
                "MainMenu" => _mainWindow.MainView,
                nameof(GuiRegions.Toolbar) => _mainWindow.Toolbar,
                nameof(GuiRegions.Sidebar) => _mainWindow.Sidebar,
                nameof(GuiRegions.Footer) => _mainWindow.Footer,
                _ => null
            };
        }

        private UIElement CreateUiElement(IGuiElement element)
        {
            Debug.WriteLine($"Creating UI element: {element.ElementType}");

            return element.ElementType switch
            {
                "Button" => new Button { Content = element.Properties["Label"] },
                "TextBox" => new TextBox { PlaceholderText = (string)element.Properties["Placeholder"] },
                _ => throw new NotImplementedException($"Unknown element type: {element.ElementType}")
            };
        }
    }

}
