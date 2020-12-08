# JWS - Logger/IEventLogger.cs
*This is the documentation about the source file located under `Logger/IEventLogger.cs`; only public/protected fields/methods/classes are described in here.*

## Interface Jay.IO.Logging.IEventLogger
*Logger interface with 5 events, can be used to hook multiple loggers together.*  
Inheritance: [ILogger](./ILogger.md) -> [IEventLogger](./IEventLogger.md).

### Events
 - ``event EventHandler<LogEventArgs> LogEvent``: event which is fired whenever something is logged.  
 - ``event EventHandler<LogEventArgs> LogWarningEvent``: event which is fired whenever a warning is logged.  
 - ``event EventHandler<LogEventArgs> LogErrorEvent``: event which is fired whenever a error is logged.  
 - ``event EventHandler<LogEventArgs> LogMessageEvent``: event which is fired whenever a message is logged.  
 - ``event EventHandler<LogEventArgs> LogDebugEvent``: event which is fired whenever a debug message is logged.  

## Abstract Class Jay.IO.Logging.DefaultEventLogger
*Base class for creating RP loggers.*  
Inheritance: [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object?view=net-5.0) -> [DefaultEventLogger](./IEventLogger.md).  
Implements: [IEventLogger](./IEventLogger.md).  

### Fields
 - ``protected bool FirePre``: log before (true) or after (false) the logging itself happened.  

### Events
 - ``event EventHandler<LogEventArgs> LogEvent``: event which is fired whenever something is logged.  
 - ``event EventHandler<LogEventArgs> LogWarningEvent``: event which is fired whenever a warning is logged.  
 - ``event EventHandler<LogEventArgs> LogErrorEvent``: event which is fired whenever a error is logged.  
 - ``event EventHandler<LogEventArgs> LogMessageEvent``: event which is fired whenever a message is logged.  
 - ``event EventHandler<LogEventArgs> LogDebugEvent``: event which is fired whenever a debug message is logged.  

### Methods
 - ``public void Log(string message)``: prepares the event arguments and logs the message using the CascadeLog method. Events are fired.  
 - ``public void Log(string message, LogSeverity severity)``: prepares the event arguments and logs the message using the CascadeLog method. Events are fired.  
 - ``public void Log(object message)``: shorthand for ``Log(message.ToString())``.  
 - ``public void Log(object message, LogSeverity severity)``: shorthand for ``Log(message.ToString(), severity)``.  
 - ``protected void Fire(LogEventArgs args)``: calls the correct event delegates: ``OnLog`` in any case, and one of the four others depending on the log severity in the arguments.  

### Virtual Methods
 - ``protected virtual void OnLog(LogSeverity args)``: invokes the ``LogEvent`` delegates if not empty.  
 - ``protected virtual void OnLogWarning(LogSeverity args)``: invokes the ``LogWarningEvent`` delegates if not empty.  
 - ``protected virtual void OnLogMessage(LogSeverity args)``: invokes the ``LogMessageEvent`` delegates if not empty.  
 - ``protected virtual void OnLogError(LogSeverity args)``: invokes the ``LogErrorEvent`` delegates if not empty.  
 - ``protected virtual void OnLogDebug(LogSeverity args)``: invokes the ``LogDebugEvent`` delegates if not empty.  

### Abstract Methods
 - ``public abstract void CascadeLog(string message)``: perform the actual logging of a message.  
 - ``public abstract void CascadeLog(string message, LogSeverity severity)``: perform actual logging of a message, with a given severity.  

## Class Jay.IO.Logging.LogEventArgs
*Event data for Diagnostic Log Events.*  
Inheritance: [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object?view=net-5.0) -> [EventArgs](https://docs.microsoft.com/en-us/dotnet/api/system.eventargs?view=net-5.0) -> [LogEventArgs](./IEventLogger.md).  

### Fields
 - ``public LogSeverity Severity``: the severity of the message logged.  
 - ``public string Message``: the message that has been logged (or will be logged, if fired before logging).  
 - ``public DateTime Timestamp``: the moment the message was to be logged (can differ if events are fired before logging).  

### Constructors
 - ``public LogEventArgs(LogSeverity severity, string message)``: creates a new LogEventArgs instance with the given severity, message and the current time.
