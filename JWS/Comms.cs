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
        public Encoding ContentEncoding;
        public long ContentLength;
        public string MIMEType;
        public CookieCollection Cookies;
        public WebHeaderCollection Headers;
        public int StatusCode;
        public byte[] Buffer;

        private Response() {}

        private static string LoadError(int code)
        {
            string fallback = "<html><head><meta charset=\"utf-8\" /><title>JWS - HTTP " + code.ToString() + "</title></head>"
                + "<body>The server has run into an HTTP " + code.ToString() + " status.</body>";

            string msg = null;
            string generic = "";
            try
            {
                object n = Program.Settings["JWS.Server.Name"];
                if(n is string name)
                {
                    generic = "<html><head><meta charset=\"utf-8\" /><title>" + name + " - HTTP " + code.ToString() + "</title></head>"
                        + "<body>The server has run into an HTTP " + code.ToString() + " status.</body>";
                }
                else
                {
                    msg = "Server name (JWS.Server.Name) should be a string. Using fallback JWS.";
                }
            }
            catch(ArgumentException)
            {
                msg = "Server name (JWS.Server.Name) not set in config file. Using fallback JWS.";
            }

            if(File.Exists($"{Listener.ErrorDir}/{code}.html"))
            {
                try
                {
                    return File.ReadAllText($"{Listener.ErrorDir}/{code}.html");
                }
                catch(IOException)
                {
                    StaticLogger.LogWarning("_resolver", $"Tried to load existing error file {Listener.ErrorDir}/{code}.html, but couldn't read it. Returning generic error file.");
                    if(msg == null)
                    {
                        return generic;
                    }
                    else
                    {
                        StaticLogger.LogWarning("_resolver", msg);
                        return fallback;
                    }
                }
            }
            else
            {
                StaticLogger.LogWarning("_resolver", $"Tried to load non-existent error file {Listener.ErrorDir}/{code}.html. Returning generic error file.");
                if(msg == null)
                {
                    return generic;
                }
                else
                {
                    StaticLogger.LogWarning("_resolver", msg);
                    return fallback;
                }
            }
        }

        public static Response ToRequest(Request r)
        {
            if(r.Path == "/")
            {
                StaticLogger.LogDebug("_resolver", "Client requested server root (/). Attempting to resolve...");
                try
                {
                    object rt = Program.Settings["JWS.Server.Root"];
                    if(rt is string root)
                    {
                        r.Path = root;
                    }
                    else
                    {
                        StaticLogger.LogWarning("_resolver", "Server root (JWS.Server.Root) should be a string. Using fallback /index.html.");
                        r.Path = "/index.html";
                    }
                }
                catch(ArgumentException)
                {
                    StaticLogger.LogWarning("_resolver", "Server root (JWS.Server.Root) is not configured. Using fallback /index.html.");
                    r.Path = "/index.html";
                }
            }

            byte[] buffer; int code = 200; bool error = false;
            if(File.Exists(Listener.HTMLDir + r.Path))
            {
                try
                {
                    buffer = Encoding.UTF8.GetBytes(File.ReadAllText(Listener.HTMLDir + r.Path));
                }
                catch(IOException)
                {
                    StaticLogger.LogWarning("_resolver", $"Client requested {r.Path}; which resolved to existing file {Listener.HTMLDir + r.Path}, but couldn't be read. Returning 403.");
                    code = 403;
                    buffer = Encoding.UTF8.GetBytes(LoadError(code));
                    error = true;
                }
            }
            else
            {
                StaticLogger.LogMessage("_resolver", $"Client requested {r.Path}; which resolved to non-existent file {Listener.HTMLDir + r.Path}. Returning 404.");
                code = 404;
                buffer = Encoding.UTF8.GetBytes(LoadError(404));
                error = true;
            }

            string mime = "text/html";
            if(!error)
            {
                string ext = r.Path.Split('.').Last();
                try
                {
                    object m = Program.Settings["JWS.Server.FileAssoc." + ext];
                    if(m is string mi)
                    {
                        mime = mi;
                    }
                    else
                    {
                        StaticLogger.LogWarning("_resolver", $"File associations should be strings; not the case for JWS.Server.FileAssoc.{ext}. Using fallback text/html.");
                    }
                }
                catch(ArgumentException)
                {
                    StaticLogger.LogWarning("_resolver", $"File association *.{ext} not present. Configure as JWS.Server.FileAssoc.{ext}. Using fallback text/html.");
                }
            }

            return new Response() {
                ContentEncoding = Encoding.UTF8,
                ContentLength = buffer.Length,
                MIMEType = mime,
                Cookies = null,
                Headers = null,
                StatusCode = code,
                Buffer = buffer
            };
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
                    StaticLogger.LogWarning(this, $"Status description for {StatusCode} should be string.");
                    resp.StatusDescription = "-- unknown status --";
                }
            }
            catch(ArgumentException)
            {
                StaticLogger.LogWarning(this, $"Status description for {StatusCode} (JWS.Statusses.{StatusCode}) is undefined.");
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
