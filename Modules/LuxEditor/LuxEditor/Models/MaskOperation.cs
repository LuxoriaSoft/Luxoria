using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SkiaSharp;
using Windows.UI;

namespace LuxEditor.Models
{
    public class MaskOperation : INotifyPropertyChanged
    {
        private BrushType _brushType;
        private StrokeMode _mode;
        private Color _overlayColor = Color.FromArgb(255, 0, 0, 0);
        private double _overlayOpacity = 1.0;
        private double _strength = 100;

        public BrushType BrushType
        {
            get => _brushType;
            set => SetField(ref _brushType, value);
        }

        public StrokeMode Mode
        {
            get => _mode;
            set => SetField(ref _mode, value);
        }

        public Color OverlayColor
        {
            get => _overlayColor;
            set => SetField(ref _overlayColor, value);
        }

        public double OverlayOpacity
        {
            get => _overlayOpacity;
            set
            {
                if (value < 0) value = 0;
                if (value > 1) value = 1;
                SetField(ref _overlayOpacity, value);
            }
        }

        public double Strength
        {
            get => _strength;
            set
            {
                if (value < 0) value = 0;
                if (value > 200) value = 200;
                SetField(ref _strength, value);
            }
        }

        public ObservableCollection<Stroke> Strokes { get; } = new ObservableCollection<Stroke>();

        public MaskOperation(BrushType brushType, StrokeMode mode = StrokeMode.Add)
        {
            _brushType = brushType;
            _mode = mode;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null!)
        {
            if (Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}
