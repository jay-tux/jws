using System;
using System.IO;
using System.Collections.Generic;
using Jay.IO.Logging;

namespace Shub
{
    public static class Settings
    {
        public static bool Initialized = false;
        private static string Config = "/usr/local/etc/shub/default.cnf";
        public static string SaveDir;
        public static string PyRSA;
        public static string ServiceDir;

        public static bool AutoRsa;
        public static int RSATries;
        public static int RSADelay;

        public static bool EnableLog;
        public static bool RedirMsg;
        public static bool RedirWarn;
        public static bool RedirErr;
        public static bool RedirDbg;
        public static bool EnableDbg;

        public static string MsgLog;
        public static string WarnLog;
        public static string ErrLog;
        public static string DbgLog;

        public static string PyCmnd;
        public static string Shell;

        public static int PyRSAPort;

        public static void Init()
        {
            Logger.LogMessage("_conf", "Loading config file...");
            Logger.LogMessage("_conf", $"Config file at {Config}");
            try
            {
                var data = new CnfParser();
                data.Parse(File.ReadAllText(Config));
                SaveDir = data["Config.Paths.SaveDir"];
                Logger.LogMessage("_conf", $"Set save directory to {SaveDir}.");
                PyRSA = data["Config.Paths.PyRSA"];
                Logger.LogMessage("_conf", $"Set PyRSA directory to {PyRSA}.");
                ServiceDir = data["Config.Paths.ServiceDir"];
                Logger.LogMessage("_conf", $"Set services directory to {ServiceDir}.");
                EnableLog = data["Config.Logging.State"] == "on";
                Logger.LogMessage("_conf", $"Set log state to {EnableLog}.");
                RedirMsg = data["Config.Logging.RedirMsg"] == "on";
                Logger.LogMessage("_conf", $"Set redirect message state to {RedirMsg}.");
                RedirWarn = data["Config.Logging.RedirWarn"] == "on";
                Logger.LogMessage("_conf", $"Set redirect warning state to {RedirWarn}.");
                RedirErr = data["Config.Logging.RedirErr"] == "on";
                Logger.LogMessage("_conf", $"Set redirect error state to {RedirErr}.");
                RedirDbg = data["Config.Logging.RedirDbg"] == "on";
                Logger.LogMessage("_conf", $"Set redirect debug state to {RedirDbg}.");
                EnableDbg = data["Config.Logging.EnableDbg"] == "on";
                Logger.LogMessage("_conf", $"Set debug state to {EnableDbg}.");
                MsgLog = data["Config.Logging.MsgLog"];
                Logger.LogMessage("_conf", $"Set message log file to {MsgLog}.");
                WarnLog = data["Config.Logging.WarnLog"];
                Logger.LogMessage("_conf", $"Set warning log file to {WarnLog}.");
                ErrLog = data["Config.Logging.ErrLog"];
                Logger.LogMessage("_conf", $"Set error log file to {ErrLog}.");
                DbgLog = data["Config.Logging.DbgLog"];
                Logger.LogMessage("_conf", $"Set debug log file to {DbgLog}.");
                PyCmnd = data["Config.Commands.PyCmnd"];
                Logger.LogMessage("_conf", $"Set python command to {PyCmnd}.");
                Shell = data["Config.Commands.Shell"];
                Logger.LogMessage("_conf", $"Set shell to {Shell}.");
                PyRSAPort = int.Parse(data["Config.Ports.PyRSA"]);
                Logger.LogMessage("_conf", $"Set PyRSA service port to {PyRSAPort}.");
                AutoRsa = data["Config.RSA.AutoStart"] == "on";
                Logger.LogMessage("_conf", $"Set Auto RSA state to {AutoRsa}.");
                RSATries = int.Parse(data["Config.RSA.Tries"]);
                Logger.LogMessage("_conf", $"Set RSA tries to {RSATries}.");
                RSADelay = int.Parse(data["Config.RSA.Delay"]);
                Logger.LogMessage("_conf", $"Set RSA delay to {RSADelay} ms.");
                Initialized = true;
            }
            catch(ArgumentException e)
            {
                Logger.LogError("_conf", "Corrupted configuration file: " + e.Message);
                Environment.Exit(1);
            }
            catch(Exception ioe)
            {
                Logger.LogError("_conf", "Can't load config file: " + ioe.Message);
                Environment.Exit(1);
            }
        }
    }

