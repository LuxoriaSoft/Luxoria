using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxExport.Logic
{
    /// <summary>
    /// Represents a file naming preset, which includes a name and a pattern for generating file names.
    /// </summary>
    public class FileNamingPreset
    {
        public required string Name { get; set; }
        public required string Pattern { get; set; }
    }
}
