using System;
using Jay.IO.Logging;
using System.IO;
using Jay.Config;
using System.Collections.Generic;
using Jay.Ext;
using System.Linq;

namespace Jay.Web.Server
{
    public class Program
    {
        private static ILogger _logger;
        public static ILogger Logger {
            get => _logger;
            set {
                _logger = value;
                if(Instance.LogBuffer != null)
                {
                    while(Instance.LogBuffer.Count > 0)
                    {
                        (string message, LogSeverity severity, DateTime tm) = Instance.LogBuffer.Pop();
                        _logger.LogFormatted("_driver", message, severity, tm);
                    }
                }
            }
        }
        public static Jcf Settings { get => Instance._settings; }
        private Jcf _settings;
        public static Program Instance;
        public event EventHandler OnExit;
        private List<(string, LogSeverity, DateTime)> LogBuffer;
        private bool Overridden;

        public static void Main(string[] args)
        {
            new Program(args);
        }

        private void Log(string message, LogSeverity severity)
        {
            if(Logger == null) {
                LogBuffer.Add((message, severity, DateTime.Now));
            }
            else
                Logger.LogFormatted("_driver", message, severity);
        }

        private Program(string[] args)
        {
            LogBuffer = new List<(string, LogSeverity, DateTime)>();
            Log("Loading Override CLI arguments...", LogSeverity.Debug);
            Dictionary<string, string> over = Overrides(args);
            Instance = this;
            Console.CancelKeyPress += (o, e) => Exit(-1);
            Log($"Starting server at {DateTime.Now.ToString()}.", LogSeverity.Message);

            Log("Loading Settings...", LogSeverity.Message);
            string[] locs = new string[0];
            int loc = -1;

            if(over.ContainsKey("config")) {
                locs = new string[] { over["config"] };
                loc = LoadSettings(locs);
            }
            else if(!over.ContainsKey("noconfig")) {
                locs = new string[] {
                    GetHome() + "/.config/jws/jws.jcf", Data() + "/jws/jws.jcf"
                };
                loc = LoadSettings(locs);
            }
            else {
                _settings = new Jcf();
                Logger = SimpleLogger.Instance;
                Overridden = true;
            }

            if(over.ContainsKey("cwd"))
            {
                string cwd = Environment.CurrentDirectory;
                Settings.Override("JWS.Paths.HTML", cwd + "/html");
                Settings.Override("JWS.Paths.Error", cwd + "/error");
                Settings.Override("JWS.Paths.Template", cwd + "/template");
            }

            if(over.ContainsKey("state")) Settings.Override("JWS.Listener.State", over["state"]);
            if(over.ContainsKey("html")) Settings.Override("JWS.Paths.HTML", over["html"]);
            if(over.ContainsKey("error")) Settings.Override("JWS.Paths.Error", over["error"]);
            if(over.ContainsKey("template")) Settings.Override("JWS.Paths.Template", over["template"]);
            if(over.ContainsKey("name")) Settings.Override("JWS.Server.Name", over["name"]);
            if(over.ContainsKey("port")) Settings.Override($"JWS.Listener.{Settings["JWS.Listener.State"]}.Port", over["port"]);


            if(!Overridden) {
                OnExit += ((o, e) => {
                    Log($"Attempting to save config file to {locs[loc]} (index {loc}).", LogSeverity.Message);
                    _settings.Save(locs[loc]);
                });
            }
            Log("Settings succesfully loaded.", LogSeverity.Message);

            Log($"Starting hooks...", LogSeverity.Message);
            Hooks();
            Log($"Hooks are ready!", LogSeverity.Message);

            Log($"Starting Listener loop...", LogSeverity.Message);
            Listener server = new Listener();
            foreach(var v in server.Loop()) {}

            Log($"Server shut down at {DateTime.Now.ToString()}.", LogSeverity.Message);
            Exit(0);
        }

        private Dictionary<string, string> Overrides(string[] args)
        {
            Dictionary<string, string> cli = new Dictionary<string, string>();
            string[] expect = new string[] {
                "--config", "--port", "--state", "--html", "--error", "--template", "--name"
            };
            string[] single = new string [] {
                "--help", "--cwd", "--noconfig"
            };

            for(int i = 0; i < args.Length; i++)
            {
                if(expect.Contains(args[i]))
                {
                    if(i == args.Length - 1 || args[i + 1].StartsWith("--"))
                    {
                        Log($"CLI Argument {i}: {args[i]} expects an argument, none given. Ignoring...", LogSeverity.Warning);
                    }
                    else
                    {
                        cli[args[i].Substring(2)] = args[++i];
                        Log($"Parsed CLI Argument {i - 1}: {args[i - 1]} got set to {args[i]}", LogSeverity.Debug);
                    }
                }
                else if(single.Contains(args[i]))
                {
                    if(args[i] == "--help") PrintHelp();
                    cli[args[i].Substring(2)] = "";
                    Log($"Added option #{i} ({args[i]}) to the liste of options.", LogSeverity.Debug);
                }
                else
                {
                    Log($"Argument {i}: unknown. Ignoring...", LogSeverity.Warning);
                }
            }
            return cli;
        }

