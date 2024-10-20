using Luxoria.SDK.Interfaces;
using Luxoria.SDK.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxoria.SDK.Services
{
    public class LoggerService : ILoggerService
    {
        public void Log(string message, string category = "General", LogLevel level = LogLevel.Info)
        {
            Debug.WriteLine($"{DateTime.Now} [{level}] {category}: {message}");
        }
    }
}
