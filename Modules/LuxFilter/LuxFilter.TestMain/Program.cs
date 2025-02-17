// See https://aka.ms/new-console-template for more information

using LuxFilter.Services;
using Luxoria.SDK.Models;
using Luxoria.SDK.Services;
using Luxoria.SDK.Services.Targets;
using SkiaSharp;

var loggerService = new LoggerService(LogLevel.Debug, new DebugLogTarget());

var pipeline = new PipelineService(loggerService);


pipeline
    .AddAlgorithm(new LuxFilter.Algorithms.ImageQuality.SharpnessAlgo(), 0.75)
    .AddAlgorithm(new LuxFilter.Algorithms.ImageQuality.ResolutionAlgo(), 0.15)
    .AddAlgorithm(new LuxFilter.Algorithms.PerceptualMetrics.BrisqueAlgo(), 0.1);

// Get the root directory of the application
string? baseDirectory = Directory.GetParent(AppContext.BaseDirectory)?.Parent?.Parent?.Parent?.Parent?.FullName;

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

// Compute scores for the collection of bitmaps
IEnumerable<(Guid, Dictionary<string, double>)> scores = await pipeline.Compute(
[
    (Guid.NewGuid(), image),
    (Guid.NewGuid(), image2),
    (Guid.NewGuid(), image3)
]);

int index = 1;
foreach (var finalScore in scores)
{
    // Log the final score for each bitmap, including both the Guid and the score
    loggerService.Log($"Final score for image {index++} (Guid: {finalScore.Item1}): {finalScore.Item2.Keys.Count} key(s)");
    // Display the dictionary of scores for each bitmap
    foreach (var score in finalScore.Item2)
    {
        loggerService.Log($"[+]\t{score.Key}\t:\t{score.Value}");
    }
}


var algo = new LuxFilter.Algorithms.PerceptualMetrics.BrisqueAlgo();
loggerService.Log($"Brisque score for image 1: {algo.Compute(image, image.Height, image.Width)}");
loggerService.Log($"Brisque score for image 2: {algo.Compute(image2, image.Height, image.Width)}");
loggerService.Log($"Brisque score for image 3: {algo.Compute(image3, image.Height, image.Width)}");