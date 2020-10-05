using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Collections.Specialized;
using Jay.IO.Logging;

namespace Jay.Web.Server
{
    public class Request
    {
        public Encoding ContentEncoding;
        public long ContentLength;
        public string MIMEType;
        public CookieCollection Cookies;
        public NameValueCollection Headers;
        public string Method;
        public string Path;
        public NameValueCollection Queries;
        public string Content;

        public Request(HttpListenerRequest r)
        {
            ContentEncoding = r.ContentEncoding;
            ContentLength = r.ContentLength64;
            MIMEType = r.ContentType;
            Cookies = r.Cookies;
            Headers = r.Headers;
            Method = r.HttpMethod;
            Path = string.Join("", r.Url.Segments);
            Queries = r.QueryString;
            Content = (r.HasEntityBody) ? new StreamReader(r.InputStream, ContentEncoding).ReadToEnd() : "";
        }

        public override string ToString()
        {
            string cookies = "";
            for(int i = 0; i < Cookies.Count; i++)
            {
                cookies += $"\n\tCookie #{i}: {Cookies[i]}";
            }
            if(Cookies.Count == 0) cookies = "-- no cookies --";
            List<string> h = new List<string>();
            foreach(var key in Headers.AllKeys)
            {
                h.Add($"{key}: {Headers[key]}");
            }
            List<string> q = new List<string>();
            foreach(var key in Queries.AllKeys)
            {
                q.Add($"{key}: {Queries[key]}");
            }
            string headers = string.Join("\n\t", h);
            string queries = string.Join("\n\t", q);
            return $"Encoding: {ContentEncoding}\nMIME Type: {MIMEType}\nHTTP Method: {Method}\nQuery string: {Path}\nContent: '{Content}' (Length: {ContentLength})\n" +
                $"Headers: {headers}\n\nQueries: {queries}\nCookies: {cookies}";
        }
    }

    public class Response
    {
        private string _target;
        private bool _error;
        private bool _finished;
        public Encoding ContentEncoding;
        public long ContentLength;
        public string MIMEType;
        public CookieCollection Cookies;
        public WebHeaderCollection Headers;
        public int StatusCode;
        public byte[] Buffer;

        private Response() {}

        private string LoadError(int code)
        {
            string fallback = "<html><head><meta charset=\"utf-8\" /><title>" + Listener.ServerName + " - HTTP " + code.ToString() + "</title></head>"
                + "<body>The server has run into an HTTP " + code.ToString() + " status.</body>";

            if(File.Exists($"{Listener.ErrorDir}/{code}.html"))
            {
                try
                {
                    return File.ReadAllText($"{Listener.ErrorDir}/{code}.html");
                }
                catch(IOException)
                {
                    Program.Logger.LogFormatted("_resolver",
                        $"Tried to load existing error file {Listener.ErrorDir}/{code}.html, but couldn't read it. Returning generic error file.", LogSeverity.Warning);
                    //StaticLogger.LogWarning("_resolver", $"Tried to load existing error file {Listener.ErrorDir}/{code}.html, but couldn't read it. Returning generic error file.");
                    return fallback;
                }
            }
            else
            {
                Program.Logger.LogFormatted("_resolver", $"Tried to load non-existent error file {Listener.ErrorDir}/{code}.html. Returning generic error file.", LogSeverity.Warning);
                //StaticLogger.LogWarning("_resolver", $"Tried to load non-existent error file {Listener.ErrorDir}/{code}.html. Returning generic error file.");
                return fallback;
            }
        }

