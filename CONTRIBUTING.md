# Contributing to JWS
## Bug Reports
Have you found a bug in JWS? Great! We'll fix it as soon as we can!  
### You Can Fix It
Can you fix the issue yourself in the code? Great! In that case, please feel free to fork JWS, fix the code and create a pull request using the template.
### You Can't Fix It
If you can't fix the issue in the code or you don't have time, we'll do our best to get it fixed as soon as we can.
Please file a bug report (an issue using the bug template) and we'll start working on it.

## Feature Requests
Is there something you miss in JWS? Let us know and we'll add it as soon as we can. Feel free to implement it yourself and submit a pull request when done. If you don't have the time
to do so, please refer to the opened issues. Leave a comment on an issue if it describes approximately what you want, or create a new one if you don't find what you like. New feature
request should be created with the feature request template.

## Pull Requests
Did you fix something? Did you add something? Did you rewrite our documentation? Great! We'll be sure to check what you made of it. Please use the Pull Request template when creating
a pull request. Please also refer to the File Tree and the Style Guide, so our code remains structured as it is. Thank you for contributing!

### File Tree
The repository contains lots of source files in a structured manner; below is a listing of which directories serve what purpose:  
 - `/jws/` is the root directory and should only contain the `.gitignore`, the guides, the community files (license, contributing guide, code-of-conduct, ...)
and the `jws.csproj` compilation unit.  
 - `/jws/JWS/` is for the general JWS files (the entry point, the listener, ...) or JWS-specific implementations (JWSChannelLogger).  
 - `/jws/Libs/` is for general libraries (these files will be moved around once [cslibs](https://www.github.com/jay-tux/cslibs) project gets a little more concrete).  
 - `/jws/Logger/` is for the logger system.  

### Style Guide
There are many ways to writing clean and smooth C# code, but this is the style we use for JWS.
#### Variable, Method, Class and Interface Naming Scheme
 - Private fields: `_lowerCamelCase`  
 - Public fields and properties: `UpperCamelCase`  
 - Methods (all kinds): `UpperCamelCase`  
 - Classes: `UpperCamelCase`  
 - Interfaces: `IUpperCamelCase`  
#### Singleton Classes
A singleton class should contain a static field of its own type called `Instance`, and a `private` constructor. Either a specific static method creates the instance, or the instance
creates itself in its own declaration.
#### Initialization
For initialization, we prefer to use a simple constructor with the bare minimum arguments (often none at all), combined with a object initializer
(example from `/jws/JWS/Comms.cs`, lines 239-249):  
```cs
Response resp = new Response() {
    _finished = false,
    _target = r.Path,
    _error = false,
    StatusCode = 200,
    ContentEncoding = Encoding.UTF8,
    MIMEType = "text/html",
    Cookies = new CookieCollection(),
    Headers = new WebHeaderCollection(),
    Buffer = null
};
```
#### Methods
 - If a method is simple and used only once, please consider using a lambda (for example, with the Linq methods).  
 - If a method consists of one line, please consider using an expression-bodied method.
