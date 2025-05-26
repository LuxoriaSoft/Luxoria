using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using LuxEditor.Models;

namespace LuxEditor.Logic
{
    public class LayerManager : INotifyPropertyChanged
    {
        private static readonly Lazy<LayerManager> _instance = new(() => new LayerManager());
        public static LayerManager Instance => _instance.Value;

        public ObservableCollection<Layer> Layers { get; } = new ObservableCollection<Layer>();

        public IEnumerable<Layer> LayersFiltered => Layers.Skip(1);

        private Layer _selectedLayer;
        public Layer SelectedLayer
        {
            get => _selectedLayer;
            set => SetField(ref _selectedLayer, value);
        }

        private LayerManager()
        {
            var overall = new Layer { Name = "Overall", BrushType = BrushType.Brush };
            Layers.Add(overall);
            SelectedLayer = overall;
        }
        public void AddLayer(BrushType type)
        {
            int index = SelectedLayer != null
                ? Layers.IndexOf(SelectedLayer) + 1
                : Layers.Count;

            var layer = new Layer
            {
                Name = $"Layer {Layers.Count}",
                BrushType = type
            };
            
            layer.Operations.Add(new MaskOperation(BrushType.Brush));

            Layers.Insert(index, layer);
            SelectedLayer = layer;
        }


        public void RemoveSelectedLayer()
        {
            if (SelectedLayer == null) return;
            int idx = Layers.IndexOf(SelectedLayer);
            if (idx <= 0) return;
            Layers.RemoveAt(idx);
            SelectedLayer = Layers[Math.Max(0, idx - 1)];
        }

        public void MoveLayer(int oldIndex, int newIndex)
        {
            if (oldIndex <= 0 || oldIndex >= Layers.Count) return;
            newIndex = Math.Max(1, Math.Min(Layers.Count - 1, newIndex));
            var layer = Layers[oldIndex];
            Layers.RemoveAt(oldIndex);
            Layers.Insert(newIndex, layer);
        }

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
