using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using System.IO;
using System.Threading.Tasks;
using System;

namespace Luxoria.App.Views
{
    public sealed class PickSaveFileWindow : Window
    {
        private readonly TaskCompletionSource<string?> _tcs = new();
        private readonly TextBox _folderBox = new();
        private readonly TextBox _nameBox = new();
        private readonly string _ext;

        public Task<string?> PickAsync() => _tcs.Task;

        public PickSaveFileWindow(string suggestedName, string ext)
        {
            _ext = ext;
            Title = "Save preset";
            
            var browse = new Button { Content = "Browse", MinWidth = 80 };
            var save = new Button { Content = "Save", MinWidth = 80 };
            var cancel = new Button { Content = "Cancel", MinWidth = 80 };

            browse.Click += Browse_Click;
            save.Click += OnSave;
            cancel.Click += (_, __) => CloseWindow(null);

            _nameBox.Text = Path.GetFileNameWithoutExtension(suggestedName);

            Content = new StackPanel
            {
                Spacing = 12,
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 8,
                        Children = { _folderBox, browse }
                    },
                    _nameBox,
                    new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Spacing = 10,
                        Children = { cancel, save }
                    }
                }
            };
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
            Close();
        }
    }
}
