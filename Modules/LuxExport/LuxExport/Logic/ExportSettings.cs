using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxExport.Logic
{
    /// <summary>
    /// Represents the settings used for exporting images, including quality, color space, and file size limits.
    /// </summary>
    public class ExportSettings
    {
        public int Quality { get; set; } = 100;
        public string ColorSpace { get; set; } = "sRGB";
        public bool LimitFileSize { get; set; } = false;
        public int MaxFileSizeKB { get; set; } = 0;
    }
}
