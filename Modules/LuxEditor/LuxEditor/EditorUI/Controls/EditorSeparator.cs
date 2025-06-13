using LuxEditor.EditorUI.Interfaces;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;

namespace LuxEditor.EditorUI.Controls
{
    public class EditorSeparator : IEditorGroupItem
    {
        private readonly UIElement _element;

        /// <summary>
        /// Creates a new separator for the editor UI.
        /// </summary>
        /// <param name="marginTop"></param>
        /// <param name="marginBottom"></param>
        public EditorSeparator(double marginTop = 10, double marginBottom = 10)
        {
            _element = new Rectangle
            {
                Height = 1,
                Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 100, 100, 100)),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(5, marginTop, 5, marginBottom)
            };
        }

        /// <summary>
        /// Gets the UI element for this separator.
        /// </summary>
        /// <returns></returns>
        public UIElement GetElement() => _element;
    }
}
