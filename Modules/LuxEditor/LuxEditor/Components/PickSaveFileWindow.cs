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
            Title = "Save preset";

            var root = new Grid
            {
                Padding = new Thickness(20),
                Background = (Brush)Application.Current.Resources["SystemControlBackgroundBaseLowBrush"],
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };

            var folderLabel = new TextBlock
            {
                Text = "Folder:",
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(folderLabel, 0);
            Grid.SetColumn(folderLabel, 0);
            root.Children.Add(folderLabel);

            _folderBox.Margin = new Thickness(5, 0, 5, 0);
            Grid.SetRow(_folderBox, 0);
            Grid.SetColumn(_folderBox, 1);
            root.Children.Add(_folderBox);

            var browse = new Button
            {
                Content = "Browse…",
                MinWidth = 75
            };
            browse.Click += Browse_Click;
            Grid.SetRow(browse, 0);
            Grid.SetColumn(browse, 2);
            root.Children.Add(browse);

            var nameLabel = new TextBlock
            {
                Text = "File name:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 12, 0, 0)
            };
            Grid.SetRow(nameLabel, 1);
            Grid.SetColumn(nameLabel, 0);
            root.Children.Add(nameLabel);

            _nameBox.Text = Path.GetFileNameWithoutExtension(suggestedName);
            _nameBox.Margin = new Thickness(5, 12, 0, 0);
            Grid.SetRow(_nameBox, 1);
            Grid.SetColumn(_nameBox, 1);
            Grid.SetColumnSpan(_nameBox, 2);
            root.Children.Add(_nameBox);

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 20, 0, 0)
            };
            var save = new Button
            {
                Content = "Save",
                MinWidth = 75
            };
            save.Click += OnSave;
            var cancel = new Button
            {
                Content = "Cancel",
                MinWidth = 75,
                Margin = new Thickness(10, 0, 0, 0)
            };
            cancel.Click += (_, __) => CloseWindow(null);

            buttonPanel.Children.Add(save);
            buttonPanel.Children.Add(cancel);

            Grid.SetRow(buttonPanel, 2);
            Grid.SetColumn(buttonPanel, 0);
            Grid.SetColumnSpan(buttonPanel, 3);
            root.Children.Add(buttonPanel);

            this.AppWindow.Resize(new(400, 200));

            Content = root;
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
