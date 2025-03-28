using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxExport.Interfaces
{
    public interface ITagResolver
    {
        bool CanResolve(string tag);
        string Resolve(string tag, string originalName, IReadOnlyDictionary<string, string> metadata, int counter);
    }

}
