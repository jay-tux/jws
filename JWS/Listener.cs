using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using Jay.Ext;
using Jay.Config;
using Jay.IO.Logging;

namespace Jay.Web.Server
{
    public class Listener
    {
        public static string HTMLDir { get; private set; }
        public static string ErrorDir { get; private set; }
        private HttpListener _listener { get; set; }
        public int Port { get; private set; }
        public string ListenerState { get; private set; }
        public bool Stopped { get; set; }

        public Listener()
        {
            if(!HttpListener.IsSupported)
            {
                StaticLogger.LogError(this, "Failed to start HttpListener: not supported.");
                Environment.Exit(1);
            }
            _listener = new HttpListener();
            GetPrefixes().ForEach(prefix => {
                _listener.Prefixes.Add(prefix);
                StaticLogger.LogDebug(this, $"Added {prefix}.");
            });
            _listener.Start();
            Stopped = false;

            LoadPaths();
            StaticLogger.LogMessage(this, "Listener started.");
        }

        public void LoadPaths()
        {
            try
            {
                object h = Program.Settings["JWS.Paths.HTML"];
                if(h is string html)
                {
                    try
                    {
                        html = html.Replace("@Home", Program.GetHome()).Replace("@Data", Program.Data());
                        if(Directory.Exists(html))
                        {
                            if(html.EndsWith("/")) html = html.Substring(0, html.Length - 1);
                            HTMLDir = html;
                            StaticLogger.LogMessage(this, $"Set HTML dir to {html}.");
                        }
                        else
                        {
                            StaticLogger.LogError(this, $"HTML Dir in config file ({html}) doesn't exist. No fallback available.");
                            Program.Instance.Exit(3);
                        }
                    }
                    catch(IOException)
                    {
                        StaticLogger.LogError(this, $"HTML Dir in config file doesn't exist. No fallback available.");
                        Program.Instance.Exit(3);
                    }
                }
            }
            catch(ArgumentException)
            {
                string pth = Program.GetHome() + "/.config/jws/html/";
                StaticLogger.LogWarning(this, $"HTML Dir not configured in config file (JWS.Paths.HTML). Trying fallback {pth}.");
                try
                {
                    if(Directory.Exists(pth))
                    {
                        HTMLDir = pth;
                        StaticLogger.LogMessage(this, $"Set HTML dir to {pth}.");
                    }
                    else
                    {
                        StaticLogger.LogError(this, $"Fallback HTML Dir doesn't exist. No fallback available.");
                        Program.Instance.Exit(3);
                    }
                }
                catch(IOException)
                {
                    StaticLogger.LogError(this, $"Fallback HTML Dir doesn't exist. No fallback available.");
                    Program.Instance.Exit(3);
                }
            }

            try
            {
                object e = Program.Settings["JWS.Paths.Error"];
                if(e is string err)
                {
                    try
                    {
                        err = err.Replace("@Home", Program.GetHome()).Replace("@Data", Program.Data());
                        if(Directory.Exists(err))
                        {
                            if(err.EndsWith("/")) err = err.Substring(0, err.Length - 1);
                            ErrorDir = err;
                            StaticLogger.LogMessage(this, $"Set Error dir to {err}.");
                        }
                        else
                        {
                            StaticLogger.LogError(this, $"Error Dir in config file ({err}) doesn't exist. No fallback available.");
                            Program.Instance.Exit(4);
                        }
                    }
                    catch(IOException)
                    {
                        StaticLogger.LogError(this, $"Error Dir in config file doesn't exist. No fallback available.");
                        Program.Instance.Exit(4);
                    }
                }
            }
            catch(ArgumentException)
            {
                string pth = Program.GetHome() + "/.config/jws/error/";
                StaticLogger.LogWarning(this, $"Error Dir not configured in config file (JWS.Paths.Error). Trying fallback {pth}.");
                try
                {
                    if(Directory.Exists(pth))
                    {
                        ErrorDir = pth;
                        StaticLogger.LogMessage(this, $"Set Error dir to {pth}.");
                    }
                    else
                    {
                        StaticLogger.LogError(this, $"Fallback Error Dir doesn't exist. No fallback available.");
                        Program.Instance.Exit(4);
                    }
                }
                catch(IOException)
                {
                    StaticLogger.LogError(this, $"Fallback Error Dir doesn't exist. No fallback available.");
                    Program.Instance.Exit(4);
                }
            }
        }

