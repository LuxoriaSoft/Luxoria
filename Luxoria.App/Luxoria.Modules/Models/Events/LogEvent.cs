namespace Luxoria.Modules.Models.Events
{
    public class LogEvent
    {
        public string Message { get; }

        public LogEvent(string message)
        {
            Message = message;
        }
    }
}