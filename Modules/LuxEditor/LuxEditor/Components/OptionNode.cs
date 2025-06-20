using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml.Media.Imaging;

namespace LuxEditor.Components
{
    public class OptionNode : INotifyPropertyChanged
    {
        public string DisplayName { get; }
        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set { _isChecked = value; OnPropertyChanged(); }
        }

        public ObservableCollection<OptionNode> Children { get; } = new();
        public bool HasPreview { get; set; }
        public BitmapImage PreviewImage { get; set; }
        public object Tag { get; set; }

        public OptionNode(string name, bool @checked = false)
        {
            DisplayName = name;
            IsChecked = @checked;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? n = null)
          => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

        public IEnumerable<OptionNode> Flatten()
        {
            yield return this;
            foreach (var c in Children)
                foreach (var d in c.Flatten())
                    yield return d;
        }
    }
}
