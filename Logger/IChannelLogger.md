# JWS - Logger/IChannelLogger.cs
*This is the documentation about the source file located under `Logger/IChannelLogger.cs`; only public/protected fields/methods/classes are described in here.*

## Interface Jay.IO.Logging.IChannelLogger
*Interface for loggers which use different channels.*  
Inheritance: [ILogger](./ILogger.md) -> [IChannelLogger](./IChannelLogger.md).

### Methods
 - ``void AddChannel(TextWriter target)``: adds a new channel to the logger.  
 - ``TextWriter GetChannel(int index)``: gets the index-th channel.  
 - ``void RemoveChannel(int index)``: removes the index-th channel.  
 - ``void SetDefault(int index)``: sets the default channel.  
 - ``TextWriter GetDefault()``: gets the default channel.  
 - ``void AddPredicate(Func<string, LogSeverity, bool> predicate, int index)``: add a predicate to the index-th channel.  
 - ``void RemovePredicates(int index)``: remove all predicates from channel index.  

**Note to implementers:**  
 - the predicates should be used to log to channels other than the default one if certain conditions are met.  
 - it is advisable to also implement [IDisposable](https://docs.microsoft.com/en-us/dotnet/api/system.idisposable?view=net-5.0), to clean up the [TextWriters](https://docs.microsoft.com/en-us/dotnet/api/system.io.textwriter?view=net-5.0).

## Class Jay.IO.Logging.SimpleChannelLogger
*Singleton default implementation for the IChannelLogger, without predicates.*  
Inheritance: [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object?view=net-5.0) -> [SimpleChannelLogger](./IChannelLogger.md).  
Implements: [IChannelLogger](./IChannelLogger.md).  

### Static Fields
 - ``public static SimpleChannelLogger Instance``: the singleton instance for SimpleChannelLogger.  

### Methods
 - ``public void AddChannel(TextWriter target)``: adds a new channel to the logger.  
 - ``public TextWriter GetChannel(int index)``: gets the index-th channel.  
 - ``public void RemoveChannel(int index)``: removes the index-th channel. *Throws an ArgumentException if index not in the range [0, amount of channels - 1] or if there's only one channel left.*  
 - ``public void SetDefault(int index)``: sets the default channel. *Throws an ArgumentException if index not in the range [0, amount of channels - 1].*  
 - ``public TextWriter GetDefault()``: gets the default channel.  
 - ``public void AddPredicate(Func<string, LogSeverity, bool> predicate, int index)``: **not implemented**.  
 - ``public void RemovePredicates(int index)``: **not implemented.**  
 - ``public void Log(string message, LogSeverity severity)``: logs the given message to the default channel, formatted as ``[uppercase severity]\t[message]``.  
 - ``public void Log(string message)``: shorthand for ``Log(message, LogSeverity.Message)``.  
 - ``public void Log(object message)``: shorthand for ``Log(message.ToString(), LogSeverity.Message)``.  
 - ``public void Log(object message, LogSeverity severity)``: shorthand for ``Log(message, severity)``.  

## Class Jay.IO.Logging.SimplePredicateLogger
*Singleton predicate-based implementation for the IChannelLogger, using stdout (for debug and message severities) and stderr (for warning and error severities).*  
Inheritance: [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object?view=net-5.0) -> [SimplePredicateLogger](./IChannelLogger.md).  
Implements: [IChannelLogger](./IChannelLogger.md).  

### Static Fields
 - ``public static SimplePredicateLogger Instance``: the singleton instance for SimplePredicateLogger.  

### Methods
- ``void AddChannel(TextWriter target)``: **not implemented.**  
- ``TextWriter GetChannel(int index)``: gets stdout (index 0) or stderr (index 1). *Throws an ArgumentException if index is not 0 or 1.*
- ``void RemoveChannel(int index)``: **not implemented.**  
- ``void SetDefault(int index)``: **not implemented.**  
- ``TextWriter GetDefault()``: **not implemented.**  
- ``void AddPredicate(Func<string, LogSeverity, bool> predicate, int index)``: add a predicate to stdout (index 0) or stderr (index 1). *Throws an ArgumentException if index is not 0 or 1.*  
- ``void RemovePredicates(int index)``: remove all predicates from stdout (index 0) or stderr (index 1). *Throws an ArgumentException if index is not 0 or 1.*  
- ``public void Log(string message, LogSeverity severity)``: logs the given message to stdout (if there are predicates matching the message) and/or stderr (if there are predicates matching the message), formatted as ``[uppercase severity]\t[message]``.  
- ``public void Log(string message)``: shorthand for ``Log(message, LogSeverity.Message)``.  
- ``public void Log(object message)``: shorthand for ``Log(message.ToString(), LogSeverity.Message)``.  
- ``public void Log(object message, LogSeverity severity)``: shorthand for ``Log(message, severity)``.  
