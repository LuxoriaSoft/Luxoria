using Luxoria.SDK.Interfaces;
using Luxoria.SDK.Models;
using System.Diagnostics;

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
