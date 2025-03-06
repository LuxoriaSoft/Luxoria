using LuxFilter.Interfaces;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;  // Add this for INotifyPropertyChanged

namespace LuxFilter.Views
{
    public sealed partial class StatusView : Page
    {
        private readonly IEventBus _eventBus;
        private readonly MainFilterView _parent;
        private readonly IPipelineService _pipeline;

        // Observable collection for storing score data
        public ObservableCollection<ScoreItem> ScoreList { get; } = new ObservableCollection<ScoreItem>();

        public StatusView(IEventBus eventBus, MainFilterView parent, IPipelineService pipeline, IEnumerable<LuxAsset> collection)
        {
            _eventBus = eventBus;
            _parent = parent;
            _pipeline = pipeline;

            this.InitializeComponent();

            // Set the DataContext for binding to the current instance of StatusView
            this.DataContext = this;

            // Attach pipeline event handlers
            _pipeline.OnScoreComputed += OnScoreComputedEvent;
            _pipeline.OnPipelineFinished += OnPipelineCompletedEvent;

            // Start pipeline execution
            StartPipeline(collection.Select(asset => (asset.Id, asset.Data)).ToList());
        }

        private void OnScoreComputedEvent(object sender, (Guid, double) args)
        {
            var (imageId, score) = args;

            // Update the log and progress
            DispatcherQueue.TryEnqueue(() =>
            {
                // Find the item in the list and update its status
                var item = ScoreList.FirstOrDefault(x => x.ImageId == imageId);
                if (item != null)
                {
                    // Directly update the existing row's Score and Status
                    item.Status = $"Score: {score}";
                    item.Score = score.ToString("F2");
                }
            });
        }

        private void OnPipelineCompletedEvent(object sender, TimeSpan duration)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                StatusMessage.Text = $"Pipeline Completed in {duration.TotalSeconds:F2} sec";
                ProgressIndicator.IsActive = false;
            });
        }

        private async void StartPipeline(IEnumerable<(Guid, ImageData)> images)
        {
            StatusMessage.Text = "Processing...";
            ProgressIndicator.IsActive = true;

            // Initialize ScoreList with the new images only once (avoid clearing)
            foreach (var image in images)
            {
                // Only add the items the first time
                if (!ScoreList.Any(x => x.ImageId == image.Item1))
                {
                    ScoreList.Add(new ScoreItem
                    {
                        ImageId = image.Item1,
                        Status = "Processing...",
                        Score = "0.00"
                    });
                }
            }

            // Execute the pipeline
            await _pipeline.Compute(images);
        }
    }

    public class ScoreItem : INotifyPropertyChanged
    {
        private string _status;
        private string _score;

        public Guid ImageId { get; set; }

        // Implement INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(Status)); // Notify that Status has changed
                }
            }
        }

        public string Score
        {
            get => _score;
            set
            {
                if (_score != value)
                {
                    _score = value;
                    OnPropertyChanged(nameof(Score)); // Notify that Score has changed
                }
            }
        }

        // This method is called to notify the UI about property changes
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
