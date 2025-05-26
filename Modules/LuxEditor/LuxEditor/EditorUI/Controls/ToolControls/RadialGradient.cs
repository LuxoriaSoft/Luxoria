using Microsoft.UI.Xaml.Controls;
using LuxEditor.Models;

namespace LuxEditor.Controls.ToolControls
{
    public class RadialGradientToolControl : UserControl
    {
        public RadialGradientToolControl(MaskOperation operation)
        {
            Content = new TextBlock
            {
                Text = "Radial Gradient Tool (WIP)",
                Padding = new Microsoft.UI.Xaml.Thickness(8)
            };
        }
    }
}
