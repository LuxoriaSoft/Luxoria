using LuxEditor.Controls;
using Microsoft.UI;
using System;
using System.Collections.Generic;
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
        private Color _overlayColor;
        public ObservableCollection<MaskOperation> Operations { get; }
        public MaskOperation? SelectedOperation { get; set; }
        public LayersDetailsPanel DetailsPanel { get; set; }
        public Dictionary<string, object> Filters { get; set; } = new()
        {
            ["Temperature"] = 6500f,
            ["Tint"] = 0f,
            ["Exposure"] = 0f,
            ["Contrast"] = 0f,
            ["Highlights"] = 0f,
            ["Shadows"] = 0f,
            ["Whites"] = 0f,
            ["Blacks"] = 0f,
            ["Texture"] = 0f,
            ["Dehaze"] = 0f,
            ["Vibrance"] = 0f,
            ["Saturation"] = 0f,
        };

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
            set
            {
                SetField(ref _overlayColor, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OverlayColor)));
            }
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

        /// <summary>
        /// Creates a new layer with the specified z-index.
        /// </summary>
        /// <param name="zIndex"></param>
        public Layer(uint zIndex)
        {
            _id = _nextId++;
            _zIndex = zIndex;
            Operations = new ObservableCollection<MaskOperation>();
            DetailsPanel = new LayersDetailsPanel();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Sets a field and raises the PropertyChanged event if the value has changed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        /// <summary>
        /// Notifies that the filters have changed, triggering UI updates.
        /// </summary>
        public void NotifyFiltersChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Filters)));
        }

        /// <summary>
        /// Checks if the layer has any active filters applied.
        /// </summary>
        /// <returns></returns>
        public bool HasActiveFilters()
        {
            foreach (var kv in Filters)
            {
                switch (kv.Value)
                {
                    case float f:
                        if (kv.Key == "Temperature")
                        {
                            if (Math.Abs(f - 6500f) > 1e-2) return true;
                        }
                        else if (Math.Abs(f) > 1e-2)
                        {
                            return true;
                        }
                        break;

                    case byte[] lut when LutIsModified(lut):
                        return true;

                    default:
                        if (kv.Value != null) return true;
                        break;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if the LUT (Lookup Table) is modified from its default state.
        /// </summary>
        /// <param name="lut"></param>
        /// <returns></returns>
        private static bool LutIsModified(byte[] lut)
        {
            if (lut.Length != 256)
                return true;

            for (int i = 0; i < 256; i++)
                if (lut[i] != i)
                    return true;

            return false;
        }

    }

}
