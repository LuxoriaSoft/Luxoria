using Newtonsoft.Json;
using SkiaSharp;

namespace LuxExport.Logic
{
    /// <summary>
    /// Stores every option required to draw a watermark on an image.
    /// </summary>
    public class WatermarkSettings
    {
        public bool Enabled { get; set; } = false;
        public WatermarkType Type { get; set; } = WatermarkType.Text;
        public string Text { get; set; } = "Luxoria";
        public byte Opacity { get; set; } = 64;
        public float Angle { get; set; } = -45f;
        public int Step { get; set; } = 0;
        public string FontFamily { get; set; } = "Arial";
        public int FontSize { get; set; } = 64;
        [JsonIgnore]
        public SKBitmap? Logo { get; set; }
    }

    /// <summary>
    /// Indicates whether the watermark is a text banner or a logo.
    /// </summary>
    public enum WatermarkType
    {
        Text,
        Logo
    }
}
