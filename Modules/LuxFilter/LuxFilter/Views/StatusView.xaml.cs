using LuxFilter.Interfaces;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LuxFilter.Views;

/// <summary>
/// Status View to display the progress of the Filter Pipeline.
/// </summary>
public sealed partial class StatusView : Page
{
    private readonly IEventBus _eventBus;
    private readonly MainFilterView _parent;
    private readonly IPipelineService _pipeline;

    public ObservableCollection<KeyValuePair<string, double>> ScoreList { get; } = [];


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="eventBus">Event communication (IPC)</param>
    /// <param name="parent">Parent View</param>
    /// <param name="pipeline">Pipeline service to be triggered</param>
    public StatusView(IEventBus eventBus, MainFilterView parent, IPipelineService pipeline)
    {
        _eventBus = eventBus;
        _parent = parent;
        _pipeline = pipeline;

        // Model properties
        Width = 600;

        this.InitializeComponent();

        // Attach pipeline event handlers
        _pipeline.OnScoreComputed += OnScoreComputedEvent;
        _pipeline.OnPipelineFinished += OnPipelineCompletedEvent;

        // Bind score list to UI
        ScoreListView.ItemsSource = ScoreList;
    }

    /// <summary>
    /// Called when a score is computed.
    /// </summary>
    private void OnScoreComputedEvent(object sender, (Guid, double) args)
    {
        var (imageId, score) = args;

        _ = DispatcherQueue.TryEnqueue(() =>
        {
            ScoreList.Add(new KeyValuePair<string, double>($"Image {ScoreList.Count + 1}", score));
            ScoreListView.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        });
    }

    /// <summary>
    /// Called when the pipeline computation is complete.
    /// </summary>
    private void OnPipelineCompletedEvent(object sender, TimeSpan duration)
    {
        _ = DispatcherQueue.TryEnqueue(() =>
        {
            StatusMessage.Text = $"Pipeline Completed in {duration.TotalSeconds:F2} sec";
            ProgressIndicator.IsActive = false;
        });
    }

    /// <summary>
    /// Start pipeline execution
    /// </summary>
    public async void StartPipeline(IEnumerable<(Guid, ImageData)> images)
    {
        StatusMessage.Text = "Processing...";
        ProgressIndicator.IsActive = true;
        ScoreList.Clear();
        ScoreListView.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;

        await _pipeline.Compute(images);
    }
}
