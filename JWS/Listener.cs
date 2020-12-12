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
            JcfResult<string> name = Program.Settings.GetString("JWS.Server.Name");
            if(name)
            {
                ServerName = (string)name;
                Program.Logger.LogFormatted("Listener", $"Succesfully set Server Name to {ServerName}.", LogSeverity.Debug);
            }
            else
            {
                Program.Logger.LogFormatted("Listener", $"JWS.Server.Name is undefined or incorrectly defined. Using fallback 'JWS'.", LogSeverity.Warning);
                ServerName = "JWS";
            }
        }

        public void LoadPaths()
        {
            JcfResult<string> _html = Program.Settings.GetString("JWS.Paths.HTML");
            if(_html)
            {
                string html = ((string)_html).Replace("@Home", Program.GetHome()).Replace("@Data", Program.Data());
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
            else
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

            JcfResult<string> _err = Program.Settings.GetString("JWS.Paths.Error");
            if(_err)
            {
                try
                {
                    string err = ((string)_err).Replace("@Home", Program.GetHome()).Replace("@Data", Program.Data());
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
            else
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
            JcfResult<string> state = Program.Settings.GetString("JWS.Listener.State");
            if(state)
            {
                ListenerState = (string)state;
                Program.Logger.LogFormatted("Listener", $"State is {ListenerState}.", LogSeverity.Message);
            }
            else
            {
                Program.Logger.LogFormatted("Listener", $"JWS.Listener.State config variable doesn't exist or is invalid. Falling back to Debug.", LogSeverity.Warning);
                ListenerState = "Debug";
            }
        }

        private void GetPort()
        {
            JcfResult<string> prt = Program.Settings.GetString($"JWS.Listener.{ListenerState}.Port");
            if(prt)
            {
                if(int.TryParse((string)prt, out int port))
                {
                    Port = port;
                    Program.Logger.LogFormatted("Listener", $"Booting server to port {port}.", LogSeverity.Message);
                }
                else
                {
                    Program.Logger.LogFormatted("Listener", $"JWS.Listener.{ListenerState}.Port config variable is not an integer. Falling back to port 1300.",
                        LogSeverity.Warning);
                    Port = 1300;
                }

            }
            else
            {
                Program.Logger.LogFormatted("Listener", $"JWS.Listener.{ListenerState}.Port config variable doesn't exist or is incorrect (maybe your state is undefined?). " +
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
            var lst = Program.Settings.GetMappedList($"JWS.Listener.{ListenerState}.Prefixes", "Entry");
            if(lst)
            {
                ((IEnumerable<JcfResult<string>>)lst).Enumerate((maybe, ind) => {
                    if(maybe)
                    {
                        prefixes.Add($"{(string)maybe}:{Port}/");
                        Program.Logger.LogFormatted("Listener", $"Added {(string)maybe}.", LogSeverity.Debug);
                    }
                    else
                    {
                        Program.Logger.LogFormatted("Listener", $"Prefixes element {ind}: Entry is invalid; check the type and whether or not it has an Entry key. Ignoring.", LogSeverity.Warning);
                    }
                });
            }
            else
            {
                Program.Logger.LogFormatted("Listener", $"JWS.Listener.{ListenerState}.Prefixes config variable doesn't exist or is invalid (maybe your state is undefined?). " +
                    "Falling back to 'https://localhost'.", LogSeverity.Warning);
                prefixes.Add("https://localhost:" + Port + "/");
            }

            if(prefixes.Count == 0)
            {
                Program.Logger.LogFormatted("Listener", $"No eligible prefixes found. Falling back to 'https://localhost'.", LogSeverity.Warning);
                prefixes.Add("https://localhost:" + Port + "/");
            }

            return prefixes;
        }
    }
}
