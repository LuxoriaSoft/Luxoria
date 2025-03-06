using LuxFilter.Interfaces;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace LuxFilter.Views
{
    public sealed partial class StatusView : Page
    {
        private readonly IEventBus _eventBus;
        private readonly MainFilterView _parent;
        private readonly IPipelineService _pipeline;

        // Observable collection for storing score data
        public ObservableCollection<ScoreItem> ScoreList { get; } = [];

        // Log collection to track the progress
        public ObservableCollection<string> LogEntries { get; } = [];

        public StatusView(IEventBus eventBus, MainFilterView parent, IPipelineService pipeline, IEnumerable<LuxAsset> collection)
        {
            _eventBus = eventBus;
            _parent = parent;
            _pipeline = pipeline;

            // Initialize the component and set width
            Width = 800;
            this.InitializeComponent();

            // Attach pipeline event handlers
            _pipeline.OnScoreComputed += OnScoreComputedEvent;
            _pipeline.OnPipelineFinished += OnPipelineCompletedEvent;

            // Bind score list to UI
            LogViewer.ItemsSource = LogEntries;

            // Start pipeline execution
            Debug.WriteLine($"Starting pipeline with {collection.Count()} assets");
            StartPipeline(collection.Select(asset => (asset.Id, asset.Data)).ToList());
        }

        private void OnScoreComputedEvent(object sender, (Guid, double) args)
        {
            var (imageId, score) = args;

            // Update the log and progress
            _ = DispatcherQueue.TryEnqueue(() =>
            {
                // Find the item in the list and update its status
                var item = ScoreList.FirstOrDefault(x => x.ImageId == imageId);
                if (item != null)
                {
                    item.Status = $"Score: {score}";
                    item.Score = score.ToString("F2");
                }

                // Log the computed score
                LogEntries.Add($"Score [{imageId}]: {score:F2}");
            });
        }

        private void OnPipelineCompletedEvent(object sender, TimeSpan duration)
        {
            _ = DispatcherQueue.TryEnqueue(() =>
            {
                LogEntries.Add($"Pipeline completed in {duration.TotalSeconds:F2} seconds.");
                StatusMessage.Text = $"Pipeline Completed in {duration.TotalSeconds:F2} sec";
                ProgressIndicator.IsActive = false;
            });
        }

        private async void StartPipeline(IEnumerable<(Guid, ImageData)> images)
        {
            StatusMessage.Text = "Processing...";
            ProgressIndicator.IsActive = true;
            LogEntries.Clear();  // Clear any previous logs
            ScoreList.Clear();

            // Execute the pipeline
            await _pipeline.Compute(images);
        }
    }

    /// <summary>
    /// Score item for displaying image scores
    /// </summary>
    public class ScoreItem
    {
        public Guid ImageId { get; set; }
        public string Status { get; set; }
        public string Score { get; set; }
    }
}
