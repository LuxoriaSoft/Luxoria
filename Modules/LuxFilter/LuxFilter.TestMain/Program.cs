// See https://aka.ms/new-console-template for more information

using LuxFilter.Services;
using Luxoria.Modules.Models;
using Luxoria.SDK.Models;
using Luxoria.SDK.Services;
using Luxoria.SDK.Services.Targets;
using SkiaSharp;

var loggerService = new LoggerService(LogLevel.Debug, new DebugLogTarget());

var pipeline = new PipelineService(loggerService);


pipeline
    .AddAlgorithm(new LuxFilter.Algorithms.ImageQuality.SharpnessAlgo(), 0.75)
    .AddAlgorithm(new LuxFilter.Algorithms.ImageQuality.ResolutionAlgo(), 0.15)
    .AddAlgorithm(new LuxFilter.Algorithms.ColorVisualAesthetics.CLIPAlgo(), 0.15)
    .AddAlgorithm(new LuxFilter.Algorithms.PerceptualMetrics.BrisqueAlgo(), 0.1);

// Get the root directory of the application
string? baseDirectory = Directory.GetParent(AppContext.BaseDirectory)?.Parent?.Parent?.Parent?.Parent?.Parent?.FullName;

// Ensure the base directory is not null
if (string.IsNullOrEmpty(baseDirectory))
{
    throw new InvalidOperationException("Unable to determine the base directory of the application.");
}

loggerService.Log($"Base directory: {baseDirectory}");

// Decode the image file into a SKBitmap (in assets folder)
/*
 * - landscape_4k.jpg
 * - landscape_bad_quality.jpeg
 * - net_logo.png
 */
loggerService.Log("Decoding images...");

loggerService.Log("Decoding landscape_4k.jpg...");
SKBitmap image = SKBitmap.Decode(Path.Combine(baseDirectory, "assets", "landscape_4k.jpg"));
SKBitmap image4 = SKBitmap.Decode(Path.Combine(baseDirectory, "assets", "landscape_4k.jpg"));
loggerService.Log("Decoding landscape_bad_quality.jpeg...");
SKBitmap image2 = SKBitmap.Decode(Path.Combine(baseDirectory, "assets", "landscape_bad_quality.jpeg"));
loggerService.Log("Decoding net_logo.png...");
SKBitmap image3 = SKBitmap.Decode(Path.Combine(baseDirectory, "assets", "net_logo.png"));

// Attach handlers to the pipeline
pipeline.OnScoreComputed += (sender, args) =>
{
    loggerService.Log($"Score computed for image {args.Item1}: {args.Item2}");
};

pipeline.OnPipelineFinished += (sender, args) =>
{
    loggerService.Log("Pipeline finished time consumed: " + args);
};

loggerService.Log("Computing scores...");

// Compute scores for the collection of bitmaps
var scores = await pipeline.Compute(
[
    (Guid.NewGuid(), new(image, FileExtension.UNKNOWN)),
    (Guid.NewGuid(), new(image2, FileExtension.UNKNOWN)),
    (Guid.NewGuid(), new (image3, FileExtension.UNKNOWN)),
    (Guid.NewGuid(), new (image4, FileExtension.UNKNOWN))
]);

loggerService.Log("Scores computed !");

int index = 1;
foreach (var finalScore in scores)
{
    loggerService.Log($"Image {index++} ({finalScore.Key}):");
    foreach (var score in finalScore.Value)
    {
        loggerService.Log($"  {score.Key}: {score.Value}");
    }
}
