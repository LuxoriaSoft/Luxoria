using Luxoria.Modules.Interfaces;
using Luxoria.Modules.Models.Events;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LuxFilter.Views
{
    public sealed partial class FilterView : Page
    {
        private readonly IEventBus _eventBus;
        private readonly MainFilterView _parent;

        public ObservableCollection<FilterItem> Filters { get; set; } = new();

        public FilterView(IEventBus eventBus, MainFilterView parent)
        {
            _eventBus = eventBus;
            _parent = parent;

            this.InitializeComponent(); // Ensure UI is initialized first

            LoadFiltersCollection();
        }

        private async void LoadFiltersCollection()
        {
            try
            {
                var filterEvent = new FilterCatalogEvent();

                // Publish the event and wait for a response
                await _eventBus.Publish(filterEvent);
                var receivedFilters = await filterEvent.Response.Task; // Wait for the response

                if (receivedFilters is null || receivedFilters.Count == 0)
                {
                    Debug.WriteLine("No filters received.");
                    return;
                }

                Debug.WriteLine($"Received {receivedFilters.Count} filters.");

                // Ensure UI updates happen on the main thread
                DispatcherQueue.TryEnqueue(() =>
                {
                    Filters.Clear();
                    foreach (var (name, description, version) in receivedFilters)
                    {
                        Filters.Add(new FilterItem(name, description, version));
                    }
                    Debug.WriteLine($"Loaded {Filters.Count} filters.");
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading filters: {ex.Message}");
            }
        }


        /// <summary>
        /// Event handler for the Apply Filters button
        /// </summary>
        private void OnApplyFiltersClicked(object sender, RoutedEventArgs e)
        {
            var selectedFilters = Filters.Where(f => f.IsSelected).ToList();
            if (selectedFilters.Count == 0)
            {
                Debug.WriteLine("No filters selected. Please select at least one filter.");
                return;
            }

            string selectedFiltersText = string.Join(", ", selectedFilters.Select(f => $"{f.Name} ({f.Weight:F1})"));
            Debug.WriteLine($"Applying Filters: {selectedFiltersText}");
        }
    }

    public class FilterItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isSelected;
        private double _weight;

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

        public double Weight
        {
            get => _weight;
            set
            {
                if (_weight != value)
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
