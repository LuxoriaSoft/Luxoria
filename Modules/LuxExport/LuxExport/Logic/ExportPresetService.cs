using LuxExport.Models;
using Luxoria.Modules.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace LuxExport.Logic
{
    /// <summary>
    /// Manages export presets persistence using the vault storage system.
    /// Provides default presets and allows users to create custom ones.
    /// </summary>
    public sealed class ExportPresetService
    {
        private const string PresetsKey = "exportPresets";
        private readonly IStorageAPI _vault;

        /// <summary>
        /// Builds an ExportPresetService bound to the vault storage.
        /// </summary>
        public ExportPresetService(IStorageAPI vault)
        {
            _vault = vault;
        }

        /// <summary>
        /// Gets all presets (default + custom) from storage.
        /// If no presets exist, creates and returns default presets.
        /// </summary>
        public List<ExportPreset> GetPresets()
        {
            if (!_vault.Contains(PresetsKey))
            {
                var defaultPresets = CreateDefaultPresets();
                SavePresets(defaultPresets);
                return defaultPresets;
            }

            var presetsJson = _vault.Get<string>(PresetsKey);
            var options = new JsonSerializerOptions { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };

            return JsonSerializer.Deserialize<List<ExportPreset>>(presetsJson, options) ?? CreateDefaultPresets();
        }

        /// <summary>
        /// Saves the preset list to storage.
        /// </summary>
        public void SavePresets(List<ExportPreset> presets)
        {
            var json = JsonSerializer.Serialize(presets, new JsonSerializerOptions { WriteIndented = true, TypeInfoResolver = new DefaultJsonTypeInfoResolver() });
            _vault.Save(PresetsKey, json);
        }

        /// <summary>
        /// Adds a new custom preset to the existing list.
        /// </summary>
        public void AddPreset(ExportPreset preset)
        {
            var presets = GetPresets();
            presets.Add(preset);
            SavePresets(presets);
        }

        /// <summary>
        /// Removes a preset by name from the existing list.
        /// </summary>
        public bool RemovePreset(string name)
        {
            var presets = GetPresets();
            var preset = presets.FirstOrDefault(p => p.Name == name);
            if (preset != null)
            {
                presets.Remove(preset);
                SavePresets(presets);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Updates an existing preset with new values.
        /// </summary>
        public bool UpdatePreset(string oldName, ExportPreset newPreset)
        {
            var presets = GetPresets();
            var existingPreset = presets.FirstOrDefault(p => p.Name == oldName);
            if (existingPreset != null)
            {
                var index = presets.IndexOf(existingPreset);
                presets[index] = newPreset;
                SavePresets(presets);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets a specific preset by name.
        /// </summary>
        public ExportPreset? GetPreset(string name)
        {
            return GetPresets().FirstOrDefault(p => p.Name == name);
        }

        /// <summary>
        /// Creates the default export presets based on common usage scenarios.
        /// </summary>
        private static List<ExportPreset> CreateDefaultPresets()
        {
            return new List<ExportPreset>
            {
                // JPEG Quality Variants
                new ExportPreset
                {
                    Name = "JPEG High Quality",
                    Description = "Maximum quality JPEG (100%) for archival and printing",
                    Format = ExportFormat.JPEG,
                    Quality = 100,
                    ColorSpace = "sRGB",
                    LimitFileSize = false,
                    CustomFileFormat = "{name}",
                    ExtensionCase = "a..z",
                    ExportLocation = "Desktop",
                },
                new ExportPreset
                {
                    Name = "JPEG Medium Quality",
                    Description = "Medium quality JPEG (80%) for general use",
                    Format = ExportFormat.JPEG,
                    Quality = 80,
                    ColorSpace = "sRGB",
                    LimitFileSize = false,
                    CustomFileFormat = "{name}",
                    ExtensionCase = "a..z",
                    ExportLocation = "Desktop",
                },
                new ExportPreset
                {
                    Name = "JPEG Low Quality",
                    Description = "Lower quality JPEG (60%) for quick sharing",
                    Format = ExportFormat.JPEG,
                    Quality = 60,
                    ColorSpace = "sRGB",
                    LimitFileSize = true,
                    MaxFileSizeKB = 500,
                    CustomFileFormat = "{name}",
                    ExtensionCase = "a..z",
                    ExportLocation = "Desktop",
                },
                
                // Web Export Variants
                new ExportPreset
                {
                    Name = "Web High + Watermark",
                    Description = "High quality web export with watermark protection",
                    Format = ExportFormat.JPEG,
                    Quality = 100,
                    ColorSpace = "sRGB",
                    CustomFileFormat = "{name}_web",
                    ExtensionCase = "a..z",
                    CreateSubfolder = true,
                    SubfolderName = "Web_Export",
                    WatermarkEnabled = true,
                    WatermarkType = WatermarkType.Text,
                    WatermarkText = "© Luxoria",
                    WatermarkOpacity = 40,
                    WatermarkPosition = "Bottom Right",
                    ExportLocation = "Web",
                },
                new ExportPreset
                {
                    Name = "Web High - No Watermark",
                    Description = "High quality web export without watermark",
                    Format = ExportFormat.JPEG,
                    Quality = 100,
                    ColorSpace = "sRGB",
                    CustomFileFormat = "{name}_web",
                    ExtensionCase = "a..z",
                    ExportLocation = "Web",
                    CreateSubfolder = true,
                    SubfolderName = "Web_Export",
                    WatermarkEnabled = false
                },
                new ExportPreset
                {
                    Name = "Web Medium + Watermark",
                    Description = "Medium quality web export with watermark, fast loading",
                    Format = ExportFormat.JPEG,
                    Quality = 75,
                    ColorSpace = "sRGB",
                    CustomFileFormat = "{name}_web",
                    ExtensionCase = "a..z",
                    ExportLocation = "Web",
                    CreateSubfolder = true,
                    SubfolderName = "Web_Export",
                    WatermarkEnabled = true,
                    WatermarkType = WatermarkType.Text,
                    WatermarkText = "© Luxoria",
                    WatermarkOpacity = 35,
                    WatermarkPosition = "Bottom Right"
                },
                new ExportPreset
                {
                    Name = "Web Medium - No Watermark",
                    Description = "Medium quality web export without watermark",
                    Format = ExportFormat.JPEG,
                    Quality = 75,
                    ColorSpace = "sRGB",
                    CustomFileFormat = "{name}_web",
                    ExtensionCase = "a..z",
                    ExportLocation = "Web",
                    CreateSubfolder = true,
                    SubfolderName = "Web_Export",
                    WatermarkEnabled = false
                },
                
                // PNG for Transparency
                new ExportPreset
                {
                    Name = "PNG Transparency",
                    Description = "Lossless PNG with transparency support",
                    Format = ExportFormat.PNG,
                    Quality = 100,
                    ColorSpace = "sRGB",
                    LimitFileSize = false,
                    CustomFileFormat = "{name}",
                    ExtensionCase = "a..z"
                },
                
                // Email/Sharing Preset
                new ExportPreset
                {
                    Name = "Email/Sharing",
                    Description = "Optimized file size for email attachments",
                    Format = ExportFormat.JPEG,
                    Quality = 70,
                    ColorSpace = "sRGB",
                    LimitFileSize = true,
                    MaxFileSizeKB = 300,
                    CustomFileFormat = "{name}_email",
                    ExtensionCase = "a..z"
                }
            };
        }
    }
}