using SkiaSharp;

namespace LuxEditor.EditorUI.Controls
{
    public sealed class ColorChannelCurve : PointCurve
    {
        private readonly string _key;
        public override string SettingKey => _key;

        /// <summary>
        /// Initialises a new color channel curve with a semi-tinted background and white stroke.
        /// </summary>
        /// <param name="tint"></param>
        public ColorChannelCurve(string key, SKColor tint)
            : base(new SKColor(tint.Red, tint.Green, tint.Blue, 50),
                   new SKColor((byte)(255 - tint.Red),
                               (byte)(255 - tint.Green),
                               (byte)(255 - tint.Blue), 50))
        {
            _key = key;
        }
    }
}
