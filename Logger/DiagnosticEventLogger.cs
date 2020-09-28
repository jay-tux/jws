using System;
using System.Diagnostics;

namespace Jay.IO.Logging
{
    public abstract class DiagnosticEventLogger : DefaultEventLogger
    {
        public new void Log(string message)
        {
            LogEventArgs args = new DiagnosticLogEventArgs(LogSeverity.Message, message);
            if(FirePre) Fire(args);
            CascadeLog(message);
            if(!FirePre) Fire(args);
        }

        public new void Log(string message, LogSeverity severity)
        {
            LogEventArgs args = new DiagnosticLogEventArgs(severity, message);
            if(FirePre) Fire(args);
            CascadeLog(message);
            if(!FirePre) Fire(args);
        }
    }

    public class DiagnosticLogEventArgs : LogEventArgs
    {
        public StackFrame Frame { get => Trace.GetFrame(1); }
        public StackTrace Trace;

        public DiagnosticLogEventArgs(LogSeverity severity, string message) : base(severity, message)
        {
            Trace = new StackTrace(true);
        }
    }
}
