// See https://aka.ms/new-console-template for more information

using LuxFilter.Algorithms.ColorVisualAesthetics;
using LuxFilter.Services;
using Luxoria.SDK.Models;
using Luxoria.SDK.Services;
using Luxoria.SDK.Services.Targets;
using SkiaSharp;

var loggerService = new LoggerService(LogLevel.Debug, new DebugLogTarget());

var pipeline = new PipelineService(loggerService);


pipeline.AddAlgorithm(new LuxFilter.Algorithms.ColorVisualAesthetics.SharpnessAlgo(), 1);

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

// Load an image from C:\Mac\Home\Downloads\NET Logo_resized copy.png
SKBitmap image = SKBitmap.Decode(@"C:\Mac\Home\Downloads\NET Logo_resized copy.png");

pipeline.Compute(image, image.Height, image.Width);