        public IEnumerable<(Request, Response)> Loop()
        {
            while(!Stopped)
            {
                StaticLogger.LogMessage(this, $"Waiting for requests...");
                HttpListenerContext ctext = _listener.GetContext();
                Request req = new Request(ctext.Request);
                StaticLogger.LogMessage(this, $"Received request:\n{req}");
                Response resp = Response.ToRequest(req);
                resp.Write(ctext.Response);
                StaticLogger.LogMessage(this, $"Sent Response:\n{resp}");
                yield return (req, resp);
            }
        }

        private void GetState()
        {
            try
            {
                object s = Program.Settings["JWS.Listener.State"];
                if(s is string state)
                {
                    ListenerState = state;
                    StaticLogger.LogMessage(this, $"State is {state}.");
                }
                else
                {
                    StaticLogger.LogWarning(this, $"JWS.Listener.State config variable is invalid; expected string. Using Debug configuration.");
                    ListenerState = "Debug";
                }
            }
            catch(ArgumentException)
            {
                StaticLogger.LogWarning(this, $"JWS.Listener.State config variable doesn't exist. Falling back to Debug.");
                ListenerState = "Debug";
            }
        }

        private void GetPort()
        {
            try
            {
                object p = Program.Settings[$"JWS.Listener.{ListenerState}.Port"];
                if(p is string prt)
                {
                    if(int.TryParse(prt, out int port))
                    {
                        Port = port;
                        StaticLogger.LogMessage(this, $"Booting server to port {port}.");
                    }
                    else
                    {
                        StaticLogger.LogWarning(this, $"JWS.Listener.{ListenerState}.Port config variable is not an integer. Falling back to port 1300.");
                        Port = 1300;
                    }
                }
                else
                {
                    StaticLogger.LogWarning(this, $"JWS.Listener.{ListenerState}.Port config variable is invalid; expected string. Using port 1300.");
                    Port = 1300;
                }
            }
            catch(ArgumentException)
            {
                StaticLogger.LogWarning(this, $"JWS.Listener.{ListenerState}.Port config variable doesn't exist (maybe your state is undefined?). Falling back to port 1300.");
                Port = 1300;
            }
        }

        private List<string> GetPrefixes()
        {
            GetState();
            GetPort();

            List<string> prefixes = new List<string>();
            try
            {
                object l = Program.Settings[$"JWS.Listener.{ListenerState}.Prefixes"];
                if(l is List<Jcf> lst)
                {
                    lst.Enumerate((jcf, ind) => {
                        try
                        {
                            object p = jcf["Entry"];
                            if(p is string pref)
                            {
                                prefixes.Add($"{pref}:{Port}/");
                                StaticLogger.LogDebug(this, $"Added {pref}.");
                            }
                            else
                            {
                                StaticLogger.LogWarning(this, $"Prefixes element {ind}: Entry is invalid; expected string. Ignoring.");
                            }
                        }
                        catch(ArgumentException)
                        {
                            StaticLogger.LogWarning(this, $"Prefixes element {ind}: can't find Entry key. Ignoring.");
                        }
                    });
                }
            }
            catch(ArgumentException)
            {
                StaticLogger.LogWarning(this, $"JWS.Listener.{ListenerState}.Prefixes config variable doesn't exist (maybe your state is undefined?). " +
                    "Falling back to 'https://localhost'.");
                prefixes.Add("https://localhost:" + Port + "/");
            }

            if(prefixes.Count == 0)
            {
                StaticLogger.LogWarning(this, $"No eligible prefixes found. Falling back to 'https://localhost'.");
                prefixes.Add("https://localhost:" + Port + "/");
            }

            return prefixes;
        }
    }
}
