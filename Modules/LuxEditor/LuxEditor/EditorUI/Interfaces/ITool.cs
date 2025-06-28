using Microsoft.UI.Xaml.Input;
using SkiaSharp.Views.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxEditor.EditorUI.Interfaces
{
    internal interface ITool
    {
        void OnPointerMoved(object sender, PointerRoutedEventArgs e);
        void OnPointerPressed(object sender, PointerRoutedEventArgs e);
        void OnPointerReleased(object sender, PointerRoutedEventArgs e);
        void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e);
    }
}
