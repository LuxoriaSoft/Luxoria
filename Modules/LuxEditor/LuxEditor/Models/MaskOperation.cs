using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using LuxEditor.EditorUI.Controls.ToolControls;
using LuxEditor.EditorUI.Interfaces;
using SkiaSharp;
using Windows.UI;

namespace LuxEditor.Models
{
    public enum BooleanOperationMode
    {
        Add,
        Subtract
    }

    public class MaskOperation : INotifyPropertyChanged
    {
        static private uint _nextId = 1;

        private BooleanOperationMode _mode;
        private uint _id;
        public ATool Tool;


        public BooleanOperationMode Mode
        {
            get => _mode;
            set => SetField(ref _mode, value);
        }

        public uint Id => _id;


        public MaskOperation(ToolType brushType, BooleanOperationMode mode = BooleanOperationMode.Add)
        {
            _mode = mode;
            _id = _nextId++;

            switch (brushType)
            {
                case ToolType.Brush:
                    Tool = new BrushToolControl(mode);
                    Debug.WriteLine("MaskOperation: Brush tool initialized.");
                    break;
                case ToolType.LinearGradient:
                    Tool = new LinearGradientToolControl(mode);
                    Debug.WriteLine("MaskOperation: LinearGradient tool initialized.");
                    break;
                case ToolType.RadialGradient:
                    Tool = new RadialGradientToolControl(mode);
                    Debug.WriteLine("MaskOperation: RadialGradient tool initialized.");
                    break;
                case ToolType.ColorRange:
                    //Tool = new ColorRangeToolControl();
                    Debug.WriteLine("MaskOperation: ColorRange tool initialized.");
                    break;
                default:
                    throw new ArgumentException("Unsupported tool type for MaskOperation");
            }
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
