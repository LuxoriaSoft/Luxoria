using LuxFilter.Interfaces;
using LuxFilter.Models;
using LuxFilter.Services;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models;
using Luxoria.Modules.Models.Events;
using Luxoria.SDK.Interfaces;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LuxFilter.Components;

public sealed partial class FilterExplorer : UserControl
{
    private readonly IEventBus _eventBus;
    private readonly ILoggerService _logger;
    private IPipelineService? _pipeline;
    ICollection<LuxAsset> _assets;

    public ObservableCollection<FilterItem> Filters { get; set; } = [];

    public FilterExplorer(IEventBus eventBus, ILoggerService logger)
    {
        InitializeComponent();

        _eventBus = eventBus;
        _logger = logger;

        AttachEventHandlers();
        LoadFiltersCollection();
    }

    public void SetImages(ICollection<LuxAsset> assets) => _assets = assets;

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
                FilterListPanel.Children.Clear();

                foreach (var (name, description, version) in receivedFilters)
                {
                    var filter = new FilterItem(name, description, version);
                    Filters.Add(filter);
                    FilterListPanel.Children.Add(CreateFilterButton(filter));
                }

                HideLoadingMessage();
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading filters: {ex.Message}");
            HideLoadingMessage();
        }
    }

    private Button CreateFilterButton(FilterItem filter)
    {
        var icon = new FontIcon
        {
            Glyph = "\uE768",
            Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green),
            VerticalAlignment = VerticalAlignment.Center
        };

        var text = new TextBlock
        {
            Text = filter.Name,
            VerticalAlignment = VerticalAlignment.Center,
            Padding = new Thickness(20, 0, 0, 0)
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        Grid.SetColumn(icon, 0);
        Grid.SetColumn(text, 1);
        grid.Children.Add(icon);
        grid.Children.Add(text);

        var button = new Button
        {
            Tag = (filter, icon),
            Content = grid,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(5)
        };

        button.Click += OnFilterClicked;
        return button;
    }

    public event Action<(string, Guid, double)>? OnScoreUpdated;

    private async void OnFilterClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        if (button.Tag is not ValueTuple<FilterItem, FontIcon> tag) return;

        var (filter, icon) = tag;

        button.IsEnabled = false;

        icon.Visibility = Visibility.Collapsed;
        var spinner = new ProgressRing
        {
            Width = 16,
            Height = 16,
            IsActive = true,
            VerticalAlignment = VerticalAlignment.Center
        };

        var parentGrid = button.Content as Grid;
        if (parentGrid == null)
        {
            button.IsEnabled = true;
            return;
        }

        Grid.SetColumn(spinner, 0);
        parentGrid.Children.Add(spinner);

        try
        {
            var localPipeline = new PipelineService(_logger)
                .AddAlgorithm(FilterService.Catalog[filter.Name], 1.0);

            localPipeline.OnScoreComputed += (s, score) =>
            {
                OnScoreUpdated?.Invoke((filter.Name, score.Item1, score.Item2));
                _logger.Log($"Score computed for {score.Item1}: {score.Item2}");
            };

            localPipeline.OnPipelineFinished += (s, duration) =>
            {
                _logger.Log($"Pipeline for '{filter.Name}' finished in {duration.TotalMilliseconds} ms");

                DispatcherQueue.TryEnqueue(() =>
                {
                    parentGrid.Children.Remove(spinner);
                    icon.Visibility = Visibility.Visible;
                    button.IsEnabled = true;
                });
            };

            await Task.Run(async () =>
            {
                if (_assets is null || !_assets.Any())
                {
                    _logger.Log("No assets to process.");
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        parentGrid.Children.Remove(spinner);
                        icon.Visibility = Visibility.Visible;
                        button.IsEnabled = true;
                    });
                    return;
                }
                var result = await localPipeline.Compute(_assets.Select(asset => (asset.Id, asset.Data)));

                if (result is not null)
                    _logger.Log($"Pipeline result for '{filter.Name}': {result.Count} items.");
                else
                    _logger.Log($"Pipeline for '{filter.Name}' returned no results.");
            });
        }
        catch (Exception ex)
        {
            _logger.Log($"Error during filter execution: {ex.Message}");

            DispatcherQueue.TryEnqueue(() =>
            {
                parentGrid.Children.Remove(spinner);
                icon.Visibility = Visibility.Visible;
                button.IsEnabled = true;
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

    private void HideLoadingMessage() =>
        LoadingMsg.Visibility = Visibility.Collapsed;

    private void ShowLoadingMessage() =>
        LoadingMsg.Visibility = Visibility.Visible;
}
