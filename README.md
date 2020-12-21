# JWS
### Jay's Web Server

## How to Run
*To run JWS, you either need Mono (Mac, Linux) or the .NET Framework (Windows).*  
Run/compile instructions are given for `bash`+Mono, but are analogous for other systems:
```sh
git clone https://github.com/jay-tux/jws
cd jws
msbuild
mono bin/Debug/netcoreapp3.1/jws.dll
```
However, running JWS like this will cause a crash with the error message
```
[ERROR      in   _driver - 09-30-20 11:56:06 AM]: Failed to find any configuration file.
```

## How to Configure
There are two locations where JWS will look for the configuration file: `$HOME/.config/jws/jws.jcf` (or a similar location, depending on OS versions.) and
`/usr/local/etc/jws/jws.jcf` (or `%APPDATA/jws/jws.jcf` on Windows), so I recommend copying the default config file (`config/jws.jcf`) to one of those locations.  
*This is the easy configuration guide, for a more in-depth guide, see [In-depth Configuration](../../blob/master/advanced.md)*  
*For the guide on templating, see [JWS Templates](../../blob/master/templates.md)*  
*For the guide on the builtin control panel, see [JWS Default Control Panel](../../blob/master/cpanel.md)*  
*For the source code documentation, see [Source Code Documentation](./source.md)*

### Some Configuration variables
*If the config file looks like gibberish to you, take a look at [Reading the Configuration File](../../blob/master/jcf.md)*  
*A dot (.) is used in this description to move into a block.*  
`JWS.Paths.HTML`: this is the directory in which your files-to-be-served will reside; default is `$HOME/.config/jws/html`.  
`JWS.Paths.Error`: this is the directory in which the error pages will reside (will be used for page not found, ...); default is `$HOME/.config/jws/error`.  
`JWS.Server.Root`: this is the file (in the HTML directory) representing the home page (the page at URL `/`); default is `/index.html`.  
`JWS.Server.Name`: this represents the name of the server.  
`JWS.Listener.State`: this represents the current state in which the server boots (you can define your own states); default is `Debug`.  
`JWS.Listener.Debug.Port` (analogous for other states): this is the port the server will listen to.  

For the `JWS.Paths.HTML` and `JWS.Paths.Error`, there are two constants:  
 - `@Home` for your own home directory (`/home/username` on *nix and `C:\Users\username` on Windows)  
 - `@Data` for the data directory (`/usr/local/etc/jws` on *nix and `%APPDATA%/jws` on Windows)  

For `JWS.Paths.Error`: whenever an error occurs (with status code `code`), the file `[JWS.Paths.Error]/[code].html` is searched for.

### Overriding Settings
You can use command-line arguments to override the settings or even the setting file loading:  
**Arguments:**  
 - `--config <configuration file>`: points the config loader to `<configuration file>` instead of the default locations,  
 - `--port <port no>`: uses `<port no>` rather than `JWS.Listener.<State>.Port`,  
 - `--state <state>`: uses `<state>` rather than `JWS.Listener.State`,  
 - `--html <directory>`: uses `<directory>` rather than `JWS.Paths.HTML`,  
 - `--error <directory>`: uses `<directory>` rather than `JWS.Paths.Error`,  
 - `--template <directory>`: uses `<directory>` rather than `JWS.Paths.Template`, and  
 - `--name <name>`: uses `<name>` rather than `JWS.Server.Name`.  

*Note when overriding settings:* the overridden settings are never saved and can't be seen when using the control panel.  

**Toggles:**
 - `--help`: displays a help message and exits,  
 - `--cwd`: uses `$PWD/html`, `$PWD/error` and `$PWD/template` as `JWS.Paths.HTML`, `JWS.Paths.Error` and `JWS.Paths.Template` respectively, and  
 - `--noconfig`: uses the fallback settings everywhere. Do not use this option as several things might break if you don't use the other arguments as well.

Alternatively, you can override any setting by using `--<setting path> <setting>`. However, you can only set single settings like this, not blocks or lists.
