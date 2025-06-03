using LuxEditor.Controls;
using Microsoft.UI;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI;

namespace LuxEditor.Models
{
    public class Layer : INotifyPropertyChanged
    {
        static private uint _nextId = 1;
        private uint _id;
        private uint _zIndex;
        private string _name = "Layer";
        private bool _visible = true;
        private bool _invert;
        private double _strength = 100;
        private Color _overlayColor = Color.FromArgb(100, 255, 255, 255);
        public ObservableCollection<MaskOperation> Operations { get; }
        public MaskOperation? SelectedOperation { get; set; }
        public LayersDetailsPanel DetailsPanel { get; set; }

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

        public uint Id
        {
            get => _id;
            private set => SetField(ref _id, value);
        }

        public uint ZIndex
        {
            get => _zIndex;
            set => SetField(ref _zIndex, value);
        }

        public Layer(uint zIndex)
        {
            _id = _nextId++;
            _zIndex = zIndex;
            Operations = new ObservableCollection<MaskOperation>();
            DetailsPanel = new LayersDetailsPanel();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}