        private void PrintHelp()
        {
            Console.WriteLine("                                         === JWS:  Jay's  Web  Server ===                                          ");
            Console.WriteLine("JWS is a simple, configurable and extendable web server designed for ease of use.");
            Console.WriteLine("For an in-depth reference, see https://github.com/jay-tux/jws.");
            Console.WriteLine("    --- Arguments --- ");
            Console.WriteLine(" --config <config file>; starts with the given config file, skips scanning of other config files.");
            Console.WriteLine(" --port <port no>; starts JWS on the given port, instead of the port defined in the config file.");
            Console.WriteLine(" --state <statename>; uses <statename>, overriding the JWS.Server.State settings.");
            Console.WriteLine(" --html <html>; uses <html> as the HTML source directory.");
            Console.WriteLine(" --error <error>; uses <error> as the directory containing error messages.");
            Console.WriteLine(" --template <template>; uses <template> as the templates directory.");
            Console.WriteLine(" --name <name>; changes the server's name (overriding the settings file).");
            Console.WriteLine("");
            Console.WriteLine("    ---  Toggles  --- ");
            Console.WriteLine(" --help; displays this help message and exits.");
            Console.WriteLine(" --cwd; uses the current directory as root (expects html/, error/ and template/ directories; unless specified).");
            Console.WriteLine(" --noconfig; skips config loading and uses default settings (will crash if the required directories are not given).");
            Environment.Exit(0);
        }

        private int LoadSettings(string[] locations)
        {
            int ret = 0;
            string f = "";
            foreach(string loc in locations)
            {
                Log($"Trying config location {loc}...", LogSeverity.Message);
                try
                {
                    f = File.ReadAllText(loc);
                    break;
                }
                catch(IOException) { ret++; }
            }

            if(f == "")
            {
                Log($"Failed to find any configuration file.", LogSeverity.Message);
                Environment.Exit(2);
            }

            _settings = JcfParser.Parse(f);
            SetLogger();
            return ret;
        }

        private void SetLogger()
        {
            Log($"Trying to initialize logger settings...", LogSeverity.Message);
            SetSubState();
            try
            {
                object t = Settings["JWS.Logging.Type"];
                if(t is string type)
                {
                    switch(type)
                    {
                        case "Channel":
                            (List<TextWriter> ch, List<List<Func<string, LogSeverity, bool>>> pr) = GetChannels();
                            Logger = new JWSChannelLogger() {
                                Channels = ch,
                                Predicates = pr
                            };
                            break;

                        case "Simple":
                            Logger = SimpleLogger.Instance;
                            break;

                        default:
                            Log($"Logger type {type} is invalid. Using fallback Simple.", LogSeverity.Warning);
                            Logger = SimpleLogger.Instance;
                            break;
                    }
                    SetSubState();
                }
                else
                {
                    Log($"Failed to initialize logger (JWS.Logging.Type should be a string). Using fallback Simple.", LogSeverity.Warning);
                    Logger = SimpleLogger.Instance;
                }
            }
            catch(ArgumentException)
            {
                Log($"Failed to initialize logger (JWS.Logging.Type is not set). Using fallback Simple.", LogSeverity.Warning);
                Logger = SimpleLogger.Instance;
            }
        }

