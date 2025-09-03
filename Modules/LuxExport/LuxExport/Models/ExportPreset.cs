using LuxExport.Logic;

namespace LuxExport.Models
{
    /// <summary>
    /// Represents a complete export preset containing all export settings and parameters.
    /// </summary>
    public class ExportPreset
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        
        // Format & Export Settings
        public ExportFormat Format { get; set; } = ExportFormat.JPEG;
        public int Quality { get; set; } = 100;
        public string ColorSpace { get; set; } = "sRGB";
        public bool LimitFileSize { get; set; } = false;
        public int MaxFileSizeKB { get; set; } = 0;
        
        // File Naming Settings
        public bool RenameFile { get; set; } = true;
        public string FileNamingMode { get; set; } = "Filename";
        public string CustomFileFormat { get; set; } = "{name}";
        public string ExtensionCase { get; set; } = "A..Z";
        
        // Export Location Settings  
        public string ExportLocation { get; set; } = "Desktop";
        public string FileConflictResolution { get; set; } = "Overwrite";
        public bool CreateSubfolder { get; set; } = false;
        public string SubfolderName { get; set; } = "Luxoria";
        
        // Watermark Settings
        public bool WatermarkEnabled { get; set; } = false;
        public WatermarkType WatermarkType { get; set; } = WatermarkType.Text;
        public string WatermarkText { get; set; } = "";
        public int WatermarkOpacity { get; set; } = 50;
        public string WatermarkPosition { get; set; } = "Bottom Right";
    }
}