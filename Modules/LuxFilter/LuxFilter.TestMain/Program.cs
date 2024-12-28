// See https://aka.ms/new-console-template for more information

using LuxFilter.Interfaces;
using LuxFilter.Services;
using Luxoria.SDK.Models;
using Luxoria.SDK.Services;
using Luxoria.SDK.Services.Targets;
using SkiaSharp;

var loggerService = new LoggerService(LogLevel.Debug, new DebugLogTarget());

var pipeline = new PipelineService(loggerService);


pipeline.AddAlgorithm(new LuxFilter.Algorithms.ImageQuality.SharpnessAlgo(), 0.85);
pipeline.AddAlgorithm(new LuxFilter.Algorithms.ImageQuality.ResolutionAlgo(), 0.15);

// Initialize a 100x100 SKBitmap
var bitmap = new SKBitmap(100, 100);

//pipeline.Compute(bitmap, bitmap.Height, bitmap.Width);


/*
var pipe2 = new PipelineService(loggerService);

pipe2.AddAlgorithm(new LuxFilter.Algorithms.ColorVisualAesthetics.SharpnessAlgo(), 0.5);
pipe2.AddAlgorithm(new LuxFilter.Algorithms.ColorVisualAesthetics.SharpnessAlgo(), 0.25);
pipe2.AddAlgorithm(new LuxFilter.Algorithms.ColorVisualAesthetics.SharpnessAlgo(), 0.25);

pipe2.Compute(bitmap, bitmap.Height, bitmap.Width);
*/

// Load the images
SKBitmap image = SKBitmap.Decode(@"C:\Mac\Home\Downloads\landscape_4k.jpg");
SKBitmap image2 = SKBitmap.Decode(@"C:\Mac\Home\Downloads\EditorView2.png");
SKBitmap image3 = SKBitmap.Decode(@"C:\Mac\Home\Downloads\EditorView2.png");
SKBitmap image4 = SKBitmap.Decode(@"C:\Mac\Home\Downloads\sketch.png");
SKBitmap image5 = SKBitmap.Decode(@"C:\Mac\Home\Downloads\Luxoria 1000x1000.png");

// Create a collection of BitmapWithSize objects
List<BitmapWithSize> bitmapsWithSizes = new List<BitmapWithSize>
{
    new BitmapWithSize(image),
    new BitmapWithSize(image2),
    new BitmapWithSize(image3),
    new BitmapWithSize(image4),
    new BitmapWithSize(image5)
};

// Compute scores for the collection of bitmaps
var bitmapScores = await pipeline.Compute(bitmapsWithSizes);

int index = 1;
foreach (var finalScore in bitmapScores)
{
    // Log the final score for each bitmap
    loggerService.Log($"Final score for image {index}: {finalScore}");
    index++;
}

