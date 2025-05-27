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
