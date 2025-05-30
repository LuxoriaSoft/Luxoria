using Luxoria.Modules.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace LuxFilter.Components
{
    public sealed partial class FlagsComponent : UserControl
    {
        private LuxAsset? _selectedAsset;

        public event Action<LuxAsset>? OnFlagUpdated;

        public FlagsComponent()
        {
            InitializeComponent();
        }

        public void SetSelectedAsset(LuxAsset? asset)
        {
            _selectedAsset = asset;

            if (_selectedAsset == null)
            {
                DisplayNoSelectionMessage();
                return;
            }

            // Update toggle states based on asset flags
            FilterData.FlagType? flag = _selectedAsset.FilterData.GetFlag();

            if (flag == null)
            {
                FKeep.IsChecked = false;
                FIgnore.IsChecked = false;
            }
            else
            {
                FKeep.IsChecked = flag == FilterData.FlagType.Keep;
                FIgnore.IsChecked = flag == FilterData.FlagType.Ignore;
            }
            HideNoSelectionMessage();
        }

        private void DisplayNoSelectionMessage()
        {
            FStack.Visibility = Visibility.Collapsed;
            NoSelectionPanel.Visibility = Visibility.Visible;
        }

        private void HideNoSelectionMessage()
        {
            FStack.Visibility = Visibility.Visible;
            NoSelectionPanel.Visibility = Visibility.Collapsed;
        }

        private void FKeep_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedAsset == null) return;

            // If toggle is checked, set the flag to Keep and uncheck FIgnore
            _selectedAsset.FilterData.SetFlag(FKeep.IsChecked == true ? FilterData.FlagType.Keep : null);
            if (FKeep.IsChecked == true)
            {
                FIgnore.IsChecked = false; // Uncheck Ignore if it was checked
            }
            OnFlagUpdated?.Invoke(_selectedAsset);
        }

        private void FIgnore_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedAsset == null) return;

            // If toggle is checked, set the flag to Ignore and uncheck FKeep
            _selectedAsset.FilterData.SetFlag(FIgnore.IsChecked == true ? FilterData.FlagType.Ignore : null);
            if (FIgnore.IsChecked == true)
            {
                FKeep.IsChecked = false; // Uncheck Keep if it was checked
            }
            OnFlagUpdated?.Invoke(_selectedAsset);
        }
    }
}
