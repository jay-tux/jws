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
        public string Query;
        public NameValueCollection Getters;
        public string Content;

        public Request(HttpListenerRequest r)
        {
            ContentEncoding = r.ContentEncoding;
            ContentLength = r.ContentLength64;
            MIMEType = r.ContentType;
            Cookies = r.Cookies;
            Headers = r.Headers;
            Method = r.HttpMethod;
            Query = string.Join("", r.Url.Segments);
            Getters = r.QueryString;
            Content = (r.HasEntityBody) ? new StreamReader(r.InputStream, ContentEncoding).ReadToEnd() : "";
        }

        public override string ToString()
        {
            /*List<string> c = new List<string>();
            foreach(var cookie in Cookies)
            {
                c.Add($"Cookie {cookie.Name}: {cookie.Value}");
            }
            string cookies = string.Join("\n\t", c);*/
            string cookies = "-- cookies failed --";
            List<string> h = new List<string>();
            foreach(var key in Headers.AllKeys)
            {
                h.Add($"{key}: {Headers[key]}");
            }
            string headers = string.Join("\n\t", h);
            return $"Encoding: {ContentEncoding}\nMIME Type: {MIMEType}\nHTTP Method: {Method}\nQuery string: {Query}\nContent: '{Content}' (Length: {ContentLength})\n" +
                $"Headers: {headers}\nCookies: {cookies}";
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
        public static Response ToRequest(Request r)
        {
            byte[] buffer = Encoding.UTF8.GetBytes("<html><head><title>JWS</title></head><body>You found a page: " + r.Query + ".</body></html>");
            return new Response() {
                ContentEncoding = Encoding.UTF8,
                ContentLength = buffer.Length,
                MIMEType = "text/html",
                Cookies = null,
                Headers = null,
                StatusCode = 200,
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
            string cookies = "-- cookies failed --";
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
