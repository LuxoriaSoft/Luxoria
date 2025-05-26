using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using LuxEditor.Models;

namespace LuxEditor.Controls
{
    public class LayerTreeTemplateSelector : DataTemplateSelector
    {
        public DataTemplate LayerTemplate { get; set; }

        public DataTemplate OperationTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is Layer)
                return LayerTemplate;
            if (item is MaskOperation)
                return OperationTemplate;
            return base.SelectTemplateCore(item);
        }
    }
}
