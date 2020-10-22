using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using Jay.Config;
using Jay.Ext;
using Jay.IO.Logging;

namespace Jay.Web.Server
{
    public class ControlPanel
    {
        private static string LoginTitle = "Control Panel Login";
        private static string CPanelTitle = "Control Panel";
        private static string Footer = "Control Panel - Only for Authorized Users";
        private static Dictionary<string, string> CSS = new Dictionary<string, string>();
        private static List<string> Hashes;
        private static SHA256 Hasher;

        private static List<string> LoadHashes()
        {
            try
            {
                Program.Logger.LogFormatted("_cpanel_hashes", $"Loading Hashes from {Program.Data()}/jws/hs.", LogSeverity.Debug);
                if(!File.Exists(Program.Data() + "/jws/hs"))
                {
                    File.WriteAllText(Program.Data() + "/jws/hs", UserHash("admin", "admin"));
                }

                List<string> res = new List<string>();
                File.ReadAllLines(Program.Data() + "/jws/hs").ForEach(line => res.Add(line));
                return res;
            }
            catch(UnauthorizedAccessException)
            {
                Console.Error.WriteLine("Can't open hashes file " + Program.Data() + "/jws/hs.");
                Environment.Exit(6);
            }
            catch(IOException)
            {
                Console.Error.WriteLine("Can't open hashes file " + Program.Data() + "/jws/hs.");
                Environment.Exit(6);
            }
            return null;
        }

        public static void Load()
        {
            Hashes = LoadHashes();
            try
            {
                object s = Program.Settings["JWS.ControlPanel.State"];
                if(s is string state)
                {
                    if(state == "off")
                    {
                        Program.Logger.LogFormatted("_hook_cpanel", "Disabling ControlPanel...", LogSeverity.Message);
                        return;
                    }
                    else if(state != "on")
                    {
                        Program.Logger.LogFormatted("_hook_cpanel", "Setting JWS.ControlPanel.State should be either 'on' or 'off'. Assuming off.", LogSeverity.Warning);
                        return;
                    }
                }
                else
                {
                    Program.Logger.LogFormatted("_hook_cpanel", "JWS.ControlPanel.State should be a string. Assuming 'off'.", LogSeverity.Warning);
                    return;
                }
            }
            catch(ArgumentException)
            {
                Program.Logger.LogFormatted("_hook_cpanel", "JWS.ControlPanel.State is not defined. Assuming 'off'.", LogSeverity.Warning);
                return;
            }

            string cpath = "/cpanel/";
            try
            {
                object p = Program.Settings["JWS.ControlPanel.URL"];
                if(p is string path)
                {
                    Program.Logger.LogFormatted("_hook_cpanel", $"Set Control Panel URL to {path}.", LogSeverity.Debug);
                    cpath = path;
                }
                else
                {
                    Program.Logger.LogFormatted("_hook_cpanel", "Control Panel URL (JWS.ControlPanel.URL) should be a string. Using fallback /cpanel/.", LogSeverity.Warning);
                }
            }
            catch(ArgumentException)
            {
                Program.Logger.LogFormatted("_hook_cpanel", "Control Panel URL (JWS.ControlPanel.URL) not set. Using fallback /cpanel/.", LogSeverity.Warning);
            }

            try
            {
                object t = Program.Settings["JWS.ControlPanel.LoginTitle"];
                if(t is string title)
                {
                    Program.Logger.LogFormatted("_hook_cpanel", $"Set Control Panel Login Title to {title}", LogSeverity.Debug);
                    LoginTitle = title;
                }
                else
                {
                    Program.Logger.LogFormatted("_hook_cpanel", "JWS.ControlPanel.LoginTitle should be a string. Using fallback Control Panel Login.", LogSeverity.Warning);
                }
            }
            catch(ArgumentException)
            {
                Program.Logger.LogFormatted("_hook_cpanel", "JWS.ControlPanel.LoginTitle is not defined. Using fallback Control Panel Login.", LogSeverity.Warning);
            }

            try
            {
                object t = Program.Settings["JWS.ControlPanel.CPanelTitle"];
                if(t is string title)
                {
                    Program.Logger.LogFormatted("_hook_cpanel", $"Set Control Panel Title to {title}", LogSeverity.Debug);
                    CPanelTitle = title;
                }
                else
                {
                    Program.Logger.LogFormatted("_hook_cpanel", "JWS.ControlPanel.CPanelTitle should be a string. Using fallback Control Panel.", LogSeverity.Warning);
                }
            }
            catch(ArgumentException)
            {
                Program.Logger.LogFormatted("_hook_cpanel", "JWS.ControlPanel.CPanelTitle is not defined. Using fallback Control Panel.", LogSeverity.Warning);
            }

            try
            {
                object f = Program.Settings["JWS.ControlPanel.Footer"];
                if(f is string footer)
                {
                    Program.Logger.LogFormatted("_hook_cpanel", $"Set Footer to {footer}", LogSeverity.Debug);
                    Footer = footer;
                }
                else
                {
                    Program.Logger.LogFormatted("_hook_cpanel", "JWS.ControlPanel.Footer should be a string. Using fallback Control Panel - Only for Authorized Users.",
                        LogSeverity.Warning);
                }
            }
            catch(ArgumentException)
            {
                Program.Logger.LogFormatted("_hook_cpanel", "JWS.ControlPanel.Footer is not defined. Using fallback Control Panel - Only for Authorized Users.", LogSeverity.Warning);
            }

            try
            {
                object c = Program.Settings["JWS.ControlPanel.CSS"];
                if(c is Jcf css)
                {
                    css.EnumerateKeys((key, value) => {
                        CSS[key] = value;
                        Program.Logger.LogFormatted("_css_cpanel", $"Found CSS for {key}: {value}.", LogSeverity.Debug);
                    });
                }
                else
                {
                    Program.Logger.LogFormatted("_hook_cpanel", "JWS.ControlPanel.CSS should be a block. Assuming an empty block.", LogSeverity.Warning);
                }
            }
            catch(ArgumentException)
            {
                Program.Logger.LogFormatted("_hook_cpanel", "No CSS defined for Control Panel. If you want to define any, use the JWS.ControlPanel.CSS block.", LogSeverity.Message);
            }

            Program.Logger.LogFormatted("_hook_cpanel", "Hooking into Comms (1 hook).", LogSeverity.Debug);
            Response.Hook(
                (req, resp) => req.Path == cpath,
                (req, resp) => GenerateCPanel(req, resp)
            );
            Program.Logger.LogFormatted("_hook_cpanel", "Hook successful!", LogSeverity.Debug);
        }

