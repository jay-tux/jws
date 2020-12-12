using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Collections.Specialized;
using Jay.IO.Logging;
using Jay.Config;

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
        public Dictionary<string, string> POST;

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
            Program.Logger.LogFormatted("_request", $"Request content is: \n{Content}", LogSeverity.Debug);
            ParsePost(Content);
        }

        private void ParsePost(string cnt)
        {
            POST = new Dictionary<string, string>();
            Array.ForEach(cnt.Split('&'), entry => {
                if(entry.Contains('=')) POST[entry.Split('=')[0]] = entry.Split('=')[1];
                else POST[entry] = "";
            });
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
        private byte[] Buffer;
        public string Content { get => Encoding.UTF8.GetString(Buffer); set => Buffer = Encoding.UTF8.GetBytes(value); }
        private static List<(Func<Request, Response, bool>, Action<Request, Response>)> _hooks =
            new List<(Func<Request, Response, bool>, Action<Request, Response>)>();

        private Response() {}

        public static void Hook(Func<Request, Response, bool> filter, Action<Request, Response> consumer)
            => _hooks.Add((filter, consumer));

        public string this[HttpResponseHeader h] { get => Headers[h]; set => Headers[h] = value; }

        public string LoadError(int code)
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
            string state = Listener.ListenerState;
            JcfResult<string> route = Program.Settings.GetString("JWS.Routing." + _target);
            if(route)
            {
                Program.Logger.LogFormatted("_resolver", $"Route JWS.Routing.{_target} resolved to {(string)route}", LogSeverity.Debug);
                _target = (string)route;
                JcfResult<string> choice = Program.Settings.GetString($"JWS.Listener.{state}.Routing");
                if(choice)
                {
                    if((string)choice == "302")
                    {
                        StatusCode = 302;
                        Headers.Add(HttpResponseHeader.Location, _target);
                        Buffer = new byte[0];
                        _finished = true;
                    }
                    else if((string)choice != "stay")
                    {
                        Program.Logger.LogFormatted("_resolver", $"Invalid value for JWS.Listener.{state}.Routing: {choice}. " +
                            "Valid options are 302 and stay. Using fallback stay.", LogSeverity.Warning);
                    }
                }
                else if(choice.state == ResultOptions.TypeWrong)
                    Program.Logger.LogFormatted("_resolver", $"JWS.Listener.{state}.Routing should be a string (either 302 or stay). Using fallback stay.", LogSeverity.Warning);
                else
                    Program.Logger.LogFormatted("_resolver", $"Routing policy for {state} (JWS.Listener.{state}.Routing) not set (expecting either 302 or stay). Using fallback stay.",
                    LogSeverity.Warning);
            }
            else if(route.state == ResultOptions.TypeWrong)
                Program.Logger.LogFormatted("_resolver", $"Route JWS.Routing.{_target} is not a string. Attempting to resolve URL as file...", LogSeverity.Warning);
        }

        private void AttemptRootRoute()
        {
            if(_target == "/")
            {
                Program.Logger.LogFormatted("_resolver", "Client requested server root (/). Attempting to resolve...", LogSeverity.Debug);
                JcfResult<string> root = Program.Settings.GetString("JWS.Server.Root");
                if(root) { _target = (string)root; }
                else
                {
                    _target = "/index.html";
                    Program.Logger.LogFormatted("_resolver", "Server root (JWS.Server.Root) is not configured (correctly). Using fallback /index.html.", LogSeverity.Warning);
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
                JcfResult<string> mi = Program.Settings.GetString("JWS.Server.FileAssoc." + ext);
                if(mi) { MIMEType = (string)mi; }
                else
                    Program.Logger.LogFormatted("_resolver",
                        $"File association *.{ext} not present or incorrectly configured. Configure as JWS.Server.FileAssoc.{ext}. Using fallback text/html.",
                        LogSeverity.Warning);
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
            if(!resp._finished) {
                _hooks.ForEach(hook => {
                    if(!resp._finished && hook.Item1(r, resp)) hook.Item2(r, resp);
                });
            }
            resp.ContentLength = resp.Buffer.Length;

            return resp;
        }

        public void Finish() => _finished = true;

        public void Write(HttpListenerResponse resp)
        {
            resp.ContentEncoding = ContentEncoding;
            resp.ContentLength64 = ContentLength;
            resp.ContentType = MIMEType;
            if(Cookies != null) resp.Cookies = Cookies;
            if(Headers != null) resp.Headers = Headers;
            resp.StatusCode = StatusCode;

            JcfResult<string> state = Program.Settings.GetString("JWS.Statusses." + StatusCode.ToString());
            if(state) { resp.StatusDescription = (string)state; }
            else
            {
                Program.Logger.LogFormatted("Coms", $"Status description for {StatusCode} (JWS.Statusses.{StatusCode}) is undefined or incorrectly defined.", LogSeverity.Warning);
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