        private void AttemptRoute()
        {
            try
            {
                object rt = Program.Settings["JWS.Routing." + _target];
                string state = Listener.ListenerState;
                if(rt is string route)
                {
                    Program.Logger.LogFormatted("_resolver", $"Route JWS.Routing.{_target} resolved to {route}", LogSeverity.Debug);
                    //StaticLogger.LogDebug("_resolver", $"Route JWS.Routing.{_target} resolved to {route}");
                    _target = route;
                    try
                    {
                        object c = Program.Settings[$"JWS.Listener.{state}.Routing"];
                        if(c is string choice)
                        {
                            if(choice == "302")
                            {
                                StatusCode = 302;
                                Headers.Add(HttpResponseHeader.Location, _target);
                                Buffer = new byte[0];
                                _finished = true;
                            }
                            else if(choice != "stay") Program.Logger.LogFormatted("_resolver", $"Invalid value for JWS.Listener.{state}.Routing: {choice}. " +
                                "Valid options are 302 and stay. Using fallback stay.", LogSeverity.Warning);
                            //StaticLogger.LogWarning("_resolver", $"Invalid value for JWS.Listener.{state}.Routing: {choice}. " + "Valid options are 302 and stay. Using fallback stay.");
                        }
                        else
                        {
                            Program.Logger.LogFormatted("_resolver", $"JWS.Listener.{state}.Routing should be a string (either 302 or stay). Using fallback stay.", LogSeverity.Warning);
                            //StaticLogger.LogWarning("_resolver", $"JWS.Listener.{state}.Routing should be a string (either 302 or stay). Using fallback stay.");
                        }
                    }
                    catch(ArgumentException)
                    {
                        Program.Logger.LogFormatted("_resolver", $"Routing policy for {state} (JWS.Listener.{state}.Routing) not set (expecting either 302 or stay). Using fallback stay.",
                            LogSeverity.Warning);
                        //StaticLogger.LogWarning("_resolver", $"Routing policy for {state} (JWS.Listener.{state}.Routing) not set (expecting either 302 or stay). Using fallback stay.");
                    }
                }
                else
                {
                    Program.Logger.LogFormatted("_resolver", $"Route JWS.Routing.{_target} is not a string. Attempting to resolve URL as file...", LogSeverity.Warning);
                    //StaticLogger.LogWarning("_resolver", $"Route JWS.Routing.{_target} is not a string. Attempting to resolve URL as file...");
                }
            }
            catch(ArgumentException) {}
        }

        private void AttemptRootRoute()
        {
            if(_target == "/")
            {
                Program.Logger.LogFormatted("_resolver", "Client requested server root (/). Attempting to resolve...", LogSeverity.Debug);
                //StaticLogger.LogDebug("_resolver", "Client requested server root (/). Attempting to resolve...");
                try
                {
                    object rt = Program.Settings["JWS.Server.Root"];
                    if(rt is string root)
                    {
                        _target = root;
                    }
                    else
                    {
                        Program.Logger.LogFormatted("_resolver", "Server root (JWS.Server.Root) should be a string. Using fallback /index.html.", LogSeverity.Warning);
                        //StaticLogger.LogWarning("_resolver", "Server root (JWS.Server.Root) should be a string. Using fallback /index.html.");
                        _target = "/index.html";
                    }
                }
                catch(ArgumentException)
                {
                    Program.Logger.LogFormatted("_resolver", "Server root (JWS.Server.Root) is not configured. Using fallback /index.html.", LogSeverity.Warning);
                    //StaticLogger.LogWarning("_resolver", "Server root (JWS.Server.Root) is not configured. Using fallback /index.html.");
                    _target = "/index.html";
                }
            }
        }