        private void SetSubState()
        {
            string Prefix = "JWS.Logging.Formatting";
            try
            {
                object c = Settings[$"{Prefix}.Color"];
                if(c is string color)
                {
                    LogFormat.Colors = color == "on";
                    Log($"Set log color state to {LogFormat.Colors}.", LogSeverity.Debug);
                    if(!LogFormat.Colors && color != "off") Log($"{Prefix}.Color should be either 'on' or 'off'. Using fallback off.", LogSeverity.Warning);
                }
                else Log($"{Prefix}.Color should be a string. Using fallback off.", LogSeverity.Warning);
            }
            catch(ArgumentException)
            {
                Log($"{Prefix}.Color is not set. Using fallback off.", LogSeverity.Warning);
            }

            int[] vals = new int[2];
            int[] fb = new int[] { 10, 9 };
            new string[] { "MaxType", "MaxMod" }.Enumerate((x, i) => {
                try
                {
                    object v = Settings[$"{Prefix}.{x}"];
                    if(v is string vl)
                    {
                        if(int.TryParse(vl, out int value) && value > 0)
                        {
                            vals[i] = value;
                        }
                        else
                        {
                            Log($"{Prefix}.{x} should be a strictly positive integer (>0). Using fallback {fb[i]}.", LogSeverity.Warning);
                            vals[i] = fb[i];
                        }
                    }
                    else
                    {
                        Log($"{Prefix}.{x} should be a string. Using fallback {fb[i]}.", LogSeverity.Warning);
                        vals[i] = fb[i];
                    }
                }
                catch(ArgumentException)
                {
                    Log($"{Prefix}.{x} is not defined. Using fallback {fb[i]}.", LogSeverity.Warning);
                    vals[i] = fb[i];
                }
            });
            LogFormat.MaxTypeLength = vals[0];
            Log($"Set max type length to {vals[0]}.", LogSeverity.Debug);
            LogFormat.MaxModLength = vals[1];
            Log($"Set max module length to {vals[1]}.", LogSeverity.Debug);

            string[] values = new string[6];
            string[] fallback = new string[] { "36m", "37m", "33m", "31m", "yyyy-MM0dd HH:mm:ss.ffffff tt", "[@TYPE in @MOD at @TIME]: @MSG" };
            new string[] { "Colors.Debug", "Colors.Message", "Colors.Warning", "Colors.Error", "DTFormatting", "Formatting" }.Enumerate((x, i) => {
                try
                {
                    object v = Settings[$"{Prefix}.{x}"];
                    if(v is string vl)
                    {
                        values[i] = vl;
                    }
                    else
                    {
                        Log($"{Prefix}.{x} should be a string. Using fallback {fallback[i]}.", LogSeverity.Warning);
                        values[i] = fallback[i];
                    }
                }
                catch(ArgumentException)
                {
                    Log($"{Prefix}.{x} not defined. Using fallback {fallback[i]}.", LogSeverity.Warning);
                    values[i] = fallback[i];
                }
            });
            LogFormat.ColorDebug = "\u001b[" + values[0];
            Log($"Set debug color to {values[0]}.", LogSeverity.Debug);
            LogFormat.ColorMessage = "\u001b[" + values[1];
            Log($"Set message color to {values[1]}.", LogSeverity.Debug);
            LogFormat.ColorWarning = "\u001b[" + values[2];
            Log($"Set warning color to {values[2]}.", LogSeverity.Debug);
            LogFormat.ColorError = "\u001b[" + values[3];
            Log($"Set error color to {values[3]}.", LogSeverity.Debug);
            LogFormat.DateTimeFmt = values[4];
            Log($"Set date & time formatting to {values[4]}.", LogSeverity.Debug);
            LogFormat.Formatting = values[5];
            Log($"Set format string to {values[5]}.", LogSeverity.Debug);
        }

        private (List<TextWriter>, List<List<Func<string, LogSeverity, bool>>>) GetChannels()
        {
            Log("Attempting to get Debug channels...", LogSeverity.Message);
            (List<string> debugch, bool debugst) = GetChannel("Debug");
            Log("Attempting to get Message channels...", LogSeverity.Message);
            (List<string> msgch, bool msgst) = GetChannel("Message");
            Log("Attempting to get Warning channels...", LogSeverity.Message);
            (List<string> wrnch, bool wrnst) = GetChannel("Warning");
            Log("Attempting to get Error channels...", LogSeverity.Message);
            (List<string> errch, bool errst) = GetChannel("Error");

            Dictionary<string, int> resolved = new Dictionary<string, int>();
            List<TextWriter> channels = new List<TextWriter>();
            List<List<Func<string, LogSeverity, bool>>> predicates = new List<List<Func<string, LogSeverity, bool>>>();

            debugch.Enumerate((x, ind) => {
                if(!resolved.ContainsKey(x))
                {
                    TextWriter r = Resolve(x);
                    if(r != null)
                    {
                        resolved[x] = ind;
                        channels.Add(r);
                        predicates.Add(new List<Func<string, LogSeverity, bool>>());
                    }
                }

                if(resolved.ContainsKey(x))
                {
                    predicates[resolved[x]].Add((s, sev) => sev == LogSeverity.Debug && debugst);
                }
            });

            msgch.Enumerate((x, ind) => {
                if(!resolved.ContainsKey(x))
                {
                    TextWriter r = Resolve(x);
                    if(r != null)
                    {
                        resolved[x] = ind;
                        channels.Add(r);
                        predicates.Add(new List<Func<string, LogSeverity, bool>>());
                    }
                }

                if(resolved.ContainsKey(x))
                {
                    predicates[resolved[x]].Add((s, sev) => sev == LogSeverity.Message && msgst);
                }
            });

            wrnch.Enumerate((x, ind) => {
                if(!resolved.ContainsKey(x))
                {
                    TextWriter r = Resolve(x);
                    if(r != null)
                    {
                        resolved[x] = ind;
                        channels.Add(r);
                        predicates.Add(new List<Func<string, LogSeverity, bool>>());
                    }
                }

                if(resolved.ContainsKey(x))
                {
                    predicates[resolved[x]].Add((s, sev) => sev == LogSeverity.Warning && wrnst);
                }
            });

            errch.Enumerate((x, ind) => {
                if(!resolved.ContainsKey(x))
                {
                    TextWriter r = Resolve(x);
                    if(r != null)
                    {
                        resolved[x] = ind;
                        channels.Add(r);
                        predicates.Add(new List<Func<string, LogSeverity, bool>>());
                    }
                }

                if(resolved.ContainsKey(x))
                {
                    predicates[resolved[x]].Add((s, sev) => sev == LogSeverity.Error && errst);
                }
            });

            return (channels, predicates);
        }

