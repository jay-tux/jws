using System;
using Jay.IO.Logging;
using System.IO;
using Jay.Config;

namespace Jay.Web.Server
{
    public class Program
    {
        public static Jcf Settings;
        private Jcf _settings;
        public static Program Instance;
        public event EventHandler OnExit;

        public static void Main(string[] args)
        {
            new Program();
        }

        private Program()
        {
            Instance = this;
            StaticLogger.LogMessage("_driver", $"Starting server at {DateTime.Now.ToString()}.");

            StaticLogger.LogMessage("_driver", "Loading Settings...");
            string[] locs = new string[] {
                GetHome() + "/.config/jws/jws.jcf", Data() + "/jws/jws.jcf"
            };
            int loc = LoadSettings(locs);
            Settings = _settings;
            OnExit += ((o, e) => {
                StaticLogger.LogMessage("_driver", $"Attempting to save config file to {locs[loc]} (index {loc}).");
                _settings.Save(locs[loc]);
            });
            StaticLogger.LogMessage("_driver", "Settings succesfully loaded.");

            StaticLogger.LogMessage("_driver", $"Starting Listener loop...");
            Listener server = new Listener();
            foreach(var v in server.Loop()) {}

            StaticLogger.LogMessage("_driver", $"Server shut down at {DateTime.Now.ToString()}.");
            Exit(0);
        }

        private int LoadSettings(string[] locations)
        {
            int ret = 0;
            string f = "";
            foreach(string loc in locations)
            {
                StaticLogger.LogMessage("_driver", $"Trying config location {loc}...");
                try
                {
                    f = File.ReadAllText(loc);
                    break;
                }
                catch(IOException) { ret++; }
            }

            if(f == "")
            {
                StaticLogger.LogError("_driver", $"Failed to find any configuration file.");
                Environment.Exit(2);
            }

            _settings = JcfParser.Parse(f);
            return ret;
        }

        public static string GetHome()
        {
            var platform = Environment.OSVersion.Platform;
            if(platform == PlatformID.Unix || platform == PlatformID.MacOSX)
            {
                return Environment.GetEnvironmentVariable("HOME");
            }
            else
            {
                return Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
            }
        }

        public static string Data()
        {
            var platform = Environment.OSVersion.Platform;
            if(platform == PlatformID.Unix || platform == PlatformID.MacOSX)
            {
                return "/usr/local/etc";
            }
            else
            {
                return Environment.ExpandEnvironmentVariables("%APPDATA%");
            }
        }

        public void Exit(int code)
        {
            StaticLogger.LogMessage(this, "Running OnExit hooks...");
            OnExit?.Invoke("_driver", new EventArgs());
            Environment.Exit(code);
        }
    }
}
