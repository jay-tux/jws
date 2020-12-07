# JWS - JWS/Program.cs
*This is the documentation about the source file located under `JWS/Program.cs`; only public/protected fields/methods/classes are described in here.*

## Class Jay.Web.Server.Program
*The default entry point, along with some convenience functions. Is a singleton class.*  
Inheritance: [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object?view=net-5.0) -> [JWSChannelLogger](.).

### Fields
 - ``public event EventHandler OnExit``: controls the exit hooks.

### Static Fields
 - ``public static Program Instance``: the singleton instance.  
 - ``public static string ConfigContent``: the configuration file as read from the filesystem.

### Static Properties
 - ``public static ILogger Logger``: the [ILogger](../Logger/ILogger.md) used to log all messages.  
 - ``public static Jcf Settings``: the [JCF settings](../Libs/Conf.md) loaded from the config file/CLI arguments.  

### Methods
 - ``public void Exit(int code)``: causes a clean exit; runs exit hooks, cleans up, and exits with the given code.  

### Static Methods
 - ``public static void Main(string[] args)``: entry point for the application. Loads the CLI arguments, loads the config files and starts the main loop.  
 - ``public static string GetHome()``: gets the home directory. This defaults to ``$HOME`` on *nix and ``%HOMEDRIVE%%HOMEPATH%`` on Windows.  
 - ``public static string Data()``: gets the default data path. This defaults to ``/usr/local/etc`` on *nix and ``%APPDATA%`` on Windows.
