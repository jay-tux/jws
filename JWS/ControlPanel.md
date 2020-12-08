# JWS - JWS/ControlPanel.cs
*This is the documentation about the source file located under `JWS/ControlPanel.cs`; only public/protected fields/methods/classes are described in here.*

## Class Jay.Web.Server.ControlPanel
*Static class which controls the control panel (cpanel).*  
Inheritance: [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object?view=net-5.0) -> [ControlPanel](./ControlPanel.md).

### Static Methods
 - ``public static void Load()``: initializes/resets the control panel's values.
 - ``public sttic void GenerateCPanel(Request req, Response res)``: [Response Hook](./Comms.md) which generates the control panel page, using POST values in the Request, when requested.  
