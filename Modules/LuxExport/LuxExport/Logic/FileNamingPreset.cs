using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxExport.Logic;
public class FileNamingPreset
{
    public required string Name { get; set; }
    public required string Pattern { get; set; }
}
