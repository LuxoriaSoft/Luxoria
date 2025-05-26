using LuxEditor.Logic;
using LuxEditor.Models;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using SkiaSharp.Views.Windows;
using SkiaSharp;
using System;
using System.Linq;
using LuxEditor.Controls.ToolControls;

public class LayerDetailsPanel : UserControl
{
    private Button _addOpButton;
    private Button _removeOpButton;
    private ListBox _opsList;
    private SKXamlCanvas _opsPreview;
    private readonly ContentControl _toolHost;

    public LayerDetailsPanel()
    {
        BuildUI();
        LayerManager.Instance.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(LayerManager.SelectedLayer))
                UpdateUI();
        };
        UpdateUI();
    }

    private void BuildUI()
    {
        var root = new StackPanel { Orientation = Orientation.Vertical, Spacing = 12, Padding = new Thickness(8) };

        _opsPreview = new SKXamlCanvas { Height = 80, Margin = new Thickness(0, 8, 0, 8) };
        _opsPreview.PaintSurface += (s, e) => DrawCombinedMask(e.Surface.Canvas);
        root.Children.Add(_opsPreview);

        var opBtnLine = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4 };
        _addOpButton = new Button { Content = "+", Width = 24, Height = 24 };
        _removeOpButton = new Button { Content = "–", Width = 24, Height = 24 };
        _addOpButton.Click += (_, _) => ShowAddOpFlyout();
        _removeOpButton.Click += (_, _) => RemoveSelectedOp();
        opBtnLine.Children.Add(_addOpButton);
        opBtnLine.Children.Add(_removeOpButton);
        root.Children.Add(opBtnLine);

        _opsList = new ListBox
        {
            Height = 120,
            SelectionMode = SelectionMode.Single
        };
        _opsList.SelectionChanged += (_, _) => OnOpSelected();
        root.Children.Add(_opsList);

        var opDetailHost = new ContentControl();
        root.Children.Add(opDetailHost);

        Content = root;
    }

    private void UpdateUI()
    {
        var layer = LayerManager.Instance.SelectedLayer;
        if (layer == null) { Visibility = Visibility.Collapsed; return; }
        Visibility = Visibility.Visible;

        _opsList.ItemsSource = layer.Operations;
        _removeOpButton.IsEnabled = _opsList.SelectedItem != null;

        _opsPreview.Invalidate();

        OnOpSelected();
    }

    private void ShowAddOpFlyout()
    {
        var fly = new MenuFlyout();
        foreach (BrushType t in Enum.GetValues(typeof(BrushType)))
        {
            var item = new MenuFlyoutItem { Text = t.ToString(), Tag = t };
            item.Click += (s, e) =>
            {
                var layer = LayerManager.Instance.SelectedLayer!;
                var op = new MaskOperation((BrushType)((MenuFlyoutItem)s).Tag);
                layer.Operations.Add(op);
                _opsList.SelectedItem = op;
            };
            fly.Items.Add(item);
        }
        fly.ShowAt(_addOpButton);
    }

    private void RemoveSelectedOp()
    {
        var layer = LayerManager.Instance.SelectedLayer!;
        if (_opsList.SelectedItem is MaskOperation op)
            layer.Operations.Remove(op);
    }


    private void OnOpSelected()
    {
        if (!(_opsList.SelectedItem is MaskOperation op))
        {
            _toolHost.Content = null;
            return;
        }

        _toolHost.Content = op.BrushType switch
        {
            BrushType.Brush => new BrushToolControl(op),
            BrushType.LinearGradient => new LinearGradientToolControl(op),
            BrushType.RadialGradient => new RadialGradientToolControl(op),
            BrushType.ColorRange => new ColorRangeToolControl(op),
            _ => null
        };
    }


    private void DrawCombinedMask(SKCanvas canvas)
    {
        var layer = LayerManager.Instance.SelectedLayer;
        if (layer == null) return;

        canvas.Clear();

        foreach (var op in layer.Operations)
        {
            foreach (var st in op.Strokes)
            {
                using var paint = new SKPaint { Style = SKPaintStyle.Fill };
                if (op.Mode == StrokeMode.Subtract)
                    paint.BlendMode = SKBlendMode.Clear;
                canvas.DrawPath(st.Path, paint);
            }
        }
    }
}
