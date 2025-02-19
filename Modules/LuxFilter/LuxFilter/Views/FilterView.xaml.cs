using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace LuxFilter.Views
{
    public sealed partial class FilterView : Page
    {
        public ObservableCollection<FilterItem> Filters { get; set; } = new()
        {
            new FilterItem("Brisque"),
            new FilterItem("Sharpness"),
            new FilterItem("Contrast"),
            new FilterItem("Brightness"),
            new FilterItem("Color Balance")
        };

        public FilterView()
        {
            this.InitializeComponent();
        }

        private void OnApplyFiltersClicked(object sender, RoutedEventArgs e)
        {
            var selectedFilters = Filters.Where(f => f.IsSelected).ToList();

            if (selectedFilters.Count == 0)
            {
                ContentDialog noSelectionDialog = new()
                {
                    Title = "No Filters Selected",
                    Content = "Please select at least one filter before applying.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                _ = noSelectionDialog.ShowAsync();
                return;
            }

            // TODO: Apply selected filters with their weights
            string selectedFiltersText = string.Join(", ", selectedFilters.Select(f => $"{f.Name} ({f.Weight:F1})"));

            ContentDialog confirmationDialog = new()
            {
                Title = "Filters Applied",
                Content = $"Applying: {selectedFiltersText}",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            _ = confirmationDialog.ShowAsync();
        }
    }

    public class FilterItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isSelected;
        private double _weight;

        public string Name { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public double Weight
        {
            get => _weight;
            set
            {
                _weight = value;
                OnPropertyChanged(nameof(Weight));
                OnPropertyChanged(nameof(FormattedWeight));
            }
        }

        public string FormattedWeight => Weight.ToString("0.0");

        public FilterItem(string name)
        {
            Name = name;
            IsSelected = false;
            Weight = 0.5; // Default weight
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
