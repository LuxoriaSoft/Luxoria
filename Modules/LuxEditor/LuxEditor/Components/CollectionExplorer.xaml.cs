using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using SkiaSharp;
using SkiaSharp.Views.Windows;
using System.IO;
using Windows.Storage.Streams;

namespace LuxEditor;

public sealed partial class CollectionExplorer : Page
{
    private Image _image;

    public CollectionExplorer()
    {
        this.InitializeComponent();
        InitializeSkiaCanvas();
    }

    private void DrawWithSkiaSharp()
    {
        /*
        int width = 500, height = 300;

        // Création du bitmap SkiaSharp
        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);

        // Dessiner du texte
        using var paint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            TextSize = 24
        };
        canvas.DrawText("SkiaSharp", width / 2, height / 2, paint);

        // Conversion en bitmap
        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = new MemoryStream();
        data.SaveTo(stream);
        stream.Seek(0, SeekOrigin.Begin);

        // Affichage dans le Canvas
        var bitmap = new BitmapImage();
        var randomStream = new InMemoryRandomAccessStream();
        stream.CopyTo(randomStream.AsStream());
        randomStream.Seek(0);

        bitmap.SetSource(randomStream);
        _image.Source = bitmap;
        */
        /*
        int width = 500, height = 300;
        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);

        // draw some text
        using var paint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };
        using var font = new SKFont
        {
            Size = 24
        };
        var coord = new SKPoint(e.Info.Width / 2, (e.Info.Height + font.Size) / 2);
        canvas.DrawText("SkiaSharp", coord, SKTextAlign.Center, font, paint);
        */


    }

    private void InitializeSkiaCanvas()
    {
        // Create SKXamlCanvas
        var skiaCanvas = new SKXamlCanvas
        {
            IgnorePixelScaling = true // Same as in XAML
        };

        // Attach PaintSurface event
        skiaCanvas.PaintSurface += OnPaintSurface;

        // Add to the main Grid
        Content = skiaCanvas;
    }

    private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        SKCanvas canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.White);

        using (SKPaint paint = new SKPaint())
        {
            paint.Color = SKColors.Red;
            paint.IsAntialias = true;
            paint.StrokeWidth = 5;
            canvas.DrawCircle(e.Info.Width / 2, e.Info.Height / 2, 50, paint);
        }
    }

    /*
    private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        // the the canvas and properties
        var canvas = e.Surface.Canvas;

        // make sure the canvas is blank
        canvas.Clear(SKColors.White);

        // draw some text
        using var paint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };
        using var font = new SKFont
        {
            Size = 24
        };
        var coord = new SKPoint(e.Info.Width / 2, (e.Info.Height + font.Size) / 2);
        canvas.DrawText("SkiaSharp", coord, SKTextAlign.Center, font, paint);
    }
    */
}