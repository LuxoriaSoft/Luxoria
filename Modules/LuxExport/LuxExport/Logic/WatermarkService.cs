using Luxoria.Modules.Interfaces;
using Newtonsoft.Json;
using SkiaSharp;
using System.IO;

namespace LuxExport.Logic
{
    /// <summary>
    /// Persists watermark settings (and the optional logo) inside the LuxExport vault located in AppData.
    /// </summary>
    public sealed class WatermarkService
    {
        private const string VaultName = "LuxExport";
        private const string SettingsKey = "watermark.json";
        private const string LogoKey = "watermark_logo_b64";
        private readonly IStorageAPI _vault;

        /// <summary>
        /// Builds a WatermarkService bound to the LuxExport vault.
        /// </summary>
        public WatermarkService(IStorageAPI vault)
        {
            _vault = vault;
        }

        /// <summary>
        /// Saves the given settings (and embedded logo) to the vault.
        /// </summary>
        public void Save(WatermarkSettings settings)
        {
            _vault.Save(SettingsKey, settings);
            if (settings.Type == WatermarkType.Logo && settings.Logo != null)
            {
                using var ms = new MemoryStream();
                settings.Logo.Encode(SKEncodedImageFormat.Png, 100).SaveTo(ms);
                _vault.Save(LogoKey, System.Convert.ToBase64String(ms.ToArray()));
            }
        }

        /// <summary>
        /// Loads settings from the vault (defaults if none were stored).
        /// </summary>
        public WatermarkSettings Load()
        {
            if (!_vault.Contains(SettingsKey))
                return new WatermarkSettings();

            var s = _vault.Get<WatermarkSettings>(SettingsKey);
            if (s.Type == WatermarkType.Logo && _vault.Contains(LogoKey))
            {
                var b64 = _vault.Get<string>(LogoKey);
                s.Logo = SKBitmap.Decode(System.Convert.FromBase64String(b64));
            }
            return s;
        }
    }
}
