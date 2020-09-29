using System;
using Jay.IO.Logging;
using System.IO;
using Jay.Config;

namespace Jay.Web.Server
{
    public static class Program
    {
        public static Jcf Settings;
        public static void Main(string[] args)
        {
            StaticLogger.LogMessage("_driver", $"Starting server at {DateTime.Now.ToString()}.");

            StaticLogger.LogMessage("_driver", "Loading Settings...");
            LoadSettings(new string[] {
                GetHome() + "/.config/jws/jws.jcf", Data() + "/jws/jws.jcf"
            });
            StaticLogger.LogMessage("_driver", "Settings succesfully loaded.");

            StaticLogger.LogMessage("_driver", $"Starting Listener loop...");
            Listener server = new Listener();
            foreach(var v in server.Loop()) {}

            StaticLogger.LogMessage("_driver", $"Server shut down at {DateTime.Now.ToString()}.");
        }

        private static void LoadSettings(string[] locations)
        {
            string f = "";
            foreach(string loc in locations)
            {
                StaticLogger.LogMessage("_driver", $"Trying config location {loc}...");
                try
                {
                    f = File.ReadAllText(loc);
                }
                catch(IOException) {}
            }

            if(f == "")
            {
                StaticLogger.LogError("_driver", $"Failed to find any configuration file.");
                Environment.Exit(2);
            }

            Settings = JcfParser.Parse(f);
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
                return Environment.GetEnvironmentVariable("/usr/local/etc");
            }
            else
            {
                return Environment.ExpandEnvironmentVariables("%APPDATA%");
            }
        }
    }
}
