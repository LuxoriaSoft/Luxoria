using LuxFilter.Interfaces;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models;
using Luxoria.Modules.Models.Events;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace LuxFilter.Views;

/// <summary>
/// Status view
/// </summary>
public sealed partial class StatusView : Page
{
    private readonly IEventBus _eventBus;
    private readonly MainFilterView _parent;
    private readonly IPipelineService _pipeline;

    /// <summary>
    /// ScoreList property
    /// </summary>
    public ObservableCollection<ScoreItem> ScoreList { get; } = [];

    /// <summary>
    /// Scores to be exported as a dictionary
    /// </summary>
    private ConcurrentDictionary<Guid, double> _scores { get; } = [];

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="eventBus">IPC Communication</param>
    /// <param name="parent">Parent view</param>
    /// <param name="pipeline">Pipeline to be triggered</param>
    /// <param name="collection">Collection to be sorted</param>
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
        StartPipeline(collection);
    }

    /// <summary>
    /// Event handler when a score has been computed
    /// </summary>
    private void OnScoreComputedEvent(object sender, (Guid, double) args)
    {
        var (imageId, score) = args;

        _scores[imageId] = score;

        // Update the log and progress
        DispatcherQueue.TryEnqueue(() =>
        {
            // Find the item in the list and update its status
            var item = ScoreList.FirstOrDefault(x => x.ImageId == imageId);
            if (item != null)
            {
                // Directly update the existing row's Score and Status
                item.IsComputing = false;
                item.Score = score.ToString("F2");
            }
        });
    }

    /// <summary>
    /// Event handler when the pipeline has finished computing scores
    /// </summary>
    private void OnPipelineCompletedEvent(object sender, TimeSpan duration)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            StatusMessage.Text = $"Pipeline Completed in {duration.TotalSeconds:F2} sec";
            ProgressIndicator.IsActive = false;
        });

        // Notify the user that the pipeline has completed
        _eventBus.Publish(new ToastNotificationEvent
        {
            Title = "Filter",
            Message = $"Pipeline completed in {duration.TotalSeconds:F2} seconds. ({_scores.Count} asset(s))",
        });

        // Publish the scores to the event bus
        _eventBus.Publish(new CollectionEvaluationCompletedEvent
        {
            AssetsScores = _scores.ToDictionary(x => x.Key, x => x.Value),
        });

        // Clear scores
        _scores.Clear();
    }

    /// <summary>
    /// Start the pipeline
    /// </summary>
    /// <param name="images">Collamr</param>
    private async void StartPipeline(IEnumerable<LuxAsset> collection)
    {
        StatusMessage.Text = "Processing...";
        ProgressIndicator.IsActive = true;

        foreach (var asset in collection)
        {
            // Only add the items the first time
            if (!ScoreList.Any(x => x.ImageId == asset.Id))
            {
                ScoreList.Add(new ScoreItem(asset.Id, asset.MetaData.Name)
                {
                    IsComputing = true
                });
            }
        }

        // Execute the pipeline
        ICollection<(Guid, Dictionary<string, double>)> pipelineResult = await _pipeline.Compute(collection.Select(asset => (asset.Id, asset.Data)).ToList());
    }

    /// <summary>
    /// St the filter view when the user clicks on the "Start Over" button
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void StartOver_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Clear the scores and reset the view
        ScoreList.Clear();
        _scores.Clear();

        // Set the filter view
        _parent.SetFilterView();
    }
}

public class ScoreItem : INotifyPropertyChanged
{
    private string _score;
    private bool _isComputing;

    /// <summary>
    /// ImageId property
    /// </summary>
    public Guid ImageId { get; }

    /// <summary>
    /// DisplayName property
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    public ScoreItem(Guid imageId, string displayName) => (ImageId, DisplayName) = (imageId, displayName);

    /// <summary>
    /// PropertyChanged event handler
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Status property
    /// </summary>
    public bool IsComputing
    {
        get => _isComputing;
        set
        {
            if (_isComputing != value)
            {
                _isComputing = value;
                OnPropertyChanged(nameof(IsComputing)); // Notify that Status has changed
            }
        }
    }

    /// <summary>
    /// Score property
    /// </summary>
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

    /// <summary>
    /// Notify the UI that a property has changed
    /// </summary>
    /// <param name="propertyName"></param>
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