    public class CnfParser
    {
        private Dictionary<string, CnfParser> Subs;
        private Dictionary<string, string> Values;
        private CnfParser Parent;

        public string this[string key]
        {
            get => GetKey(key);
        }

        private string GetKey(string key)
        {
            int split = key.IndexOf(".");
            if(split == -1)
            {
                if(Values.ContainsKey(key)) { return Translate(Values[key]); }
                throw new ArgumentException("Invalid key: " + key);
            }
            else
            {
                string pre = key.Substring(0, split);
                string post = key.Substring(split + 1);
                if(Subs.ContainsKey(pre))
                {
                    try
                    {
                        return Subs[pre][post];
                    }
                    catch(ArgumentException e)
                    {
                        throw new ArgumentException(e.Message + " in " + pre, e);
                    }
                }
                throw new ArgumentException("Invalid partial key: " + pre);
            }
        }

        public IEnumerator<KeyValuePair<string, string>> Enumerate()
        {
            foreach(var kvp in Values) yield return kvp;
        }

        public IEnumerator<KeyValuePair<string, CnfParser>> EnumerateSubs()
        {
            foreach(var kvp in Subs) yield return kvp;
        }

        public IEnumerator<KeyValuePair<string, string>> Enumerate(string target)
        {
            int split = target.IndexOf(".");
            if(split == -1)
            {
                if(Subs.ContainsKey(target)) { return Subs[target].Enumerate(); }
                throw new ArgumentException("Invalid key: " + target);
            }
            else
            {
                string pre = target.Substring(0, split);
                string post = target.Substring(split + 1);
                if(Subs.ContainsKey(pre))
                {
                    try
                    {
                        return Subs[pre].Enumerate(post);
                    }
                    catch(ArgumentException e)
                    {
                        throw new ArgumentException(e.Message + " in " + pre, e);
                    }
                }
            }
            throw new ArgumentException("Invalid partial key: " + target);
        }

        public CnfParser()
        {
            Subs = new Dictionary<string, CnfParser>();
            Values = new Dictionary<string, string>();
            Parent = null;
        }

        public CnfParser(CnfParser parent)
        {
            Subs = new Dictionary<string, CnfParser>();
            Values = new Dictionary<string, string>();
            Parent = parent;
        }

        public string Translate(string todo)
        {
            string res = "";
            string key = "";
            bool inkey = false;
            todo.ForEach(chr => {
                if(chr == '$')
                {
                    if(inkey) { inkey = false; res += Lookup(key); key = ""; }
                    else { inkey = true; }
                }
                else
                {
                    if(inkey) { key += chr; }
                    else { res += chr; }
                }
            });
            return res;
        }

        private string Lookup(string todo)
        {
            try
            {
                return this[todo];
            }
            catch(Exception)
            {
                if(Parent != null) { return Parent[todo]; }
                throw new NullReferenceException("Can't find " + todo);
            }
        }

        public void Parse(string file)
        {
    	    string key = "";
    	    string val = "";
    	    bool inkey = true;
    	    bool inBlock = false;
    	    int depth = 0;
            file.ForEach(chr => {
        		if(chr == ':' && !inBlock)
        		{
        		    inkey = false;
        		}
        		else if(chr == '{')
        		{
        		    inBlock = true;
        		    depth++;
        		    if(depth != 1) { val += chr; }
        		}
        		else if(chr == '}')
        		{
        		    depth--;
        		    if(depth == 0) {
        		    	inBlock = false;
        			Subs[key.Trim()] = new CnfParser(this);
        			Subs[key.Trim()].Parse(val);
        			key = "";
        			val = "";
        		    }
        		    else { val += chr; }
        		}
        		else if(inBlock)
        		{
        			val += chr;
        		}
        		else if(chr == '\n')
        		{
        			inkey = true;
        			if(key.Trim() != "" && val.Trim() != "")
        			{
        			    Values[key.Trim()] = val.Trim();
        			}
        			key = ""; val = "";
        		}
        		else
        		{
        			if(inkey) { key += chr; }
        			else { val += chr; }
        		}
            });
        }
    }
}
