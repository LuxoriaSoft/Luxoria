using LuxEditor.EditorUI.Controls;
using LuxEditor.EditorUI.Interfaces;
using LuxEditor.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LuxEditor.EditorUI.Groups
{
    public sealed class EditorToneCurveGroup : UserControl, IEditorGroupItem
    {
        private readonly ContentPresenter _presenter;
        private CurveBase[]? _curves;

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

            _curves = new CurveBase[]
            {
                new ParametricCurve(),

                new PointCurve(),

                new ColorChannelCurve("ToneCurve_Red",   SKColors.Red),
                new ColorChannelCurve("ToneCurve_Green", SKColors.Lime),
                new ColorChannelCurve("ToneCurve_Blue",  new SKColor(66, 140, 255))
            };

            foreach (var c in _curves)
                c.CurveChanged += () => CurveChanged?.Invoke(c.SettingKey, c.GetLut());

            selector.SelectionChanged += i => _presenter.Content = _curves[i];
            _presenter.Content = _curves[0];

            Content = root;
        }

        public UIElement GetElement() => this;

        public void RefreshCurves(Dictionary<string, object> settings)
        {
            Debug.WriteLine("Refreshing tone curves...");
            if (_curves == null)
                return;
            for (int i = 0; i < _curves.Length; i++)
            {
                var curve = _curves[i];
                curve.RefreshCurve(settings);
            }
        }
    }
}
