using LuxEditor.EditorUI.Controls;
using LuxEditor.EditorUI.Interfaces;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SkiaSharp;
using System;

namespace LuxEditor.EditorUI.Groups
{
    public sealed class EditorToneCurveGroup : UserControl, IEditorGroupItem
    {
        private readonly ContentPresenter _presenter;
        public event Action<string, byte[]>? CurveChanged;

        /// <summary>
        /// Initialises UI and wires selection.
        /// </summary>
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

                new ColorChannelCurve("ToneCurve_Red",   SKColors.Red),
                new ColorChannelCurve("ToneCurve_Green", SKColors.Lime),
                new ColorChannelCurve("ToneCurve_Blue",  new SKColor(66, 140, 255))
            };

            foreach (var c in curves)
                c.CurveChanged += () => CurveChanged?.Invoke(c.SettingKey, c.GetLut());

            selector.SelectionChanged += i => _presenter.Content = curves[i];
            _presenter.Content = curves[0];

            Content = root;
        }

        public UIElement GetElement() => this;
    }
}
