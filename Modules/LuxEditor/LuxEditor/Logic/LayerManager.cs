using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using LuxEditor.Models;
using Windows.UI;

namespace LuxEditor.Logic
{
    public class LayerManager : INotifyPropertyChanged
    {

        public ObservableCollection<Layer> Layers { get; } = new ObservableCollection<Layer>();

        private Layer? _selectedLayer;
        public Layer? SelectedLayer
        {
            get => _selectedLayer;
            set
            {
                SetField(ref _selectedLayer, value);
                OnLayerChanged?.Invoke();
                if (value != null && value.SelectedOperation == null && value.Operations.Count > 0)
                {
                    value.SelectedOperation = value.Operations[0];
                }
                OnOperationChanged?.Invoke();
            }
        }

        public Action? OnLayerChanged;
        public Action? OnOperationChanged;

        public LayerManager()
        {
        }

        public MaskOperation CreateMaskOperation(ToolType brushType, BooleanOperationMode mode = BooleanOperationMode.Add)
        {
            var op = new MaskOperation(brushType, mode);
            OnOperationChanged?.Invoke();
            return op;
        }

        public void AddLayer(ToolType type)
        {
            var layer = new Layer()
            {
                Name = $"Layer {Layers.Count}",
                Visible = true,
                Invert = false,
                Strength = 100,
                OverlayColor = Color.FromArgb(100, 255, 0, 0),
            };

            layer.Operations.Add(CreateMaskOperation(type));
            layer.SelectedOperation = layer.Operations[0];

            Layers.Insert(0, layer);

            SelectedLayer = layer;
            OnLayerChanged?.Invoke();
            OnOperationChanged?.Invoke();
            layer.PropertyChanged += Layer_PropertyChanged;
        }

        private void Layer_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnLayerChanged?.Invoke();
        }

        public void RenameLayer(uint id, string name)
        {
            var layer = Layers.FirstOrDefault(l => l.Id == id);
            if (layer != null)
            {
                layer.Name = name;
            }
        }

        public uint? GetSelectedLayerId() => SelectedLayer?.Id ?? null;

        public Layer? GetLayer() => SelectedLayer;

        public Layer? GetLayer(uint id)
        {
            return Layers.FirstOrDefault(l => l.Id == id) ?? null;
        }

        public Layer? GetLayerByOperation(uint operationId)
        {
            return Layers.FirstOrDefault(l => l.Operations.Any(op => op.Id == operationId));
        }

        public void RemoveLayer(uint id)
        {
            var layer = GetLayer(id);
            if (layer != null)
            {
                bool shouldSelectPrevious = SelectedLayer != null && layer == SelectedLayer;
                int idx = Layers.IndexOf(layer);
                Layers.Remove(layer);

                if (Layers.Count > 0 && shouldSelectPrevious)
                {
                    SelectedLayer = Layers[Math.Max(0, Math.Min(idx, Layers.Count - 1))];
                }
                else
                {
                    SelectedLayer = null;
                }
            }
            OnLayerChanged?.Invoke();
            OnOperationChanged?.Invoke();
        }

        public void RemoveLayer()
        {
            if (SelectedLayer != null)
            {
                RemoveLayer(SelectedLayer.Id);
            }
        }

        public ObservableCollection<MaskOperation> GetOperations(uint id)
        {
            var layer = GetLayer(id);
            return layer?.Operations ?? new ObservableCollection<MaskOperation>();
        }

        public ObservableCollection<MaskOperation> GetSelectedOperations()
        {
            return SelectedLayer?.Operations ?? new ObservableCollection<MaskOperation>();
        }

        public void AddOperation(uint id, MaskOperation maskOperation)
        {
            var layer = GetLayer(id);
            if (layer != null)
            {
                layer.Operations.Add(maskOperation);
                layer.SelectedOperation = maskOperation;
            }
            OnOperationChanged?.Invoke();
        }

        public void AddOperation(MaskOperation maskOperation)
        {
            if (SelectedLayer != null)
            {
                SelectedLayer.Operations.Add(maskOperation);
                SelectedLayer.SelectedOperation = maskOperation;
            }
            OnOperationChanged?.Invoke();
        }

        public void RemoveOperation(uint operationId)
        {
            var layer = GetLayerByOperation(operationId);
            if (layer != null)
            {
                if (layer.Operations.Count == 1)
                {
                    RemoveLayer(layer.Id);
                    return;
                }
                var operation = layer.Operations.FirstOrDefault(op => op.Id == operationId);
                if (operation != null)
                {
                    layer.Operations.Remove(operation);
                    layer.SelectedOperation = null;
                }
            }
            OnOperationChanged?.Invoke();
        }

        public void MoveLayer(uint id, int newIndex)
        {
            var layer = GetLayer(id);
            if (layer == null) return;
            int oldIndex = Layers.IndexOf(layer);
            if (oldIndex < 0 || newIndex < 0 || newIndex >= Layers.Count) return;
            Layers.RemoveAt(oldIndex);
            Layers.Insert(newIndex, layer);
            SelectedLayer = layer;
            OnLayerChanged?.Invoke();
            OnOperationChanged?.Invoke();
        }

        public void MoveLayer(int oldIndex, int newIndex)
        {
            if (oldIndex < 0 || oldIndex >= Layers.Count || newIndex < 0 || newIndex >= Layers.Count) return;
            var layer = Layers[oldIndex];
            Layers.RemoveAt(oldIndex);
            Layers.Insert(newIndex, layer);
            SelectedLayer = layer;
            OnLayerChanged?.Invoke();
            OnOperationChanged?.Invoke();
        }

        //private void RefreshZIndices()
        //{
        //    for (int i = 0; i < Layers.Count; i++)
        //    {
        //        Layers[i].ZIndex = (uint)(i + 1);
        //    }
        //}

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
