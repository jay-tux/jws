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

### `JWS.Paths.HTML` and `JWS.Paths.Error`
There are two constants you can use in these variables: `@Home` for the home directory and `@Data` for the data directory (`/usr/locl/etc/jws` on *nix and `%APPDATA%/jws` on Windows).  

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
