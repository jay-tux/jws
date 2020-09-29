using System;
using System.Net;
using System.Collections.Generic;
using Jay.Ext;
using Jay.Config;
using Jay.IO.Logging;

namespace Jay.Web.Server
{
    public class Listener
    {
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
            StaticLogger.LogMessage(this, "Listener started.");
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
