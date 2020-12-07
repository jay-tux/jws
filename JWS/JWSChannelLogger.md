# JWS - JWS/JWSChannelLogger.cs
*This is the documentation about the source file located under `JWS/JWSChannelLogger.cs`; only public/protected fields/methods/classes are described in here.*

## Class Jay.Web.Server.JWSChannelLogger
*The default channel-based logger for JWS.*  
Inheritance: [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object?view=net-5.0) -> [JWSChannelLogger](.).  
Implements: [ILogger](../Logger/ILogger.md), [IDisposable](https://docs.microsoft.com/en-us/dotnet/api/system.idisposable?view=net-5.0).

### Fields
 - ``public List<TextWriter> Channels``: the channels this logger will log to (this logger is similar to the [IChannelLogger](../Logger/IChannelLogger.md) but lacks the add/remove channel methods).  
 - ``public List<List<Func<string, LogSeverity, bool>>> Predicates``: the predicates describing which channels should get which logs.  

### Constructors
 - ``public JWSChannelLogger()``: creates a new, default logger object.  

### Methods
 - ``public void Log(string message, LogSeverity severity)``: logs a message to the channels satisfied by the message (using the Predicates).  
 - ``public void Log(string message)``: shorthand for ``Log(message, LogSeverity.Message)``.  
 - ``public void Log(object message)``: shorthand for ``Log(message.ToString(), LogSeverity.Message)``.  
 - ``public void Log(object message, LogSeverity severity)``: shorthand for ``Log(message.ToString(), severity)``.  
 - ``public void Dispose()``: cleans up the channels.  
 - ``protected virtual void Dispose(bool disp)``: does the actual cleaning up.  
 - ``~JWSChannelLogger()``: clean up; calls ``this.Dispose(false)``.
