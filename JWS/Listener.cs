using System;
using System.Net;
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

        public Listener()
        {
            if(!HttpListener.IsSupported)
            {
                StaticLogger.LogError(this, "Failed to start HttpListener: not supported.");
                Environment.Exit(1);
            }
            _listener = new HttpListener();
            GetPrefixes().ForEach(prefix => _listener.Prefixes.Add(prefix));
        }

        private void GetState()
        {
            try
            {
                object s = Program.Settings["JWS.Listener.State"];
                if(s is string state)
                {
                    ListenerState = state;
                }
                else
                {
                    StaticLogger.LogWarning(this, $"JWS.Listener.State config variable is invalid; expected string. Using Debug configuration.");
                    ListenerState = "Debug";
                }
            }
            catch(JcfException)
            {
                StaticLogger.LogWarning(this, $"JWS.Listener.State config variable doesn't exist. Falling back to Debug.");
                ListenerState = "Debug";
            }
        }

        private void GetPort()
        {
            //
        }

        private string[] GetPrefixes()
        {
            GetState();
            GetPort();
            return new string[0];
        }
    }
}
