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

    public class SimpleLogger : ILogger
    {
        public static SimpleLogger Instance = new SimpleLogger();

        private SimpleLogger() {}

        public override void Log(string message) => Log(message, LogSeverity.Message);
        public override void Log(object message) => Log(message.ToString());

        public override void Log(string message, LogSeverity sev) => Console.WriteLine($"{sev.ToString().ToUpper()}\t{message}");
        public override void Log(object message, LogSeverity sev) => Log(message.ToString(), sev);
    }
}
