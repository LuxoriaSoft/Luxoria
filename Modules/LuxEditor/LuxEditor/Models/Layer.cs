using Microsoft.UI;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI;

namespace LuxEditor.Models
{
    public class Layer : INotifyPropertyChanged
    {
        private string _name = "Layer";
        private bool _visible = true;
        private bool _invert;
        private double _strength = 100;
        private Color _overlayColor = Colors.Black;
        private double _overlayOpacity = 0.3;
        private BrushType _brushType;
        public ObservableCollection<MaskOperation> Operations { get; } = new ObservableCollection<MaskOperation>();

        public string Name
        {
            get => _name;
            set => SetField(ref _name, value);
        }

        public bool Visible
        {
            get => _visible;
            set => SetField(ref _visible, value);
        }

        public bool Invert
        {
            get => _invert;
            set => SetField(ref _invert, value);
        }

        public double Strength
        {
            get => _strength;
            set => SetField(ref _strength, value);
        }

        public Color OverlayColor
        {
            get => _overlayColor;
            set => SetField(ref _overlayColor, value);
        }

        public double OverlayOpacity
        {
            get => _overlayOpacity;
            set => SetField(ref _overlayOpacity, value);
        }

        public BrushType BrushType
        {
            get => _brushType;
            set => SetField(ref _brushType, value);
        }

        public ObservableCollection<Stroke> Strokes { get; } = new ObservableCollection<Stroke>();

        public event PropertyChangedEventHandler PropertyChanged;
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}
