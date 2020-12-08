# JWS - Logger/DiagnosticEventLogger.cs
*This is the documentation about the source file located under `Logger/DiagnosticEventLogger.cs`; only public/protected fields/methods/classes are described in here.*

## Abstract Class Jay.IO.Logging.DiagnosticEventLogger
*Base class for creating RP loggers which keep stack-trace data.*  
Inheritance: [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object?view=net-5.0) -> [DefaultEventLogger](./IEventLogger.md) -> [DiagnosticEventLogger](./DiagnosticEventLogger.md).

### Hiding Methods
 - ``public new void Log(string message)``: creates the diagnostic info, throws the pre-event (if necessary), logs the message (using the ``CascaceLog(message)`` in the implementating class) and throws the post-event (if necessary).  
 - ``public new void Log(string message, LogSeverity severity)``: creates the diagnostic info, throws the pre-event (if necessary), logs the message (using the ``CascaceLog(message, severity)`` in the implementating class) and throws the post-event (if necessary).  

## Class DiagnosticLogEventArgs
*Event data for Diagnostic Log Events.*  
Inheritance: [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object?view=net-5.0) -> [EventArgs](https://docs.microsoft.com/en-us/dotnet/api/system.eventargs?view=net-5.0) -> [LogEventArgs](./IEventLogger.md) -> [DiagnosticLogEventArgs](./DiagnosticEventLogger.md).

### Fields
 - ``public StackTrace Trace``: the current at the moment of creation.  

### Properties
 - ``public StackFrame Frame { get; }``: the top-most stack frame at the moment of creation (wrapper around ``Trace.GetFrame(1)``).

### Constructors
 - ``public DiagnosticLogEventArgs(LogSeverity severity, string message)``: creates a new instance of this class, setting the stack trace and stack frame accordingly.
