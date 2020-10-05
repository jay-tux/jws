# JWS Configuration - Advanced Guide

## Variables, Blocks and Lists
 - `JWS`: the configuration root.  
   - `JWS.Paths`: the block containing data paths.  
     - `JWS.Paths.HTML`: the server file directory (which files are to be served).  
     - `JWS.Paths.Error`: the error file directory (each `*.html` file here represents a single error/status code).  
   - `JWS.Server`: the block containing some (misc) server configurations.  
     - `JWS.Server.Root`: the server root. This file is searched for (in `JWS.Paths.HTML`) when the root (`/`) URL is requested.  
     - `JWS.Server.Name`: the server name. This setting is used when unknown errors (errors without corresponding file) occur and in the `Server: ` HTTP header.  
     - `JWS.Server.FileAssoc`: the file association dictionary. Each entry in this block is written as `[file extension]: [MIME type]`.  
   - `JWS.Boot`: the block containing startup instructions (currently not used).  
     - `JWS.Boot.Commands`: currently not used.  
   - `JWS.Listener`: the block containing listener initialization settings.  
     - `JWS.Listener.State`: the current state for the Listener. Default only `Debug` and `Production` are supported, but you can define your own states as well. (See further)  
     - `JWS.Listener.[StateName]` (in which `[StateName]` represents a state the Listener can take): the blocks containing state variables.  
       - `JWS.Listener.[StateName].Routing`: the routing policy: see further (either `stay` or `302`).  
       - `JWS.Listener.[StateName].Port`: the port to which the listener should listen when in this state.  
       - `JWS.Listener.[StateName].Prefixes`: the prefixes the listener should server when in this state. (See further)  
   - `JWS.Statusses`: the block containing HTTP status descriptions, the defaults are pulled from [MDN](https://developer.mozilla.org/en-US/docs/Web/HTTP/Status). Each entry in this block is written as `[status code]: [description]`.  
   - `JWS.Services`: the block containing dynamic services (currently not used).  
     - `JWS.Services.Internals`: currently not used.  
     - `JWS.Services.Externals`: currently not used.  
   - `JWS.Routing`: the block containing re-routing URLS. Each entry in the block is written as `[URL]: [file]`.  
   - `JWS.Logging`: the block containing logger settings.  
     - `JWS.Logging.Type`: the logger type to use. Is either Channel (use multiple channels based on the message type) or Simple (log everything to `stdout`).  
     - `JWS.Logging.Formatting`: the logger formatting system, contains data about colors, lengths, string formats, ...  
       - `JWS.Logging.Formatting.Color`: if on, uses (ANSI) color escape codes (no Windows support currently), otherwise off.  
       - `JWS.Logging.Formatting.MaxType`: the max length for the message type (used for aesthetically even message formats).  
       - `JWS.Logging.Formatting.MaxMod`: the max length for the module name throwing the message (used for aesthetically even message formats).  
       - `JWS.Logging.Formatting.Colors`: the ANSI color codes to use (`\u001b[` is automatically prepended).
         - `JWS.Logging.Formatting.Colors.Debug`: the debug message color.  
         - `JWS.Logging.Formatting.Colors.Message`: the normal message color.  
         - `JWS.Logging.Formatting.Colors.Warning`: the warning message color.  
         - `JWS.Logging.Formatting.Colors.Error`: the error message color.  
       - `JWS.Logging.Formatting.DTFormatting`: the C# DateTime formatting string to use.  
       - `JWS.Logging.Formatting.Formatting`: the format string for all messages.  
   All other blocks in the `JWS.Logging` block are used to define Channel logger behavior.  

### `JWS.Paths.HTML` and `JWS.Paths.Error`
There are two constants you can use in these variables: `@Home` for the home directory and `@Data` for the data directory (`/usr/local/etc/jws` on *nix and `%APPDATA%/jws` on Windows).  

### Prefixes
Prefixes are of the form `protocol://domain`, where `protocol` can be any of `http` or `https`.  
`Domain` can have one of the following forms:  
 - `sub.domain.ext`, where `sub` can be a wildcard (`*`) or a defined subdomain;  
 - `domain.ext`,  
 - `*`, any domain (not recommended!).  

### State Routing (`JWS.Listener.[StateName].Routing`)
This setting determines the routing policy and has two possible options. This setting applies to both the `JWS.Routing` part and the `JWS.Server.Root` part.  
 - `stay`: when routing (re-routing), the corresponding page is sent as if it were at the URL's name,  
 - `302`: the response contains a `Location: [re-routed page]` header and has the `HTTP 302 Found` status code. In this case, the browser will do a second request to the re-routed page.  

Recommended settings for prefixes are:  
State\SSL | With SSL | Without SSL  
--- | --- | ---  
Debug | `http://localhost` | `https://localhost`  
Production | `http://*.yourdomain.ext` | `https://*.yourdomain.ext`  

## Defining Your Own States
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

## Logging - the Channel Logger
Currently the Channel logger (which is activated by setting `JWS.Logging.Type` to `Channel`) uses per-type channels, that is, uses channels for each of the message types (Debug, Message, Warning and Error) and requires a block in the `JWS.Logging` block of the corresponding type for each type. These blocks have a `State` field (which is either `on` or `off`, both values are self-explanatory) and a `Stream` list. Each entry in the `Stream` list should contain a single key, `Value`, representing the stream to write to. A stream is either `stdout` (the standard output stream), `stderr` (the standard error stream) or a file path. The path is relative to the Data directory (`/usr/local/etc/jws` or `%APPDATA%/jws`) and should start with a forward slash (`/`). If you refer to a non-existent file, the system will attempt to create it. An example channel definition is included:  
```
Message: {
    State: on
    Stream: [
        {
            Value: stdout
        }
        {
            Value: /message.log
        }
    ]
}
```
This definition will write Messages to both `stdout` and `/usr/local/etc/jws/message.log`.  

## Logging - the Formatting
There are a few things to be said about formatting: width-based fields, DateTime formatting and variables.  
### Width-Based Fields
For the message type and module name fields in a log message, there is a setting (respectively `JWS.Logging.Formatting.MaxType` and `JWS.Logging.Formatting.MaxMod`) which describes the maximal length for that field. Note that this will be always the length of said field. For both fields, if a value is shorter than the given amount of characters, it is padded with spaces. If the value is longer however, the resulting string will contain `maxlength - 3` characters of the original string, as well as an ellipsis (`...`). Padding is field dependent: the type field is padded to the right (so spaces are appended) and the module field is padded to the left (so the spaces are prepended).  
### DateTime Formatting
The value in `JWS.Logging.Formatting.DTFormatting` is a C# DateTime format string. For all info, see the [MSDN page on DateTime Formatting](https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings). Below is a small summary of fields often used:  
Characters | Meaning  
--- | ---  
`yyyy` | Year in 4 digits  
`yy` | Year in 2 digits (00->99)  
`MM` | Month in 2 digits (01->12)  
`MMM` | Abbreviated month name  
`dd` | Number of the day (01->31)  
`HH` | Hour in 2 digits (24 hour) (01->24)  
`hh` | Hour in 2 digits (12 hour) (01->12)  
`mm` | Minute in 2 digits (00->59)  
`ss` | Second in 2 digits (00->59)  
`fff` | Milliseconds in 3 digits (000->999)  
`ffffff` | Highest precision second parts in 6 digits (000000->999999)  
`tt` | AM or PM  
### Variables
In the `JWS.Logging.Formatting.Formatting` fields, these variables are automatically changed to the corresponding valuse:  
Variable | Extrapolated to  
--- | ---  
`@TYPE` | The message type in uppercase; any of DEBUG, MESSAGE, WARNING or ERROR (padded to `JWS.Logging.Formatting.MaxType` characters)  
`@MOD` | The module throwing the message (padded to `JWS.Logging.Formatting.MaxMod` characters)  
`@TIME` | The date and time on which the message was logged, formatted as described using `JWS.Logging.Formatting.DTFormatting`  
`@MSG` | The actual message  
Each formatted message is (if the `JWS.Logging.Formatting.Color` is `on`) prepended with `\u001b[` plus corresponding color code and appended with `\u001b[0m` (for coloration). This cannot be changed (i.e. you cannot have only the metadata be colored, ...).
