using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Luxoria.SDK.Models;

namespace Luxoria.SDK.Interfaces
{
    public interface ILoggerService
    {
        void Log(string message, string category = "General", LogLevel level = LogLevel.Info);
    }
}
