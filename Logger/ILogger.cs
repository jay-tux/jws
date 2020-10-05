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

    public static class LogFormat
    {
        public static bool Colors = true;
        public static int MaxTypeLength = 10;
        public static int MaxModLength = 9;
        public static string DateTimeFmt = "yyyy-MM0dd HH:mm:ss.ffffff tt";
        public static string Formatting = "[@TYPE in @MOD at @TIME]: @MSG";

        public static string ColorError = "\u001b[31m";
        public static string ColorWarning = "\u001b[33m";
        public static string ColorMessage = "\u001b[37m";
        public static string ColorDebug = "\u001b[36m";
        public static string ColorReset = "\u001b[0m";

        private static string ToLength(this string todo, int maxlen, bool padLeft)
			=> todo.Length > maxlen ? (todo.Substring(0, maxlen - 3) + "...") : (padLeft ? todo.PadLeft(maxlen) : todo.PadRight(maxlen));

        public static string FormatString(string source, string message, LogSeverity severity)
        {
            string formatted = Formatting;
            formatted = formatted.Replace("@TYPE", severity.ToString().ToUpper().ToLength(MaxTypeLength, false))
                .Replace("@MOD", source.ToLength(MaxModLength, true))
                .Replace("@TIME", DateTime.Now.ToString(DateTimeFmt))
                .Replace("@MSG", message);

            if(Colors)
            {
                switch(severity)
                {
                    case LogSeverity.Debug: formatted = $"{ColorDebug}{formatted}{ColorReset}"; break;
                    case LogSeverity.Message: formatted = $"{ColorMessage}{formatted}{ColorReset}"; break;
                    case LogSeverity.Warning: formatted = $"{ColorWarning}{formatted}{ColorReset}"; break;
                    case LogSeverity.Error: formatted = $"{ColorError}{formatted}{ColorReset}"; break;
                }
            }
            return formatted;
        }

        public static void LogFormatted(this ILogger logger, string source, string message, LogSeverity severity, DateTime tm)
        {
            logger.Log(FormatString(source, message.Replace("@TIME", tm.ToString(DateTimeFmt)), severity), severity);
        }

        public static void LogFormatted(this ILogger logger, string source, string message, LogSeverity severity)
        {
            logger.Log(FormatString(source, message, severity), severity);
        }
    }

    public enum LogSeverity { Debug, Message, Warning, Error }

    public class SimpleLogger : ILogger
    {
        public static SimpleLogger Instance = new SimpleLogger();

        private SimpleLogger() {}

        public void Log(string message) => Log(message, LogSeverity.Message);
        public void Log(object message) => Log(message.ToString());

        public void Log(string message, LogSeverity sev) => Console.WriteLine($"{sev.ToString().ToUpper()}\t{message}");
        public void Log(object message, LogSeverity sev) => Log(message.ToString(), sev);
    }
}
