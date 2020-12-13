using System;
using System.IO;
using System.Net;
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
        private static Dictionary<string, string> Hashes;
        private static SHA256 Hasher;
        private static string CPath;
        private static List<string> Tokens;

        private static Dictionary<string, string> LoadHashes()
        {
            try
            {
                Program.Logger.LogFormatted("_cpanel_hashes", $"Loading Hashes from {Program.Data()}/jws/hs.", LogSeverity.Debug);
                if(!File.Exists(Program.Data() + "/jws/hs"))
                {
                    File.WriteAllText(Program.Data() + "/jws/hs", "admin>" + UserHash("admin", "admin"));
                }

                Dictionary<string, string> res = new Dictionary<string, string>();
                //List<string> res = new List<string>();
                //File.ReadAllLines(Program.Data() + "/jws/hs").ForEach(line => res.Add(line));
                File.ReadAllLines(Program.Data() + "/jws/hs").ForEach(line => {
                    string[] sp;
                    if(!line.Contains(">") || (sp = line.Split('>')).Length != 2)
                        Program.Logger.LogFormatted("_cpanel_hashes", $"Line {line} invalid. Should contain exactly one splitter element '>'.", LogSeverity.Warning);
                    else {
                        //Hashes[sp[0]] = sp[1];
                        res[sp[0]] = sp[1];
                        Program.Logger.LogFormatted("_cpanel_hashes", $"User {sp[0]}, hash {sp[1]}.", LogSeverity.Debug);
                    }
                });
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
            Tokens = new List<string>();
            Hashes = LoadHashes();

            JcfResult<string> state = Program.Settings.GetString("JWS.ControlPanel.State");
            if(state)
            {
                if((string)state == "off")
                {
                    Program.Logger.LogFormatted("_hook_cpanel", "Disabling ControlPanel...", LogSeverity.Message);
                    return;
                }
                else if((string)state != "on")
                {
                    Program.Logger.LogFormatted("_hook_cpanel", "Setting JWS.ControlPanel.State should be either 'on' or 'off'. Assuming off.", LogSeverity.Warning);
                    return;
                }
            }
            else
            {
                Program.Logger.LogFormatted("_hook_cpanel", "JWS.ControlPanel.State is not (correctly) defined. Assuming 'off'.", LogSeverity.Warning);
                return;
            }

            CPath = "/cpanel/";
            JcfResult<string> path = Program.Settings.GetString("JWS.ControlPanel.URL");
            if(path)
            {
                Program.Logger.LogFormatted("_hook_cpanel", $"Set Control Panel URL to {path}.", LogSeverity.Debug);
                CPath = (string)path;
            }
            else
            {
                Program.Logger.LogFormatted("_hook_cpanel", "Control Panel URL (JWS.ControlPanel.URL) not (correctly) set. Using fallback /cpanel/.", LogSeverity.Warning);
            }

            JcfResult<string> title = Program.Settings.GetString("JWS.ControlPanel.LoginTitle");
            if(title)
            {
                Program.Logger.LogFormatted("_hook_cpanel", $"Set Control Panel Login Title to {title}", LogSeverity.Debug);
                LoginTitle = (string)title;
            }
            else
            {
                Program.Logger.LogFormatted("_hook_cpanel", "JWS.ControlPanel.LoginTitle is not (correctly) defined. Using fallback Control Panel Login.", LogSeverity.Warning);
            }

            title = Program.Settings.GetString("JWS.ControlPanel.CPanelTitle");
            if(title)
            {
                Program.Logger.LogFormatted("_hook_cpanel", $"Set Control Panel Title to {title}", LogSeverity.Debug);
                CPanelTitle = (string)title;
            }
            else
            {
                Program.Logger.LogFormatted("_hook_cpanel", "JWS.ControlPanel.CPanelTitle is not (correctly) defined. Using fallback Control Panel.", LogSeverity.Warning);
            }

            JcfResult<string> footer = Program.Settings.GetString("JWS.ControlPanel.Footer");
            if(footer)
            {
                Program.Logger.LogFormatted("_hook_cpanel", $"Set Footer to {footer}", LogSeverity.Debug);
                Footer = (string)footer;
            }
            else
            {
                Program.Logger.LogFormatted("_hook_cpanel", "JWS.ControlPanel.Footer is not (correctly) defined. Using fallback Control Panel - Only for Authorized Users.", LogSeverity.Warning);
            }

            JcfResult<Jcf> css = Program.Settings.GetBlock("JWS.ControlPanel.CSS");
            if(css)
            {
                ((Jcf)css).EnumerateKeys((key, value) => {
                    CSS[key] = value;
                    Program.Logger.LogFormatted("_css_cpanel", $"Found CSS for {key}: {value}.", LogSeverity.Debug);
                });
            }
            else
            {
                Program.Logger.LogFormatted("_hook_cpanel", "No CSS defined for Control Panel. If you want to define any, use the JWS.ControlPanel.CSS block (maybe the block is not a block?).", LogSeverity.Message);
            }

            Program.Logger.LogFormatted("_hook_cpanel", "Hooking into Comms (1 hook).", LogSeverity.Debug);
            Response.Hook(
                (req, resp) => req.Path == CPath,
                (req, resp) => GenerateCPanel(req, resp)
            );
            Program.Logger.LogFormatted("_hook_cpanel", "Hooking into Shutdown (1 hook).", LogSeverity.Debug);
            Program.Instance.OnExit += (s, e) => {
                Program.Logger.LogFormatted("_hook_cpanel", "CPanel shutting down.", LogSeverity.Debug);
                try {
                    File.WriteAllLines(Program.Data() + "/jws/hs", Hashes.JoinPairs('>'));
                }
                catch(UnauthorizedAccessException)
                {
                    Console.Error.WriteLine("Can't open hashes file " + Program.Data() + "/jws/hs.");
                }
                catch(IOException)
                {
                    Console.Error.WriteLine("Can't open hashes file " + Program.Data() + "/jws/hs.");
                }
            };
            Program.Logger.LogFormatted("_hook_cpanel", "Hooks successful!", LogSeverity.Debug);
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
                    if(Hashes.ContainsValue(h))
                    {
                        Program.Logger.LogFormatted("CPanel", $"User {req.POST["un"]} succesfully authenticated.", LogSeverity.Message);
                        string tok = GenToken(req);
                        GenerateCPanelMain(res, tok);
                        res.Finish();
                    }
                    else
                    {
                        Program.Logger.LogFormatted("CPanel", $" === USER {req.POST["un"]} ATTEMPTED TO LOG IN BUT WAS UNSUCCESFUL ===", LogSeverity.Warning);
                        Program.Logger.LogFormatted("CPanel", $"Note: username = {req.POST["un"]}, hash = {h}", LogSeverity.Message);
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
            else
            {
                //token known
                if(Tokens.Contains(req.POST["token"]))
                {
                    Program.Logger.LogFormatted("CPanel", $"Authorized user connected.", LogSeverity.Message);
                    if(req.POST.ContainsKey("action") && req.POST["action"] == "prep")
                    {
                        if(req.POST.ContainsKey("file"))
                        {
                            GenerateCPanelFile(res, req.POST["token"], WebUtility.UrlDecode(req.POST["file"]));
                            res.Finish();
                        }
                        else
                        {
                            res.Content = res.LoadError(400);
                            res.StatusCode = 400;
                            res.Finish();
                        }
                    }
                    else if(req.POST.ContainsKey("action") && req.POST["action"] == "update")
                    {
                        if(req.POST.ContainsKey("file") && req.POST.ContainsKey("cnt"))
                        {
                            //update files
                            string file = WebUtility.UrlDecode(req.POST["file"]);
                            string content = WebUtility.UrlDecode(req.POST["cnt"]);
                            if(file == "cfg") Program.ConfigContent = content;
                            GenerateCPanelMain(res, req.POST["token"]);

                            try
                            {
                                File.WriteAllText(file, content);
                                Program.Logger.LogFormatted("cpanel_write", $"Updated {file}.", LogSeverity.Message);
                            }
                            catch(Exception ioe)
                            {
                                Program.Logger.LogFormatted("cpanel_write", $"Attempted to write to {file}; failed: {ioe.Message}.", LogSeverity.Warning);
                                res.Content.Replace("<div></div>", "<p class=error>Failed to write file.</p>");
                            }
                            res.Finish();
                        }
                        else
                        {
                            res.Content = res.LoadError(400);
                            res.StatusCode = 400;
                            res.Finish();
                        }
                    }
                    else if(req.POST.ContainsKey("action") && req.POST["action"] == "usrs")
                    {
                        GenerateCPanelUsers(res, req.POST["token"]);
                        res.Finish();
                    }
                    else if(req.POST.ContainsKey("action") && req.POST["action"] == "updusr")
                    {
                        //run updates
                        if(req.POST.ContainsKey("usrt"))
                        {
                            if(req.POST["usrt"] == "add" && req.POST.ContainsKey("uname") && !Hashes.ContainsKey(req.POST["uname"])
                                && req.POST.ContainsKey("pword"))
                            {
                                Hashes[req.POST["uname"]] = UserHash(req.POST["uname"], req.POST["pword"]);
                                GenerateCPanelMain(res, req.POST["token"]);
                            }
                            else if(req.POST["usrt"] == "rem" && req.POST.ContainsKey("user") && Hashes.ContainsKey(req.POST["user"]))
                            {
                                Hashes.Remove(req.POST["user"]);
                                GenerateCPanelMain(res, req.POST["token"]);
                            }
                            else
                            {
                                res.Content = res.LoadError(400);
                                res.StatusCode = 400;
                                res.Finish();
                            }
                        }
                        else
                        {
                            res.Content = res.LoadError(400);
                            res.StatusCode = 400;
                            res.Finish();
                        }
                    }
                    else if(req.POST.ContainsKey("action") && req.POST["action"] == "logout")
                    {
                        Tokens.Remove(req.POST["token"]);
                        res.Content = "";
                        res.StatusCode = 301;
                        res[HttpResponseHeader.Location] = "/";
                    }
                    else
                    {
                        GenerateCPanelMain(res, req.POST["token"]);
                        res.Finish();
                    }
                }
                else
                {
                    //invalid token
                    res.Content = res.LoadError(403);
                    res.StatusCode = 403;
                    res.Finish();
                }
            }
        }

        private static void GenerateCPanelFile(Response target, string token, string file)
        {
            bool success = true;
            string fcnt;
            try
            {
                fcnt = file == "Config file" ? Program.ConfigContent : (file == "New file" ? "" : File.ReadAllText(file));
            }
            catch(Exception)
            {
                success = false;
                fcnt = "";
            }
            string form = $"<!DOCTYPE html>\n<html>\n<head>\n<meta charset=utf-8 />\n<title>{Listener.ServerName} - {CPanelTitle}</title>\n" +
                (CSS.ContainsKey("Home") ? $"<link href=\"../{CSS["Home"]}\" rel=stylesheet type=\"text/css\" />\n" : "") +
                $"</head>\n<body><h1>{Listener.ServerName} - Control Panel</h1>\n<div class=center>\n<form action=\"{CPath}\" id=main method=POST>" +
                (file == "Config file" ? "<input type=hidden name=file value=cfg />" :
                    ("File path: <input type=text name=file " + (file == "New file" ? "" : $"value=\"{file}\" readonly") + " /><br />")) +
                $"<textarea name=cnt rows=40 cols=80 form=main>{fcnt}</textarea><br />\n" +
                ((success) ? "" : $"<p class=error>Couldn't load the requested file.</p><br />\n") +
                $"<input type=hidden name=token value=\"{token}\" /><input type=hidden name=action value=update /><br />\n" +
                $"<input type=submit value=\"Update File\" /></form>\n" +
                $"<form action=\"{CPath}\" id=lout method=POST><input type=hidden name=action value=logout /><input type=hidden name=token value=\"{token}\" />" +
                $"<input type=submit value=Logout /></form></div>" +
                $"<div class=footer>{Footer}</div>\n</body>\n</html>";
            target.Content = form;
            target.StatusCode = 200;
        }

        private static void GenerateCPanelUsers(Response target, string token)
        {
            string form = $"<!DOCTYPE html>\n<html>\n<head>\n<meta charset=utf-8 />\n<title>{Listener.ServerName} - {CPanelTitle}</title>\n" +
                (CSS.ContainsKey("Home") ? $"<link href=\"../{CSS["Home"]}\" rel=stylesheet type=\"text/css\" />\n" : "") +
                $"</head>\n<body><h1>{Listener.ServerName} - Control Panel</h1><div></div>\n<div class=center>\n" +
                $"\n<form action=\"{CPath}\" method=POST>" +
                $"<input type=hidden name=token value=\"{token}\" /><input type=hidden name=action value=updusr /><input type=hidden name=usrt value=add /><br />\n" +
                $"Username: <input type=text name=uname /><br />Password: <input type=password name=pword /><br /><input type=submit value=\"Add User\" /></form><br />" +
                $"<form action=\"{CPath}\" method=POST>" +
                $"<input type=hidden name=token value=\"{token}\" /><input type=hidden name=action value=updusr /><input type=hidden name=usrt value=rem /><br />" +
                $"<select name=user id=sl>{string.Join("", Hashes.Keys.Select(x => $"<option value=\"{x}\">{x}</option>"))}</select><br />" +
                $"<input type=submit value=\"Modify CPanel users\" />" +
                $"</form></div>\n" +
                $"<form action=\"{CPath}\" id=lout method=POST><input type=hidden name=action value=logout /><input type=hidden name=token value=\"{token}\" />" +
                $"<input type=submit value=Logout /></form>" +
                $"<div class=footer>{Footer}</div>\n</body>\n</html>";
            target.Content = form;
            target.StatusCode = 200;
        }

        private static void GenerateCPanelMain(Response target, string token)
        {
            string form = $"<!DOCTYPE html>\n<html>\n<head>\n<meta charset=utf-8 />\n<title>{Listener.ServerName} - {CPanelTitle}</title>\n" +
                (CSS.ContainsKey("Home") ? $"<link href=\"../{CSS["Home"]}\" rel=stylesheet type=\"text/css\" />\n" : "") +
                $"</head>\n<body><h1>{Listener.ServerName} - Control Panel</h1><div></div>\n<div class=center>\n" +
                $"\n<form action=\"{CPath}\" method=POST>" +
                $"<select name=file id=sl>{string.Join("", FileList().Select(x => $"<option value=\"{x}\">{x}</option>"))}</select>" +
                $"<input type=hidden name=token value=\"{token}\" /><input type=hidden name=action value=prep /><br />\n" +
                $"<input type=submit value=\"Open File\" /></form><br />" +
                $"<form action=\"{CPath}\" method=POST>" +
                $"<input type=hidden name=token value=\"{token}\" /><input type=hidden name=action value=usrs /><input type=submit value=\"Modify CPanel users\" />" +
                $"</form></div>\n" +
                $"<form action=\"{CPath}\" id=lout method=POST><input type=hidden name=action value=logout /><input type=hidden name=token value=\"{token}\" />" +
                $"<input type=submit value=Logout /></form>" +
                $"<div class=footer>{Footer}</div>\n</body>\n</html>";
            target.Content = form;
            target.StatusCode = 200;
        }

        private static IEnumerable<string> FileList()
        {
            yield return "New file";
            yield return "Config file";
            foreach(string entry in GetSystemEntries(Listener.HTMLDir)) yield return entry;
            foreach(string entry in GetSystemEntries(Listener.ErrorDir)) yield return entry;
            if(Templating.TemplateDir != null)
                foreach(string entry in GetSystemEntries(Templating.TemplateDir)) yield return entry;
        }

        private static IEnumerable<string> GetSystemEntries(string pth)
        {
            foreach(string f in Directory.EnumerateFiles(pth)) yield return f;
            foreach(string d in Directory.EnumerateDirectories(pth))
                foreach(string f in GetSystemEntries(d)) yield return f;
        }

        private static void GenerateLogin(Response target)
        {
            string form = $"<!DOCTYPE html>\n<html>\n<head>\n<meta charset=utf-8 />\n<title>{Listener.ServerName} - {LoginTitle}</title>\n" +
                (CSS.ContainsKey("Login") ? $"<link href=\"../{CSS["Login"]}\" rel=stylesheet type=\"text/css\" />\n" : "") +
                $"</head>\n<body><h1>{Listener.ServerName} - Control Panel</h1>\n<div class=center>\n<div class=frm>\n<h2>Log In</h2>\n" +
                $"<form action=\"{CPath}\" method=POST>\n<input type=hidden name=token value=\"-1\" /><b>Username<i class=error>*</i>: </b><input type=text name=un /><br />\n" +
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
            return Hasher.ComputeHash(Hasher.ComputeHash(name).Append(Hasher.ComputeHash(pass))).ToHex();
        }

        private static string GenToken(Request from)
        {
            byte[] name = Encoding.Unicode.GetBytes(from.POST["un"]);
            byte[] dt = Encoding.Unicode.GetBytes(DateTime.Now.ToString());
            string token = Hasher.ComputeHash(Hasher.ComputeHash(name).Append(Hasher.ComputeHash(dt))).ToHex();
            Tokens.Add(token);
            return token;
        }
    }
}
