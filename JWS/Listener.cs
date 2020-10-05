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
        public static string ListenerState { get; private set; }
        public static string ServerName { get; private set; }
        public bool Stopped { get; set; }

        public Listener()
        {
            if(!HttpListener.IsSupported)
            {
                Program.Logger.LogFormatted("Listener", "Failed to start HttpListener: not supported.", LogSeverity.Error);
                //StaticLogger.LogError(this, "Failed to start HttpListener: not supported.");
                Environment.Exit(1);
            }
            _listener = new HttpListener();
            GetPrefixes().ForEach(prefix => {
                _listener.Prefixes.Add(prefix);
                Program.Logger.LogFormatted("Listener", $"Added {prefix}.", LogSeverity.Debug);
                //StaticLogger.LogDebug(this, $"Added {prefix}.");
            });
            _listener.Start();
            Stopped = false;

            LoadPaths();
            SetServerName();
            Program.Logger.LogFormatted("Listener", "Listener started.", LogSeverity.Message);
            //StaticLogger.LogMessage(this, "Listener started.");
        }

        public void SetServerName()
        {
            try
            {
                object n = Program.Settings["JWS.Server.Name"];
                if(n is string name)
                {
                    Program.Logger.LogFormatted("Listener", $"Succesfully set Server Name to {name}.", LogSeverity.Debug);
                    //StaticLogger.LogDebug(this, $"Succesfully set Server Name to {name}.");
                    ServerName = name;
                }
                else
                {
                    Program.Logger.LogFormatted("Listener", $"JWS.Server.Name should be a string. Using fallback 'JWS'.", LogSeverity.Warning);
                    //StaticLogger.LogWarning(this, $"JWS.Server.Name should be a string. Using fallback 'JWS'.");
                    ServerName = "JWS";
                }
            }
            catch(ArgumentException)
            {
                Program.Logger.LogFormatted("Listener", $"JWS.Server.Name is undefined. Using fallback 'JWS'.", LogSeverity.Warning);
                //StaticLogger.LogWarning(this, $"JWS.Server.Name is undefined. Using fallback 'JWS'.");
                ServerName = "JWS";
            }
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
                            Program.Logger.LogFormatted("Listener", $"Set HTML dir to {html}.", LogSeverity.Message);
                            //StaticLogger.LogMessage(this, $"Set HTML dir to {html}.");
                        }
                        else
                        {
                            Program.Logger.LogFormatted("Listener", $"HTML Dir in config file ({html}) doesn't exist. No fallback available.", LogSeverity.Error);
                            //StaticLogger.LogError(this, $"HTML Dir in config file ({html}) doesn't exist. No fallback available.");
                            Program.Instance.Exit(3);
                        }
                    }
                    catch(IOException)
                    {
                        Program.Logger.LogFormatted("Listener", $"HTML Dir in config file doesn't exist. No fallback available.", LogSeverity.Error);
                        //StaticLogger.LogError(this, $"HTML Dir in config file doesn't exist. No fallback available.");
                        Program.Instance.Exit(3);
                    }
                }
            }
            catch(ArgumentException)
            {
                string pth = Program.GetHome() + "/.config/jws/html/";
                Program.Logger.LogFormatted("Listener", $"HTML Dir not configured in config file (JWS.Paths.HTML). Trying fallback {pth}.", LogSeverity.Warning);
                //StaticLogger.LogWarning(this, $"HTML Dir not configured in config file (JWS.Paths.HTML). Trying fallback {pth}.");
                try
                {
                    if(Directory.Exists(pth))
                    {
                        HTMLDir = pth;
                        Program.Logger.LogFormatted("Listener", $"Set HTML dir to {pth}.", LogSeverity.Message);
                        //StaticLogger.LogMessage("Listener", $"Set HTML dir to {pth}.");
                    }
                    else
                    {
                        Program.Logger.LogFormatted("Listener", $"Fallback HTML Dir doesn't exist. No fallback available.", LogSeverity.Error);
                        //StaticLogger.LogError("Listener", $"Fallback HTML Dir doesn't exist. No fallback available.");
                        Program.Instance.Exit(3);
                    }
                }
                catch(IOException)
                {
                    Program.Logger.LogFormatted("Listener", $"Fallback HTML Dir doesn't exist. No fallback available.", LogSeverity.Error);
                    //StaticLogger.LogError("Listener", $"Fallback HTML Dir doesn't exist. No fallback available.");
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
                            Program.Logger.LogFormatted("Listener", $"Set Error dir to {err}.", LogSeverity.Message);
                            //StaticLogger.LogMessage(this, $"Set Error dir to {err}.");
                        }
                        else
                        {
                            Program.Logger.LogFormatted("Listener", $"Error Dir in config file ({err}) doesn't exist. No fallback available.", LogSeverity.Error);
                            //StaticLogger.LogError(this, $"Error Dir in config file ({err}) doesn't exist. No fallback available.");
                            Program.Instance.Exit(4);
                        }
                    }
                    catch(IOException)
                    {
                        Program.Logger.LogFormatted("Listener", $"Error Dir in config file doesn't exist. No fallback available.", LogSeverity.Error);
                        //StaticLogger.LogError(this, $"Error Dir in config file doesn't exist. No fallback available.");
                        Program.Instance.Exit(4);
                    }
                }
            }
            catch(ArgumentException)
            {
                string pth = Program.GetHome() + "/.config/jws/error/";
                Program.Logger.LogFormatted("Listener", $"Error Dir not configured in config file (JWS.Paths.Error). Trying fallback {pth}.", LogSeverity.Warning);
                //StaticLogger.LogWarning("Listener", $"Error Dir not configured in config file (JWS.Paths.Error). Trying fallback {pth}.");
                try
                {
                    if(Directory.Exists(pth))
                    {
                        ErrorDir = pth;
                        Program.Logger.LogFormatted("Listener", $"Set Error dir to {pth}.", LogSeverity.Message);
                        //StaticLogger.LogMessage("Listener", $"Set Error dir to {pth}.");
                    }
                    else
                    {
                        Program.Logger.LogFormatted("Listener", $"Fallback Error Dir doesn't exist. No fallback available.", LogSeverity.Error);
                        //StaticLogger.LogError("Listener", $"Fallback Error Dir doesn't exist. No fallback available.");
                        Program.Instance.Exit(4);
                    }
                }
                catch(IOException)
                {
                    Program.Logger.LogFormatted("Listener", $"Fallback Error Dir doesn't exist. No fallback available.", LogSeverity.Error);
                    //StaticLogger.LogError("Listener", $"Fallback Error Dir doesn't exist. No fallback available.");
                    Program.Instance.Exit(4);
                }
            }
        }

        public IEnumerable<(Request, Response)> Loop()
        {
            while(!Stopped)
            {
                Program.Logger.LogFormatted("Listener", $"Waiting for requests...", LogSeverity.Message);
                //StaticLogger.LogMessage(this, $"Waiting for requests...");
                HttpListenerContext ctext = _listener.GetContext();
                Request req = new Request(ctext.Request);
                Program.Logger.LogFormatted("Listener", $"Received request:\n{req}", LogSeverity.Message);
                //StaticLogger.LogMessage(this, $"Received request:\n{req}");
                Response resp = Response.ToRequest(req);
                resp.Write(ctext.Response);
                Program.Logger.LogFormatted("Listener", $"Sent Response:\n{resp}", LogSeverity.Message);
                //StaticLogger.LogMessage(this, $"Sent Response:\n{resp}");
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
                    Program.Logger.LogFormatted("Listener", $"State is {state}.", LogSeverity.Message);
                    //StaticLogger.LogMessage(this, $"State is {state}.");
                }
                else
                {
                    Program.Logger.LogFormatted("Listener", $"JWS.Listener.State config variable is invalid; expected string. Using Debug configuration.", LogSeverity.Warning);
                    //StaticLogger.LogWarning(this, $"JWS.Listener.State config variable is invalid; expected string. Using Debug configuration.");
                    ListenerState = "Debug";
                }
            }
            catch(ArgumentException)
            {
                Program.Logger.LogFormatted("Listener", $"JWS.Listener.State config variable doesn't exist. Falling back to Debug.", LogSeverity.Warning);
                //StaticLogger.LogWarning(this, $"JWS.Listener.State config variable doesn't exist. Falling back to Debug.");
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
                        Program.Logger.LogFormatted("Listener", $"Booting server to port {port}.", LogSeverity.Message);
                        //StaticLogger.LogMessage(this, $"Booting server to port {port}.");
                    }
                    else
                    {
                        Program.Logger.LogFormatted("Listener", $"JWS.Listener.{ListenerState}.Port config variable is not an integer. Falling back to port 1300.",
                            LogSeverity.Warning);
                        //StaticLogger.LogWarning(this, $"JWS.Listener.{ListenerState}.Port config variable is not an integer. Falling back to port 1300.");
                        Port = 1300;
                    }
                }
                else
                {
                    Program.Logger.LogFormatted("Listener", $"JWS.Listener.{ListenerState}.Port config variable is invalid; expected string. Using port 1300.",
                        LogSeverity.Warning);
                    //StaticLogger.LogWarning(this, $"JWS.Listener.{ListenerState}.Port config variable is invalid; expected string. Using port 1300.");
                    Port = 1300;
                }
            }
            catch(ArgumentException)
            {
                Program.Logger.LogFormatted("Listener", $"JWS.Listener.{ListenerState}.Port config variable doesn't exist (maybe your state is undefined?). " +
                    "Falling back to port 1300.", LogSeverity.Warning);
                //StaticLogger.LogWarning(this, $"JWS.Listener.{ListenerState}.Port config variable doesn't exist (maybe your state is undefined?). Falling back to port 1300.");
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
                                Program.Logger.LogFormatted("Listener", $"Added {pref}.", LogSeverity.Debug);
                                //StaticLogger.LogDebug(this, $"Added {pref}.");
                            }
                            else
                            {
                                Program.Logger.LogFormatted("Listener", $"Prefixes element {ind}: Entry is invalid; expected string. Ignoring.", LogSeverity.Warning);
                                //StaticLogger.LogWarning(this, $"Prefixes element {ind}: Entry is invalid; expected string. Ignoring.");
                            }
                        }
                        catch(ArgumentException)
                        {
                            Program.Logger.LogFormatted("Listener", $"Prefixes element {ind}: can't find Entry key. Ignoring.", LogSeverity.Warning);
                            //StaticLogger.LogWarning(this, $"Prefixes element {ind}: can't find Entry key. Ignoring.");
                        }
                    });
                }
            }
            catch(ArgumentException)
            {
                Program.Logger.LogFormatted("Listener", $"JWS.Listener.{ListenerState}.Prefixes config variable doesn't exist (maybe your state is undefined?). " +
                    "Falling back to 'https://localhost'.", LogSeverity.Warning);
                //StaticLogger.LogWarning(this, $"JWS.Listener.{ListenerState}.Prefixes config variable doesn't exist (maybe your state is undefined?). " +
                //    "Falling back to 'https://localhost'.");
                prefixes.Add("https://localhost:" + Port + "/");
            }

            if(prefixes.Count == 0)
            {
                Program.Logger.LogFormatted("Listener", $"No eligible prefixes found. Falling back to 'https://localhost'.", LogSeverity.Warning);
                //StaticLogger.LogWarning(this, $"No eligible prefixes found. Falling back to 'https://localhost'.");
                prefixes.Add("https://localhost:" + Port + "/");
            }

            return prefixes;
        }
    }
}