        public static void GenerateCPanel(Request req, Response res)
        {
            if(!req.POST.ContainsKey("token"))
            {
                //no token -> log in form
                Program.Logger.LogFormatted("CPanel", $"Attempted control panel access. Requesting login....", LogSeverity.Message);
                GenerateLogin(res);
                res.Finish();
            }
            else if(req.POST["token"] == "-1")
            {
                //validate
                if(req.POST.ContainsKeys("un", "pw"))
                {
                    Program.Logger.LogFormatted("CPanel", $"User {req.POST["un"]} attempted to log in. Checking credentials...", LogSeverity.Message);
                    string h = UserHash(req.POST["un"], req.POST["pw"]);
                    if(Hashes.Contains(h))
                    {
                        Program.Logger.LogFormatted("CPanel", $"User {req.POST["un"]} succesfully authenticated.", LogSeverity.Message);
                        GenerateCPanelMain(res);
                        res.Finish();
                    }
                    else
                    {
                        Program.Logger.LogFormatted("CPanel", $" === USER {req.POST["un"]} ATTEMPTED TO LOG IN BUT WAS UNSUCCESFUL ===", LogSeverity.Warning);
                        res.Content = res.LoadError(403);
                        res.StatusCode = 403;
                        res.Finish();
                    }
                }
                else
                {
                    Program.Logger.LogFormatted("CPanel", "Invalid request. Re-sending login page...", LogSeverity.Message);
                    GenerateLogin(res);
                    res.Content = res.Content.Replace("<div></div>", "<div class=error>Please enter your name and password.</div>");
                }
            }
        }

        private static void GenerateCPanelMain(Response target)
        {
            string form = $"<!DOCTYPE html>\n<html>\n<head>\n<meta charset=utf-8 />\n<title>{Listener.ServerName} - {CPanelTitle}</title>\n" +
                (CSS.ContainsKey("Home") ? $"<link href=\"../{CSS["Home"]}\" rel=stylesheet type=\"text/css\" />\n" : "") +
                $"</head>\n<body><h1>{Listener.ServerName} - Control Panel</h1>\n" +
                string.Join("\n", Program.Settings.MapToString(
                    (s => $"<div><b>{s}</b>", "</div>"),
                    (s => $"<i>{s}</i><ul>", "</ul>"),
                    ((k, v) => $"<li>{k}: {v}</li>")
                )) +
                $"\n<div class=footer>{Footer}</div>\n</body>\n</html>";
            target.Content = form;
            target.StatusCode = 200;
        }

        private static void GenerateLogin(Response target)
        {
            string form = $"<!DOCTYPE html>\n<html>\n<head>\n<meta charset=utf-8 />\n<title>{Listener.ServerName} - {LoginTitle}</title>\n" +
                (CSS.ContainsKey("Login") ? $"<link href=\"../{CSS["Login"]}\" rel=stylesheet type=\"text/css\" />\n" : "") +
                $"</head>\n<body><h1>{Listener.ServerName} - Control Panel</h1>\n<div class=center>\n<div class=frm>\n<h2>Log In</h2>\n" +
                $"<form action=\"/cpanel/\" method=POST>\n<input type=hidden name=token value=\"-1\" /><b>Username<i class=error>*</i>: </b><input type=text name=un /><br />\n" +
                $"<b>Password<i class=error>*</i>: </b><input type=password name=pw /><br />\n<input type=submit value=\"Log In\" />\n</form>\n<div></div></div>\n</div>\n" +
                $"<div class=footer>{Footer}</div>\n</body>\n</html>";
            target.Content = form;
            target.StatusCode = 200;
        }

        private static string UserHash(string un, string pw)
        {
            if(Hasher == null) Hasher = SHA256.Create();
            byte[] name = Encoding.Unicode.GetBytes(un);
            byte[] pass = Encoding.Unicode.GetBytes(pw);
            return Hasher.ComputeHash(Hasher.ComputeHash(name).Append(Hasher.ComputeHash(pass))).ToChars();
        }
    }
}
