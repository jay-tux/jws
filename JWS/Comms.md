# JWS - JWS/Comms.cs
*This is the documentation about the source file located under `JWS/Comms.cs`; only public/protected fields/methods/classes are described in here.*

## Class Jay.Web.Server.Request
*A class to access request headers, cookies and other data.*  
Inheritance: [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object?view=net-5.0) -> [Request](.).

### Fields
 - ``public Encoding ContentEncoding``: the [System.Text.Encoding](https://docs.microsoft.com/en-us/dotnet/api/system.text.encoding?view=net-5.0) used in the request.  
 - ``public long ContentLength``: the length of the request content.  
 - ``public string MIMEType``: the type of content, see [MDN - MIME types](https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/MIME_types)  
 - ``public CookieCollection Cookies``: contains all [Cookies](https://docs.microsoft.com/en-us/dotnet/api/system.net.cookie?view=net-5.0) sent with the request, see [System.Net.CookieCollection](https://docs.microsoft.com/en-us/dotnet/api/system.net.cookiecollection?view=net-5.0)  
 - ``public NameValueCollection Headers``: contains all [HTTP Headers](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers) sent in the Request; see [System.Collections.Specialized.NameValueCollection](https://docs.microsoft.com/en-us/dotnet/api/system.collections.specialized.namevaluecollection?view=net-5.0).  
 - ``public string Method``: contains the HTTP Verb the request used (GET, POST, PUT, ...)  
 - ``public string Path``: the relative path the request queries (starting with ``/``, from the server root).  
 - ``public NameValueCollection Queries``: the GET queries included in the request, see [System.Collections.Specialize.NameValueCollection](https://docs.microsoft.com/en-us/dotnet/api/system.collections.specialized.namevaluecollection?view=net-5.0).  
 - ``public string Content``: the body of the request.  
 - ``public Dictionary<string, string> POST``: a [System.Collections.Generic.Dictionary](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2?view=net-5.0) containing all POST keys and values. Can be empty, is never ``null``.  

### Constructors
 - ``public Request(HttpListenerRequest r)``: read the data in the [System.Net.HttpListenerRequest](https://docs.microsoft.com/en-us/dotnet/api/system.net.httplistenerrequest?view=net-5.0) and converts it to a ``Request``.

### Methods
 - ``public override string ToString()``: converts the ``Request`` to a human-readable version.

## Class Jay.Web.Server.Response
*A class to prepare a response.*  
Inheritance: [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object?view=net-5.0) -> [Response](.).

### Fields
 - ``public Encoding ContentEncoding``: the [System.Text.Encoding](https://docs.microsoft.com/en-us/dotnet/api/system.text.encoding?view=net-5.0) used in the request.  
 - ``public long ContentLength``: the length of the request content.  
 - ``public string MIMEType``: the type of content, see [MDN - MIME types](https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/MIME_types)  
 - ``public CookieCollection Cookies``: contains all [Cookies](https://docs.microsoft.com/en-us/dotnet/api/system.net.cookie?view=net-5.0) sent with the request, see [System.Net.CookieCollection](https://docs.microsoft.com/en-us/dotnet/api/system.net.cookiecollection?view=net-5.0)  
 - ``public WebHeaderCollection Headers``: contains all [HTTP Headers](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers) sent in the Request; see [System.Net.WebHeaderCollection](https://docs.microsoft.com/en-us/dotnet/api/system.net.webheadercollection?view=net-5.0).  
 - ``public int StatusCode``: the [HTTP status code](https://developer.mozilla.org/en-US/docs/Web/HTTP/Status). Description is read from the config file.  

### Properties
 - ``public string Content``: gets or sets the content. Content is stored as a ``byte[]`` internally and is converted each time using [System.Text.Encoding.UTF8.GetBytes](https://docs.microsoft.com/en-us/dotnet/api/system.text.encoding.getbytes?view=net-5.0) and [System.Text.Encoding.UTF8.GetString](https://docs.microsoft.com/en-us/dotnet/api/system.text.encoding.getstring?view=net-5.0).  
 - ``public string? this[HttpResponseHeader]``: gets or sets a header. Is a shortcut to the [corresponding property](https://docs.microsoft.com/en-us/dotnet/api/system.net.webheadercollection.item?view=net-5.0#System_Net_WebHeaderCollection_Item_System_Net_HttpResponseHeader_) on the ``Headers`` field.  

### Methods
 - ``public string LoadError(int code)``: loads the file corresponding to the given error code to the buffer.  
 - ``public void Finish()``: marks a request as finished (skips all following hooks).  
 - ``public void Write(HttpListenerResponse resp)``: finishes the response by copying all required data to the output [System.Net.HttpListenerResponse](https://docs.microsoft.com/en-us/dotnet/api/system.net.httplistenerresponse?view=net-5.0). Writes the content buffer to the output stream in the HttpListenerResponse.
 - ``public override string ToString()``: converts the Response to a human-readable string.

### Static Methods
 - ``public static Response ToRequest(Request r)``: constructs a default response to a given request.
 - ``public static void Hook(Func<Request, Response, bool> filter, Action<Request, Response> consumer)``: creates a new hook.

### Response Hooks
A hook contains of a predicate (of type ``Func<Request, Response, bool>``; a function which takes a Request and a Response, and returns a bool; similar to a ``bool function(Request rq, Response rs)``) and an action (of type ``Action<Request, Response>``; a function which takes a Request and a Response, and returns nothing; similar to a ``void function(Request rq, Response rs)``).  
Every hook is checked (while a Response is not yet marked as finished), and if the predicate returns true, the action is executed. It is recommended to mark a Response as finished when a hook is executed over it. This is done to prevent overriding and for optimization: all following hooks are skipped.  
**The Calling Function: ``Response::ToRequest``**
The hooks are executed when the ``public static Response ToRequest(Request r)`` is called; this method does:  
 - Initialize default values,  
 - Attempts routing, in the following order (routing finishes requests if they are found):  
   - First, root-routing: the `/`-path,  
   - Second, table-routing: searches the settings for a route corresponding to this path, and  
   - Last, literal-routing: checks if the file request exists in the filesystem.
 - Runs the hooks, until finished,  
 - Sets the content length (happens only once),  
 - Returns the constructed request.  
This method is usually called from the main loop and shouldn't be called from anywhere else.
