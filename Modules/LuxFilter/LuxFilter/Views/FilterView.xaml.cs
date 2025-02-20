using LuxFilter.Interfaces;
using LuxFilter.Services;
using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models.Events;
using Luxoria.SDK.Interfaces;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace LuxFilter.Views
{
    public sealed partial class FilterView : Page
    {
        private readonly IEventBus _eventBus;
        private readonly ILoggerService _logger;
        private readonly MainFilterView _parent;

        public ObservableCollection<FilterItem> Filters { get; set; } = [];

        public FilterView(IEventBus eventBus, ILoggerService logger, MainFilterView parent)
        {
            _eventBus = eventBus;
            _logger = logger;
            _parent = parent;

            this.InitializeComponent();

            LoadFiltersCollection();
        }

        private async void LoadFiltersCollection()
        {
            try
            {
                var filterEvent = new FilterCatalogEvent();

                // Publish the event and wait for a response
                await _eventBus.Publish(filterEvent);
                var receivedFilters = await filterEvent.Response.Task;

                if (receivedFilters is null || receivedFilters.Count == 0)
                {
                    _logger.Log("No filters received.");
                    return;
                }

                _logger.Log($"Received {receivedFilters.Count} filters.");

                // Ensure UI updates happen on the main thread
                DispatcherQueue.TryEnqueue(() =>
                {
                    Filters.Clear();
                    foreach (var (name, description, version) in receivedFilters)
                    {
                        Filters.Add(new FilterItem(name, description, version));
                    }
                    _logger.Log($"Loaded {Filters.Count} filters.");
                });
            }
            catch (Exception ex)
            {
                _logger.Log($"Error loading filters: {ex.Message}");
            }
        }


        private void OnApplyFiltersClicked(object sender, RoutedEventArgs e)
        {
            var selectedFilters = Filters.Where(f => f.IsSelected).ToList();
            if (selectedFilters.Count == 0)
            {
                return;
            }

            IPipelineService pipeline = new PipelineService(_logger);

            foreach (var filter in selectedFilters)
            {
                pipeline.AddAlgorithm(FilterService.Catalog[filter.Name], filter.Weight);
            }

            _parent.SetStatusView(pipeline);
        }
    }

    /// <summary>
    /// FilterItem class to represent a filter in the UI
    /// </summary>
    public partial class FilterItem : INotifyPropertyChanged
    {
        /// <summary>
        /// PropertyChanged event handler
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isSelected;
        private double _weight;

        /// <summary>
        /// Public properties for the FilterItem
        /// </summary>
        public string Name { get; }
        public string Description { get; }
        public string Version { get; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        private const double Epsilon = 1e-6; // Small threshold for floating point comparisons

        public double Weight
        {
            get => _weight;
            set
            {
                if (Math.Abs(_weight - value) > Epsilon) // Compare within a small range
                {
                    _weight = value;
                    OnPropertyChanged(nameof(Weight));
                    OnPropertyChanged(nameof(FormattedWeight));
                }
            }
        }


        public string FormattedWeight => Weight.ToString("0.0");

        public FilterItem(string name, string description, string version)
        {
            Name = name;
            Description = description;
            Version = version;
            IsSelected = false;
            Weight = 0.5; // Default weight
        }

        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