        private void AttemptLiteral()
        {
            if(File.Exists(Listener.HTMLDir + _target))
            {
                try
                {
                    Buffer = Encoding.UTF8.GetBytes(File.ReadAllText(Listener.HTMLDir + _target));
                }
                catch(IOException)
                {
                    Program.Logger.LogFormatted("_resolver", $"Client requested {_target}; which resolved to existing file {Listener.HTMLDir + _target}, but couldn't be read." +
                        " Returning 403.", LogSeverity.Warning);
                    //StaticLogger.LogWarning("_resolver", $"Client requested {_target}; which resolved to existing file {Listener.HTMLDir + _target}, but couldn't be read." +
                    //    " Returning 403.");
                    StatusCode = 403;
                    Buffer = Encoding.UTF8.GetBytes(LoadError(StatusCode));
                    _error = true;
                }
            }
            else
            {
                Program.Logger.LogFormatted("_resolver", $"Client requested {_target}; which resolved to non-existent file {Listener.HTMLDir + _target}. Returning 404.",
                    LogSeverity.Message);
                //StaticLogger.LogMessage("_resolver", $"Client requested {_target}; which resolved to non-existent file {Listener.HTMLDir + _target}. Returning 404.");
                StatusCode = 404;
                Buffer = Encoding.UTF8.GetBytes(LoadError(404));
                _error = true;
            }

            if(!_error)
            {
                string ext = _target.Split('.').Last();
                try
                {
                    object m = Program.Settings["JWS.Server.FileAssoc." + ext];
                    if(m is string mi)
                    {
                        MIMEType = mi;
                    }
                    else
                    {
                        Program.Logger.LogFormatted("_resolver", $"File associations should be strings; not the case for JWS.Server.FileAssoc.{ext}. Using fallback text/html.",
                            LogSeverity.Warning);
                        //StaticLogger.LogWarning("_resolver", $"File associations should be strings; not the case for JWS.Server.FileAssoc.{ext}. Using fallback text/html.");
                    }
                }
                catch(ArgumentException)
                {
                    Program.Logger.LogFormatted("_resolver", $"File association *.{ext} not present. Configure as JWS.Server.FileAssoc.{ext}. Using fallback text/html.",
                        LogSeverity.Warning);
                    //StaticLogger.LogWarning("_resolver", $"File association *.{ext} not present. Configure as JWS.Server.FileAssoc.{ext}. Using fallback text/html.");
                }
            }
        }

        public static Response ToRequest(Request r)
        {
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
            resp.Headers.Add(HttpResponseHeader.Server, Listener.ServerName);

            resp.AttemptRootRoute();
            if(!resp._finished) resp.AttemptRoute();
            if(!resp._finished) resp.AttemptLiteral();
            resp.ContentLength = resp.Buffer.Length;

            return resp;
        }

        public void Write(HttpListenerResponse resp)
        {
            resp.ContentEncoding = ContentEncoding;
            resp.ContentLength64 = ContentLength;
            resp.ContentType = MIMEType;
            if(Cookies != null) resp.Cookies = Cookies;
            if(Headers != null) resp.Headers = Headers;
            resp.StatusCode = StatusCode;

            try
            {
                object s = Program.Settings["JWS.Statusses." + StatusCode.ToString()];
                if(s is string stat)
                {
                    resp.StatusDescription = stat;
                }
                else
                {
                    Program.Logger.LogFormatted("Comms", $"Status description for {StatusCode} should be string.", LogSeverity.Warning);
                    //StaticLogger.LogWarning(this, $"Status description for {StatusCode} should be string.");
                    resp.StatusDescription = "-- unknown status --";
                }
            }
            catch(ArgumentException)
            {
                Program.Logger.LogFormatted("Coms", $"Status description for {StatusCode} (JWS.Statusses.{StatusCode}) is undefined.", LogSeverity.Warning);
                //StaticLogger.LogWarning(this, $"Status description for {StatusCode} (JWS.Statusses.{StatusCode}) is undefined.");
                resp.StatusDescription = "-- unknown status --";
            }

            resp.OutputStream.Write(Buffer, 0, Buffer.Length);
            resp.OutputStream.Close();
        }

        public override string ToString()
        {
            string headers;
            string cookies;
            if(Cookies != null)
            {
                cookies = "";
                for(int i = 0; i < Cookies.Count; i++)
                {
                    cookies += $"\n\tCookie #{i}: {Cookies[i]}";
                }
            }
            else cookies = "-- no cookies --";
            if(Headers == null) { headers = "-- no headers --"; }
            else {
                List<string> h = new List<string>();
                foreach(var key in Headers.AllKeys)
                {
                    h.Add($"{key}: {Headers[key]}");
                }
                headers = string.Join("\n\t", h);
            }

            return $"Encoding: {ContentEncoding}\nMIME Type: {MIMEType}\nStatus Code: {StatusCode}\nContent: {Encoding.UTF8.GetString(Buffer)} (Length {Buffer.Length})\n" +
                $"Headers: {headers}\nCookies: {cookies}";
        }
    }
}
