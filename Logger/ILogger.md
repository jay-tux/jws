# JWS - Logger/ILogger.cs
*This is the documentation about the source file located under `Logger/ILogger.cs`; only public/protected fields/methods/classes are described in here.*

## Interface Jay.IO.Logging.ILogger
*Base interface for logger types.*  

### Methods
 - ``void Log(string message)``: logs a simple message with the default LogSeverity (Message).  
 - ``void Log(object message)``: logs an object (usually using the default ``ToString`` method) with the default LogSeverity (Message).  
 - ``void Log(string message, LogSeverity severity)``: logs a message with the given severity.  
 - ``void Log(object message, LogSeverity severity)``: logs an object (usually using the default ``ToString`` method) with the given severity.

## Static Class Jay.IO.Logging.LogFormat
*Class containing extension methods for formatting, as well as convenience methods.*  
Inheritance: [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object?view=net-5.0) -> [LogFormat](./ILogger.md).  

### Static Fields
 - ``public static bool Colors``: use ANSI escape codes for colors.  
 - ``public static int MaxTypeLength``: the maximal length for the log severity (if used as format field).  
 - ``public static int MaxModLength``: the maximal length for the module name (if used as format field).  
 - ``public static string DateTimeFmt``: [DateTime](https://docs.microsoft.com/en-us/dotnet/api/system.datetime?view=net-5.0) formatting string (see [Custom Date and Time Format Strings](https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings)).  
 - ``public static string ColorError``: ANSI escape code to be used when logging errors.  
 - ``public static string ColorWarning``: ANSI escape code to be used when logging warnings.  
 - ``public static string ColorMessage``: ANSI escape code to be used when logging messages.  
 - ``public static string ColorDebug``: ANSI escape code to be used when logging debug messages.  
 - ``public static string ColorReset``: ANSI escape code for resetting the terminal's color.

### Static Methods
 - ``public static string FormatString(string source, string message, LogSeverity severity)``: returns a string formatted as described in the ``Formatting`` field, using ``source`` as module name, ``message`` as log message content and ``severity`` as logging severity.  

### Extension Methods
 - ``public void ILogger.LogFormatted(string source, string message, LogSeverity severity, DateTime tm)``: logs a message given at a certain time (for delayed or early logging) in a formatted manner.  
 - ``public void ILogger.LogFormatted(string source, string message, LogSeverity severity)``: logs a message in a formatted manner.

### Formatting
There are four fields you can use which are substituted in the formatting string. These are substituted in order, so if the one of the previous (data) strings contains a later flag, the flag is also substituted in the (data) string. The available flags are (in order):  
 - ``@TYPE``: for the log severity (in uppercase, left aligned to ``MaxTypeLength`` characters, or shortened with an ellipsis (``...``)),  
 - ``@MOD``: for the module name (right aligned to ``MaxModLength`` characters, or shortened with an ellipsis (``...``)),  
 - ``@TIME``: for the date and time of logging, formatted as described in ``DateTimeFmt``,  
 - ``@MSG``: for the actual message content.

## Enum Jay.IO.Logging.LogSeverity
*Enum type containing the possible severities for a log.*  
 - ``Debug``, for debug messages;  
 - ``Message``, for normal messages;  
 - ``Warning``, for warnings;  
 - ``Error``, for errors.  

## Class Jay.IO.Logging.SimpleLogger
*Simple default singleton implementation of the ILogger interface.*  
Inheritance: [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object?view=net-5.0) -> [SimpleLogger](./ILogger.md).  
Implements: [ILogger](./ILogger.md).  

### Static Fields
 - ``public static SimpleLogger Instance``: the singleton instance for the SimpleLogger.  

### Methods
 - ``public void Log(string message, LogSeverity sev)``: writes ``[uppercase sev]\t[message]`` to stdout.  
 - ``public void Log(string message)``: shorthand for ``Log(message, LogSeverity.Message)``.  
 - ``public void Log(object message)``: shorthand for ``Log(message.ToString(), LogSeverity.Message)``.  
 - ``public void Log(object message, LogSeverity sev)``: shorthand for ``Log(message.ToString(), sev)``.
