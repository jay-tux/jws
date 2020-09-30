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

## How to Configure (easy mode)
There are two locations where JWS will look for the configuration file: `$HOME/.config/jws/jws.jcf` (or a similar location, depending on OS versions.) and
`/usr/local/etc/jws/jws.jcf` (or `%APPDATA/jws/jws.jcf` on Windows), so I recommend copying the default config file (`config/jws.jcf`) to one of those locations.  

### Some Configuration variables
*If the config file looks like gibberish to you, take a look at [Reading the Configuration File](../../blob/master/jcf.md)*  
*A dot (.) is used in this description to move into a block.*  
`JWS.Paths.HTML`: this is the directory in which your files-to-be-served will reside; default is `$HOME/.config/jws/html`.  
`JWS.Paths.Error`: this is the directory in which the error pages will reside (will be used for page not found, ...); default is `$HOME/.config/jws/error`.  
`JWS.Server.Root`: this is the file (in the HTML directory) representing the home page (the page at URL `/`); default is `/index.html`.  
`JWS.Server.Name`: this represents the name of the server (currently only used for vanity & in the case of unknown errors).  
`JWS.Listener.State`: this represents the current state in which the server boots (you can define your own states); default is `Debug`.  
`JWS.Listener.Debug.Port` (analogous for other states): this is the port the server will listen to.  

For the `JWS.Paths.HTML` and `JWS.Paths.Error`, there are two constants:  
 - `@Home` for your own home directory (`/home/username` on *nix and `C:\Users\username` on Windows)  
 - `@Data` for the data directory (`/usr/local/etc/jws` on *nix and `%APPDATA%/jws` on Windows)  

For `JWS.Paths.Error`: whenever an error occurs (with status code `code`), the file `[JWS.Paths.Error]/[code].html` is searched for.

## How to Configure (hard mode)
*Down here, all variables, blocks and lists are listed with their meaning*
 - `JWS`: the configuration root.  
   - `JWS.Paths`: the block containing data paths.  
     - `JWS.Paths.HTML`: the server file directory (which files are to be served).  
     - `JWS.Paths.Error`: the error file directory (each `*.html` file here represents a single error/status code).  
   - `JWS.Server`: the block containing some (misc) server configurations.  
     - `JWS.Server.Root`: the server root. This file is searched for (in `JWS.Paths.HTML`) when the root (`/`) URL is requested.  
     - `JWS.Server.Name`: the server name. This setting won't be used if all errors have their file in `JWS.Paths.Error`.  
     - `JWS.Server.FileAssoc`: the file association dictionary. Each entry in this block is written as `[file extension]: [MIME type]`.  
   - `JWS.Boot`: the block containing startup instructions (currently not used).  
     - `JWS.Boot.Commands`: currently not used.  
   - `JWS.Listener`: the block containing listener initialization settings.  
     - `JWS.Listener.State`: the current state for the Listener. Default only `Debug` and `Production` are supported, but you can define your own states as well. (See further)  
     - `JWS.Listener.[StateName]` (in which `[StateName]` represents a state the Listener can take): the blocks containing state variables.  
       - `JWS.Listener.[StateName].Port`: the port to which the listener should listen when in this state.  
       - `JWS.Listener.[StateName].Prefixes`: the prefixes the listener should server when in this state. (See further)  
   - `JWS.Statusses`: the block containing HTTP status descriptions, the defaults are pulled from [MDN](https://developer.mozilla.org/en-US/docs/Web/HTTP/Status). Each entry in this block is written as `[status code]: [description]`.  
   - `JWS.Services`: the block containing dynamic services (currently not used).  
     - `JWS.Services.Internals`: currently not used.  
     - `JWS.Services.Externals`: currently not used.  
   - `JWS.Routing`: the block containing re-routing URLS (currently not used).

*Notes on `JWS.Paths.HTML` and `JWS.Paths.Error`*  
There are two constants you can use in these variables: `@Home` for the home directory and `@Data` for the data directory (`/usr/locl/etc/jws` on *nix and `%APPDATA%/jws` on Windows).  

*Notes on Prefixes*  
Prefixes are of the form `protocol://domain`, where `protocol` can be any of `http` or `https`.  
`Domain` can have one of the following forms:  
 - `sub.domain.ext`, where `sub` can be a wildcard (`*`) or a defined subdomain;  
 - `domain.ext`,  
 - `*`, any domain (not recommended!).  
Recommended settings for prefixes are:  
| State\SSL | With SSL | Without SSL |
|---|---|---|
| Debug | http://localhost | https://localhost |
| Production | http://*.yourdomain.ext | https://*.yourdomain.ext |

### Defining Your Own States
All states are defined like this (always in `JWS.Listener`):
```
[State name]: {
    Port: [Port no]
    Prefixes: [
        {
            Entry: [Prefix]
        }
    ]
}
```  
A state can, of course, contain multiple prefixes, and every defined state can be used as value in the `JWS.Listener.State` variable.
