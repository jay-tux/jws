using System;
using System.Net;
using Jay.Ext;

namespace Jay.Web.Server
{
    public class Listener
    {
        private HttpListener Listener { get; private set; }
        public int Port { get; private set; }
        public string ListenerState { get; private set; }
        public Listener()
        {
            if(!HttpListener.IsSupported)
            {
                Logger.LogError(this, "Failed to start HttpListener: not supported.");
                Environment.Exit(1);
            }
            Listener = new HttpListener();
            GetPrefixes().ForEach(prefix => Listener.Add(prefix));
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
                    Logger.LogWarning(this, $"JWS.Listener.State config variable is invalid; expected string. Using Debug configuration.");
                    ListenerState = "Debug";
                }
            }
            catch(JWSException jwse)
            {
                Logger.LogWarning(this, $"JWS.Listener.State config variable doesn't exist. Falling back to Debug.");
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
        }
    }
}
