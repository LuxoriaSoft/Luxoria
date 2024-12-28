﻿// See https://aka.ms/new-console-template for more information

using LuxFilter.Services;
using Luxoria.SDK.Models;
using Luxoria.SDK.Services;
using Luxoria.SDK.Services.Targets;
using SkiaSharp;

var loggerService = new LoggerService(LogLevel.Debug, new DebugLogTarget());

var pipeline = new PipelineService(loggerService);


pipeline.AddAlgorithm(new LuxFilter.Algorithms.ImageQuality.SharpnessAlgo(), 0.75);
pipeline.AddAlgorithm(new LuxFilter.Algorithms.ImageQuality.ResolutionAlgo(), 0.25);

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
SKBitmap image = SKBitmap.Decode(@"C:\Mac\Home\Downloads\landscape_4k.jpg");
SKBitmap image2 = SKBitmap.Decode(@"C:\Mac\Home\Downloads\EditorView2.png");

var fscore = pipeline.Compute(image, image.Height, image.Width);
var fscore2 = pipeline.Compute(image2, image2.Height, image2.Width);
loggerService.Log($"Final score: {fscore}");
loggerService.Log($"Final score: {fscore2}");