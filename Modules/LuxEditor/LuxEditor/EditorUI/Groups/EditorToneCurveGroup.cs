using LuxEditor.EditorUI.Controls;
using LuxEditor.EditorUI.Interfaces;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SkiaSharp;

namespace LuxEditor.EditorUI.Groups
{
    /// <summary>Selector bar + hosted curve view.</summary>
    public sealed class EditorToneCurveGroup : UserControl, IEditorGroupItem
    {
        private readonly ContentPresenter _presenter;

        /// <summary>Initialises UI and wires selection.</summary>
        public EditorToneCurveGroup()
        {
            var root = new StackPanel { Spacing = 12 };

            var selector = new CurveSelectorBar();
            _presenter = new ContentPresenter();

            root.Children.Add(selector);
            root.Children.Add(_presenter);

            var curves = new CurveBase[]
            {
                new ParametricCurve(),
                new PointCurve(),
                new ColorChannelCurve(SKColors.Red),
                new ColorChannelCurve(SKColors.Green),
                new ColorChannelCurve(SKColors.Blue)
            };

            selector.SelectionChanged += i => _presenter.Content = curves[i];
            _presenter.Content = curves[0];

            Content = root;
        }

        public UIElement GetElement() => this;
    }
}
