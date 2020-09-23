using System;

namespace Jay.IO.Logging
{
    public interface ILogger
    {
        void Log(string message);
        void Log(object message);
        void Log(string message, LogSeverity severity);
        void Log(object message, LogSeverity severity);
    }

    public enum LogSeverity { Debug, Message, Warning, Error }
}
