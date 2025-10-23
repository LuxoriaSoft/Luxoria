using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using System.IO;
using System.Threading.Tasks;
using System;
using Microsoft.UI.Xaml.Media;

namespace Luxoria.App.Views
{
    public sealed class PickSaveFileWindow : Window
    {
        private readonly TaskCompletionSource<string?> _tcs = new();
        private readonly TextBox _folderBox = new() { IsReadOnly = true };
        private readonly TextBox _nameBox = new();
        private readonly string _ext;

        public Task<string?> PickAsync() => _tcs.Task;

        public PickSaveFileWindow(string suggestedName, string ext)
        {
            _ext = ext;
            Title = "Save Preset";

            // Header
            var titleBlock = new TextBlock
            {
                Text = "Save Preset",
                FontSize = 20,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 16)
            };

            // Folder section with label above input
            var folderLabel = new TextBlock
            {
                Text = "Location",
                FontSize = 14,
                FontWeight = Microsoft.UI.Text.FontWeights.Medium,
                Margin = new Thickness(0, 0, 0, 8)
            };

            var folderInputPanel = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                ColumnSpacing = 8
            };

            _folderBox.PlaceholderText = "Select a folder...";
            _folderBox.CornerRadius = new Microsoft.UI.Xaml.CornerRadius(4);
            Grid.SetColumn(_folderBox, 0);
            folderInputPanel.Children.Add(_folderBox);

            var browseButton = new Button
            {
                Content = "Browse",
                MinWidth = 100,
                Height = 32,
                CornerRadius = new Microsoft.UI.Xaml.CornerRadius(4)
            };
            browseButton.Click += Browse_Click;
            Grid.SetColumn(browseButton, 1);
            folderInputPanel.Children.Add(browseButton);

            // File name section with label above input
            var nameLabel = new TextBlock
            {
                Text = "Preset Name",
                FontSize = 14,
                FontWeight = Microsoft.UI.Text.FontWeights.Medium,
                Margin = new Thickness(0, 20, 0, 8)
            };

            _nameBox.Text = Path.GetFileNameWithoutExtension(suggestedName);
            _nameBox.PlaceholderText = "Enter preset name...";
            _nameBox.CornerRadius = new Microsoft.UI.Xaml.CornerRadius(4);

            // File extension hint
            var extHint = new TextBlock
            {
                Text = $"File will be saved as: {Path.GetFileNameWithoutExtension(suggestedName)}{ext}",
                FontSize = 12,
                Opacity = 0.7,
                Margin = new Thickness(0, 6, 0, 0)
            };

            // Action buttons
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 12,
                Margin = new Thickness(0, 28, 0, 0)
            };

            var cancelButton = new Button
            {
                Content = "Cancel",
                MinWidth = 100,
                Height = 36,
                CornerRadius = new Microsoft.UI.Xaml.CornerRadius(4)
            };
            cancelButton.Click += (_, __) => CloseWindow(null);

            var saveButton = new Button
            {
                Content = "Save",
                MinWidth = 100,
                Height = 36,
                Style = (Microsoft.UI.Xaml.Style)Application.Current.Resources["AccentButtonStyle"],
                CornerRadius = new Microsoft.UI.Xaml.CornerRadius(4)
            };
            saveButton.Click += OnSave;

            buttonPanel.Children.Add(cancelButton);
            buttonPanel.Children.Add(saveButton);

            // Update extension hint when name changes
            _nameBox.TextChanged += (_, __) =>
            {
                var displayName = string.IsNullOrWhiteSpace(_nameBox.Text)
                    ? "filename"
                    : _nameBox.Text;
                extHint.Text = $"File will be saved as: {displayName}{ext}";
            };

            // Main container
            var contentPanel = new StackPanel
            {
                Padding = new Thickness(24),
                Spacing = 0,
                Children =
                {
                    titleBlock,
                    folderLabel,
                    folderInputPanel,
                    nameLabel,
                    _nameBox,
                    extHint,
                    buttonPanel
                }
            };

            var root = new Border
            {
                Background = (Brush)Application.Current.Resources["SystemControlBackgroundBaseLowBrush"],
                Child = contentPanel
            };

            // Set responsive window size with min/max constraints
            this.AppWindow.Resize(new(520, 340));

            Content = root;

            // Set focus to name input
            this.Activated += (_, __) => _nameBox.Focus(FocusState.Programmatic);
        }

        private async void Browse_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FolderPicker();
            picker.FileTypeFilter.Add("*");

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var folder = await picker.PickSingleFolderAsync();
            if (folder != null) _folderBox.Text = folder.Path;
        }

        private void OnSave(object sender, RoutedEventArgs e)
        {
            var folder = _folderBox.Text;
            var name = _nameBox.Text;

            if (string.IsNullOrWhiteSpace(folder) || string.IsNullOrWhiteSpace(name))
            {
                CloseWindow(null);
                return;
            }
            if (!name.EndsWith(_ext, System.StringComparison.OrdinalIgnoreCase))
                name += _ext;

            CloseWindow(Path.Combine(folder, name));
        }

        private void CloseWindow(string? result)
        {
            _tcs.TrySetResult(result);
            this.Close();
        }
    }
}
