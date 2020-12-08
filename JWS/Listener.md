# JWS - JWS/Listener.cs
*This is the documentation about the source file located under `JWS/Listener.cs`; only public/protected fields/methods/classes are described in here.*

## Class Jay.Web.Server.Listener
*The main loop controller for JWS.*  
Inheritance: [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object?view=net-5.0) -> [Listener](./Listener.md).

### Fields
 - ``public static string HTMLDir``: the directory in which the HTML server files reside (the root directory for the server).  
 - ``public static string ErrorDir``: the directory in which the error pages reside.  
 - ``public static string ListenerState``: the current state of the Listener (unused).  
 - ``public static string ServerName``: the name of the server.  
 - ``public int Port``: the port on which to listen.  
 - ``public bool Stopped``: to stop the server.

### Constructors
 - ``public Listener()``: creates a new listener object.

### Methods
 - ``public void SetServerName()``: derives this server's name from the [Program](./Program.md) settings.  
 - ``public void LoadPaths()``: attempts to derive the paths for this server from the [Program](./Program.md) settings.  
 - ``public IEnumerable<(Request, Response)> Loop()``: the main loop; listens to any incoming requests and responds to them as described in the config file.  
