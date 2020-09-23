using System;

namespace Jay.IO.Logging
{
    public interface IEventLogger : ILogger
    {
        event EventHandler<LogEventArgs> LogEvent;
        event EventHandler<LogEventArgs> LogWarningEvent;
        event EventHandler<LogEventArgs> LogMessageEvent;
        event EventHandler<LogEventArgs> LogErrorEvent;
        event EventHandler<LogEventArgs> LogDebugEvent;
    }

    public abstract class DefaultEventLogger : IEventLogger
    {
        protected static bool FirePre;
        public event EventHandler<LogEventArgs> LogEvent;
        public event EventHandler<LogEventArgs> LogWarningEvent;
        public event EventHandler<LogEventArgs> LogMessageEvent;
        public event EventHandler<LogEventArgs> LogErrorEvent;
        public event EventHandler<LogEventArgs> LogDebugEvent;

        public override void Log(string message)
        {
            LogEventArgs args = new LogEventArgs(LogSeverity.Message, message);
            if(FirePre) Fire(args);
            CascadeLog(message);
            if(!FirePre) Fire(args);
        }

        public override void Log(string message, LogSeverity severity)
        {
            LogEventArgs args = new LogEventArgs(severity, message);
            if(FirePre) Fire(args);
            CascadeLog(message, severity);
            if(!FirePre) Fire(args);
        }

        public override void Log(object message) => Log(message.ToString());
        public override void Log(object message, LogSeverity severity) => Log(message, severity);

        public abstract void CascadeLog(string message);
        public abstract void CascadeLog(string message, LogSeverity severity);

        protected void Fire(LogEventArgs args)
        {
            OnLog(args);
            switch(args.Severity)
            {
                case LogSeverity.Message: OnLogMessage(args); break;
                case LogSeverity.Debug: OnLogDebug(args); break;
                case LogSeverity.Error: OnLogError(args); break;
                case LogSeverity.Warning: OnLogWarning(args); break;
            }
        }

        protected virtual void OnLog(LogEventArgs args) => LogEvent?.Invoke(args);
        protected virtual void OnLogWarning(LogEventArgs args) => LogWarningEvent?.Invoke(args);
        protected virtual void OnLogMessage(LogEventArgs args) => LogMessageEvent?.Invoke(args);
        protected virtual void OnLogError(LogEventArgs args) => LogErrorEvent?.Invoke(args);
        protected virtual void OnLogDebug(LogEventArgs args) => LogDebugEvent?.Invoke(args);
    }

    public class LogEventArgs : EventArgs
    {
        public LogSeverity Severity;
        public string Message;
        public DateTime Timestamp;

        public LogEventArgs(LogSeverity severity, string message)
        {
            Severity = severity;
            Message = message;
            Timestamp = DateTime.Now;
        }
    }
}
