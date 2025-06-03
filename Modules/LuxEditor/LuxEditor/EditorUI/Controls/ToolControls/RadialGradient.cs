//using Microsoft.UI.Xaml.Controls;
//using LuxEditor.Models;
//using LuxEditor.EditorUI.Controls.ToolControls;
//using SkiaSharp.Views.Windows;
//using Microsoft.UI.Xaml.Input;
//using SkiaSharp;
//using System.Collections.ObjectModel;

//namespace LuxEditor.Controls.ToolControls
//{
//    public class RadialGradientToolControl : ATool
//    {
//        private ToolType _toolType = ToolType.RadialGradient;
//        public override ToolType ToolType
//        {
//            get => _toolType;
//            set => _toolType = value;
//        }

//        public override SKXamlCanvas Canvas { get; set; } = new SKXamlCanvas();
//        public override SKPath? CurrentPath { get; set; } = null;
//        public ObservableCollection<Stroke> Strokes { get; } = new ObservableCollection<Stroke>();

//        public RadialGradientToolControl() : base()
//        {
//            Content = new TextBlock
//            {
//                Text = "Radial Gradient Tool (WIP)",
//                Padding = new Microsoft.UI.Xaml.Thickness(8)
//            };
//        }

//        public override void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
//        {
//            throw new System.NotImplementedException();
//        }

//        public override void OnPointerMoved(object sender, PointerRoutedEventArgs e)
//        {
//            throw new System.NotImplementedException();
//        }

//        public override void OnPointerPressed(object sender, PointerRoutedEventArgs e)
//        {
//            throw new System.NotImplementedException();
//        }

//        public override void OnPointerReleased(object sender, PointerRoutedEventArgs e)
//        {
//            throw new System.NotImplementedException();
//        }
//    }
//}
