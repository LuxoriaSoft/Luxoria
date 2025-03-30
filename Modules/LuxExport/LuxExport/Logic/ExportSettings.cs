using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxExport.Logic
{
    public class ExportSettings
    {
        public int Quality { get; set; } = 100;
        public string ColorSpace { get; set; } = "sRGB";
        public bool LimitFileSize { get; set; } = false;
        public int MaxFileSizeKB { get; set; } = 0;
    }
}