        private (List<string>, bool) GetChannel(string channel)
        {
            try
            {
                object ch = Settings["JWS.Logging." + channel];
                bool enabled; List<string> streams = new List<string>();
                if(ch is Jcf chan)
                {
                    try
                    {
                        object s = chan["State"];
                        if(s is string state)
                        {
                            enabled = (state == "on");
                            if(state != "on" && state != "off") Log($"State for {channel} is incorrectly defined: should be on or off (using fallback off).", LogSeverity.Warning);
                            else Log($"State for {channel} succesfully set to {state}.", LogSeverity.Debug);
                        }
                        else
                        {
                            Log($"State for {channel} should be string, using fallback off.", LogSeverity.Warning);
                            enabled = false;
                        }

                        object str = chan["Stream"];
                        if(str is List<Jcf> strms)
                        {
                            strms.Enumerate((strm, ind) => {
                                try
                                {
                                    object got = strm["Value"];
                                    if(got is string vl)
                                    {
                                        streams.Add(vl);
                                        Log($"Stream {ind} for channel {channel}: added {vl}.", LogSeverity.Debug);
                                    }
                                    else Log($"Stream {ind} for channel {channel} is incorrect: Value key should contain a string.", LogSeverity.Warning);
                                }
                                catch(ArgumentException)
                                {
                                    Log($"Stream {ind} for channel {channel} is incorrect: should hold a Value key.", LogSeverity.Warning);
                                }
                            });
                        }
                        else
                        {
                            Log($"Streams for {channel} should be a list. Using fallback (only stdout).", LogSeverity.Warning);
                            streams.Add("stdout");
                        }
                        return (streams, enabled);
                    }
                    catch(ArgumentException ae)
                    {
                        Log($"Logging channel {channel} is not completely defined. Using fallback (on, stdout).", LogSeverity.Warning);
                        Console.WriteLine(ae.StackTrace);
                        return (new List<string>() { "stdout" }, true);
                    }
                }
                else
                {
                    Log($"Logging channel {channel} is incorrectly defined: should be a block. Using fallback (on, stdout).", LogSeverity.Warning);
                    return (new List<string>() { "stdout" }, true);
                }
            }
            catch(ArgumentException)
            {
                Log($"Logging channel {channel} is not defined (should be JWS.Logging.{channel}). Using default values.", LogSeverity.Warning);
                return (new List<string>() { "stdout" }, true);
            }
        }

        private TextWriter Resolve(string src)
        {
            if(src == "stdout") return Console.Out;
            if(src == "stderr") return Console.Error;

            string data = Data();
            try
            {
                if(!File.Exists(data + "/jws/" + src))
                {
                    return new StreamWriter(File.Create(data + "/jws/" + src));
                }
                else
                {
                    return new StreamWriter(File.OpenWrite(data + "/jws/" + src));
                }
            }
            catch(IOException e)
            {
                Log($"Failed get a stream for {src}: {e.Message} Using fallback stdout instead.", LogSeverity.Warning);
                return Console.Out;
            }
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

        private void Hooks()
        {
            Templating.Load();
        }

        public void Exit(int code)
        {
            Log("Running OnExit hooks...", LogSeverity.Message);
            if(Logger == null)
            {
                LogBuffer.ForEach(entry => Console.WriteLine(entry));
            }
            else
            {
                if(Logger is JWSChannelLogger jws) jws.Dispose();
            }
            OnExit?.Invoke(this, new EventArgs());
            Environment.Exit(code);
        }
    }
}
