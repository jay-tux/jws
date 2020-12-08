# JWS - Source Code Documentation
*This file is mainly used for redirection*

## Types by Namespace
 - Namespace Jay.Web.Server  
   - Class [ControlPanel](JWS/ControlPanel.md): Static class which controls the control panel (cpanel).  
   - Class [JWSChannelLogger](JWS/JWSChannelLogger.md): The default channel-based logger for JWS.  
   - Class [Listener](JWS/Listener.md): The main loop controller for JWS.  
   - Class [Program](JWS/Program.md): The default entry point, along with some convenience functions. Is a singleton class.  
   - Class [Request](JWS/Comms.md): A class to access request headers, cookies and other data.  
   - Class [Response](JWS/Comms.md): A class to prepare a response.  
   - Class [Templating](JWS/Templating.md): Static class which handles the templating (if enabled).  
 - Namespace Jay.Config  
   - Class [Jcf](Libs/Conf.md): The tree-like data structure for the configuration data.  
     - Enum [JcfType](Libs/Conf.md): Is either Value (a single value), Jcf (a sub-block) or List (a JCF list)  
   - Class [JcfException](Libs/Conf.md): Exception class for representing parsing errors.  
   - Class [JcfParser](Libs/Conf.md): A static class for parsing JCF files.  
 - Namespace Jay.Ext  
   - Class [Ext](Libs/Ext.md): A static class containing multiple extension methods.  
 - Namespace Jay.IO.Logging  
   - Class [DefaultEventLogger](Logging/IEventLogger.md): Base class for creating RP loggers.  
   - Class [DiagnosticEventLogger](Logging/DiagnosticEventLogger.md): Base class for creating RP loggers which keep stack-trace data.  
   - Class [DiagnosticLogEventArgs](Logging/DiagnosticEventLogger.md): Event data for Diagnostic Log Events.  
   - Interface [IChannelLogger](Logging/IChannelLogger.md): Interface for loggers which use different channels.  
   - Interface [IEventLogger](Logging/IEventLogger.md): Logger interface with 5 events, can be used to hook multiple loggers together.  
   - Interface [ILogger](Logging/ILogger.md): Base interface for logger types.  
   - Class [LogEventArgs](Logging/IEventLogger.md): Event data for Log Events.  
   - Class [LogFormat](Logging/ILogger.md): Class containing extension methods for formatting, as well as convenience methods.  
   - Enum [LogSeverity](Logging/ILogger.md): Enum type containing the possible severities for a log.  
   - Class [SimpleChannelLogger](Logging/IChannelLogger.md): Singleton default implementation for the IChannelLogger, without predicates.  
   - Class [SimpleLogger](Logging/ILogger.md): Simple default singleton implementation of the ILogger interface.
   - Class [SimplePredicateLogger](Logging/IChannelLogger.md): Singleton predicate-based implementation for the IChannelLogger, using stdout (for debug and message severities) and stderr (for warning and error severities).  
