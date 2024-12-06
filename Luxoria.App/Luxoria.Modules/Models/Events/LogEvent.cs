using System.Diagnostics.CodeAnalysis;

namespace Luxoria.Modules.Models.Events
{
    [ExcludeFromCodeCoverage]
    public class LogEvent
    {
        public string Message { get; }

        public LogEvent(string message)
        {
            Message = message;
        }
    }
}