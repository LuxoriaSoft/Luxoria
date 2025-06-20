using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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

        private readonly EditableImage _image;

        public LayerManager(EditableImage img)
        {
            _image = img;
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
            _image.SaveState();
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
            _image.SaveState();
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
            _image.SaveState();
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
            _image.SaveState();
        }

        public void AddOperation(MaskOperation maskOperation)
        {
            if (SelectedLayer != null)
            {
                SelectedLayer.Operations.Add(maskOperation);
                SelectedLayer.SelectedOperation = maskOperation;
            }
            OnOperationChanged?.Invoke();
            _image.SaveState();
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
            _image.SaveState();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        public LayerManager Clone()
        {
            var clonedManager = new LayerManager(_image);

            foreach (var layer in Layers)
            {
                var clonedLayer = new Layer
                {
                    Name = layer.Name,
                    Visible = layer.Visible,
                    Invert = layer.Invert,
                    Strength = layer.Strength,
                    OverlayColor = layer.OverlayColor
                };

                foreach (var op in layer.Operations)
                {
                    var clonedOp = new MaskOperation(op.Tool.ToolType, op.Mode)
                    {
                        Tool = op.Tool.Clone(),
                    };
                    clonedLayer.Operations.Add(clonedOp);
                }

                clonedLayer.SelectedOperation = clonedLayer.Operations
                    .FirstOrDefault(op => op.Id == layer.SelectedOperation?.Id);

                clonedLayer.PropertyChanged += Layer_PropertyChanged;
                clonedManager.Layers.Add(clonedLayer);
            }

            clonedManager.SelectedLayer = clonedManager.Layers
                .FirstOrDefault(l => l.Id == SelectedLayer?.Id);

            return clonedManager;
        }

        public void RestoreFrom(LayerManager source)
        {
            Layers.Clear();

            foreach (var layer in source.Layers)
            {
                var clonedLayer = new Layer
                {
                    Name = layer.Name,
                    Visible = layer.Visible,
                    Invert = layer.Invert,
                    Strength = layer.Strength,
                    OverlayColor = layer.OverlayColor
                };

                foreach (var op in layer.Operations)
                {
                    var clonedOp = new MaskOperation(op.Tool.ToolType, op.Mode)
                    {
                        Tool = op.Tool.Clone()
                    };
                    clonedLayer.Operations.Add(clonedOp);
                }

                clonedLayer.SelectedOperation = clonedLayer.Operations
                    .FirstOrDefault(op => op.Id == layer.SelectedOperation?.Id);

                clonedLayer.PropertyChanged += Layer_PropertyChanged;
                Layers.Add(clonedLayer);
            }

            SelectedLayer = Layers.FirstOrDefault(l => l.Id == source.SelectedLayer?.Id);

            OnLayerChanged?.Invoke();
            OnOperationChanged?.Invoke();
        }

    }
}
