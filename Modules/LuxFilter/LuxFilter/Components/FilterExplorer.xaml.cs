using LuxFilter.Interfaces;
using LuxFilter.Models;
using LuxFilter.Services;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models;
using Luxoria.Modules.Models.Events;
using Luxoria.SDK.Interfaces;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LuxFilter.Components;

public sealed partial class FilterExplorer : UserControl
{
    private readonly IEventBus _eventBus;
    private readonly ILoggerService _logger;
    private IPipelineService? _pipeline;
    public ObservableCollection<FilterItem> Filters { get; set; } = [];

    public FilterExplorer(IEventBus eventBus, ILoggerService logger)
    {
        InitializeComponent();

        _eventBus = eventBus;
        _logger = logger;

        AttachEventHandlers();
        LoadFiltersCollection();
    }

    private async void LoadFiltersCollection()
    {
        ShowLoadingMessage();

        try
        {
            var filterEvent = new FilterCatalogEvent();
            await _eventBus.Publish(filterEvent);
            var receivedFilters = await filterEvent.Response.Task;

            if (receivedFilters is null || receivedFilters.Count == 0)
            {
                HideLoadingMessage();
                return;
            }

            DispatcherQueue.TryEnqueue(() =>
            {
                Filters.Clear();
                foreach (var (name, description, version) in receivedFilters)
                {
                    Filters.Add(new FilterItem(name, description, version));
                }

                HideLoadingMessage();
            });
        }
        catch (Exception)
        {
            HideLoadingMessage();
        }
    }

    private void OnFilterClicked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is FilterItem filter)
        {
            ShowLoadingMessage();
            Debug.WriteLine($"Filter clicked: {filter.Name} - {filter.Description} - {filter.Version}");
            _pipeline = new PipelineService(_logger)
                .AddAlgorithm(FilterService.Catalog[filter.Name], 1.0);

            _pipeline.OnScoreComputed += (s, score) =>
            {
                Debug.WriteLine($"Score computed for {score.Item1}: {score.Item2}");
            };
            _pipeline.OnPipelineFinished += (s, duration) =>
            {
                Debug.WriteLine($"Pipeline finished in {duration.TotalMilliseconds} ms");
                HideLoadingMessage();
            };

            // Run the pipeline in a task to avoid blocking the UI thread
            Task.Run(async () =>
            {
                if (_pipeline is null)
                {
                    Debug.WriteLine("Pipeline is not initialized.");
                    HideLoadingMessage();
                    return;
                }

                Collection<(Guid, ImageData)> bitmaps = []; // Replace with actual bitmap data

                await Task.Delay(2000);
                var result = await _pipeline.Compute(bitmaps);
                if (result is not null)
                {
                    Debug.WriteLine($"Pipeline result: {result.Count} items processed.");
                }
                else
                {
                    Debug.WriteLine("Pipeline returned no results.");
                }
            });
        }
    }

    private void AttachEventHandlers()
    {
        _eventBus.Subscribe<FilterAlgorithmsLoadedEvent>(e =>
        {
            HideLoadingMessage();
        });
    }

    private void HideLoadingMessage()
    {
        LoadingMsg.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
    }

    private void ShowLoadingMessage()
    {
        LoadingMsg.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
    }
}
