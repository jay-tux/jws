# JWS - JWS/Templating.cs
*This is the documentation about the source file located under `JWS/Templating.cs`; only public/protected fields/methods/classes are described in here.*

## Class Jay.Web.Server.Templating
*Static class which handles the templating (if enabled).*  
Inheritance: [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object?view=net-5.0) -> [Templating](.).

## Static Fields
 - ``public static string MIMETemplate``: the MIME type corresponding to the template files (see the configuration settings).  
 - ``public static string MIMEData``: the MIME type corresponding to the data files (see the configuration settings).  
 - ``public static string TemplatePrefix``: the prefix which indicates variables in a template file.  
 - ``public static string TemplateDir``: the directory where template files are found.  
 - ``public static string Indexing``: the operator character used to index the ``GET`` and ``POST`` variables.

## Static Methods
 - ``public static void Load()``: loads all required data to enable/disable Templating from the settings.  
 - ``public static void Fill(Request got, Response send)``: [Response Hook](./Comms.md) to call when a request accesses a template data file.
