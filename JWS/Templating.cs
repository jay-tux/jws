using System;
using Jay.IO.Logging;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.IO;
using Jay.Ext;

namespace Jay.Web.Server
{
    public static class Templating
    {
        public static string MIMETemplate;
        public static string MIMEData;
        public static string TemplatePrefix;
        public static string TemplateDir;
        public static string Indexing;

        public static void Load()
        {
            try
            {
                object s = Program.Settings["JWS.Templates.State"];
                if(s is string state)
                {
                    switch(s)
                    {
                        case "off":
                            Program.Logger.LogFormatted("_hook_template", "Deactivating templating system.", LogSeverity.Message);
                            return;
                        case "on":
                            Program.Logger.LogFormatted("_hook_template", "Activating templating system.", LogSeverity.Message);
                            break;
                        default:
                            Program.Logger.LogFormatted("_hook_template", "Templating state (JWS.Templates.State) incorrectly defined. Expecting on or off. Using fallback off.",
                                LogSeverity.Warning);
                            return;
                    }
                }
                else
                {
                    Program.Logger.LogFormatted("_hook_template", "Templating state (JWS.Templates.State) should be a string. Using fallback off.", LogSeverity.Warning);
                    return;
                }
            }
            catch(ArgumentException)
            {
                Program.Logger.LogFormatted("_hook_template", "Templating state is not defined (expecting in JWS.Templates.State). Using fallback off.", LogSeverity.Warning);
                return;
            }

            try
            {
                object p = Program.Settings["JWS.Paths.Template"];
                if(p is string path)
                {
                    try
                    {
                        path = path.Replace("@Home", Program.GetHome()).Replace("@Data", Program.Data());
                        if(Directory.Exists(path))
                        {
                            if(path.EndsWith("/")) path = path.Substring(0, path.Length - 1);
                            TemplateDir = path;
                            Program.Logger.LogFormatted("_hook_template", $"Set Template Directory to {path}.", LogSeverity.Message);
                        }
                        else
                        {
                            Program.Logger.LogFormatted("_hook_template", $"Template dir in config file ({path}) doesn't exist. No fallback available.", LogSeverity.Error);
                            Program.Instance.Exit(3);
                        }
                    }
                    catch(IOException)
                    {
                        Program.Logger.LogFormatted("Listener", $"Template dir not defined in config file. No fallback available.", LogSeverity.Error);
                        Program.Instance.Exit(3);
                    }
                }
            }
            catch(ArgumentException)
            {
                string pth = Program.GetHome() + "/.config/jws/template";
                Program.Logger.LogFormatted("_hook_template", $"Template Dir not configured in config file (JWS.Paths.Template). Trying fallback {pth}.",
                    LogSeverity.Warning);
                try
                {
                    if(Directory.Exists(pth))
                    {
                        TemplateDir = pth;
                        Program.Logger.LogFormatted("_hook_template", $"Set Template Dir to {pth}.", LogSeverity.Message);
                    }
                    else
                    {
                        Program.Logger.LogFormatted("_hook_template", $"Fallback Template Dir doesn't exist. No fallback available.", LogSeverity.Error);
                        Program.Instance.Exit(3);
                    }
                }
                catch(IOException)
                {
                    Program.Logger.LogFormatted("_hook_template", $"Fallback Template Dir doesn't exist. No fallback available.", LogSeverity.Error);
                    Program.Instance.Exit(3);
                }
            }

            try
            {
                object t = Program.Settings["JWS.Templates.Assoc"];
                if(t is string temp)
                {
                    MIMETemplate = temp;
                    Program.Logger.LogFormatted("_hook_template", $"Template MIME type successfully set to {temp}.", LogSeverity.Message);
                }
                else
                {
                    Program.Logger.LogFormatted("_hook_template", "Template MIME type (JWS.Templates.Assoc) should be a string. Using fallback text/jwstemplate.",
                        LogSeverity.Warning);
                    MIMETemplate = "text/jwstemplate";
                }
            }
            catch(ArgumentException)
            {
                Program.Logger.LogFormatted("_hook_template", "Template MIME type (JWS.Templates.Assoc) not defined. Using fallback text/jwstemplate.", LogSeverity.Warning);
                MIMETemplate = "text/jwstemplate";
            }

            try
            {
                object d = Program.Settings["JWS.Templates.Data"];
                if(d is string data)
                {
                    MIMEData = data;
                    Program.Logger.LogFormatted("_hook_template", $"Template data MIME type successfully set to {data}.", LogSeverity.Message);
                }
                else
                {
                    Program.Logger.LogFormatted("_hook_template", "Template data MIME type (JWS.Templates.Data) should be a string. Using fallback text/jwsdata.",
                        LogSeverity.Warning);
                    MIMEData = "text/jwsdata";
                }
            }
            catch(ArgumentException)
            {
                Program.Logger.LogFormatted("_hook_template", "Template data MIME type (JWS.Templates.Data) not defined. Using fallback text/jwsdata.", LogSeverity.Warning);
                MIMEData = "text/jwsdata";
            }

            try
            {
                object p = Program.Settings["JWS.Templates.Prefix"];
                if(p is string pref)
                {
                    TemplatePrefix = pref;
                    Program.Logger.LogFormatted("_hook_template", $"Set prefix to {pref}.", LogSeverity.Debug);
                }
                else
                {
                    TemplatePrefix = "$";
                    Program.Logger.LogFormatted("_hook_template", "Prefix should be a string (JWS.Templates.Prefix). Using fallback $.", LogSeverity.Warning);
                }
            }
            catch(ArgumentException)
            {
                TemplatePrefix = "$";
                Program.Logger.LogFormatted("_hook_template", "Prefix not defined (JWS.Templates.Prefix). Using fallback $.", LogSeverity.Warning);
            }

            try
            {
                object i = Program.Settings["JWS.Templates.Indexing"];
                if(i is string index)
                {
                    Indexing = index;
                    Program.Logger.LogFormatted("_hook_template", $"Set indexing operator to {index}.", LogSeverity.Debug);
                }
                else
                {
                    Indexing = "::";
                    Program.Logger.LogFormatted("_hook_template", "Indexing operator should be a string (JWS.Templates.Indexing). Using fallback ::.", LogSeverity.Warning);
                }
            }
            catch(ArgumentException)
            {
                Indexing = "::";
                Program.Logger.LogFormatted("_hook_template", "Indexing operator not defined (JWS.Templates.Indexing). Using fallback ::.", LogSeverity.Warning);
            }

            Program.Logger.LogFormatted("_hook_template", "Attempting to hook into Comms (2 hooks).", LogSeverity.Debug);
            Response.Hook(
                ((req, resp) => resp.MIMEType == MIMETemplate),
                ((req, resp) => {
                    Program.Logger.LogFormatted("Template", $"Client requested template page {req.Path}. Blocking.", LogSeverity.Message);
                    resp.MIMEType = "text/html";
                    resp.Content = resp.LoadError(403);
                    resp.StatusCode = 403;
                })
            );
            Program.Logger.LogFormatted("_hook_template", "Added MIME block hook.", LogSeverity.Debug);
            Response.Hook(
                ((req, resp) => resp.MIMEType == MIMEData),
                ((req, resp) => Fill(req, resp))
            );
            Program.Logger.LogFormatted("_hook_template", "Added MIME translation hook.", LogSeverity.Debug);
            Program.Logger.LogFormatted("_hook_template", "Templating hooked!", LogSeverity.Message);
        }

        public static void Fill(Request got, Response send)
        {
            Program.Logger.LogFormatted("Template", $"Filling template for URL {got.Path}.", LogSeverity.Message);
            Dictionary<string, string> vars = new Dictionary<string, string>() {
                [$"{TemplatePrefix}server"] = Listener.ServerName,
                [$"{TemplatePrefix}page"] = got.Path
            };
            vars[$"{TemplatePrefix}AllGET"] = string.Join("<br />\n", got.Queries.Select((k, v) => $"{k} = {v}"));
            vars[$"{TemplatePrefix}AllPOST"] = string.Join("<br />\n", got.POST.Select(kvp => $"{kvp.Key} = {kvp.Value}"));
            got.Queries.ForEach((key, value) => {
                vars[$"{TemplatePrefix}GET{Indexing}{key}"] = value;
            });
            got.POST.ForEach(kvp => {
                vars[$"{TemplatePrefix}POST{Indexing}{kvp.Key}"] = kvp.Value;
            });
            send.Content.Split('\n').ForEach(line => {
                if(line.Contains('='))
                {
                    vars[$"{TemplatePrefix}{line.Split('=')[0].Trim()}"] = line.Split('=')[1].Trim();
                }
                else
                {
                    Program.Logger.LogFormatted("Template", $"Issue in file {got.Path}: line '{line}' doesn't contain a variable definition.", LogSeverity.Warning);
                }
            });

            if(vars.ContainsKey($"{TemplatePrefix}template"))
            {
                try
                {
                    if(File.Exists($"{TemplateDir}/{vars[$"{TemplatePrefix}template"]}"))
                    {
                        string data = File.ReadAllText($"{TemplateDir}/{vars[$"{TemplatePrefix}template"]}");
                        vars[$"{TemplatePrefix}template"] = vars[$"{TemplatePrefix}template"].Split('.')[0];
                        vars.ForEach(kvp => {
                            Program.Logger.LogFormatted("Template", $"Running replacements for {kvp.Key} (replacing with {kvp.Value}).", LogSeverity.Debug);
                            data = data.Replace(kvp.Key, kvp.Value);
                        });

                        send.Content = data;
                        send.MIMEType = "text/html";
                        send.StatusCode = 200;
                    }
                    else
                    {
                        Program.Logger.LogFormatted("Template", $"Issue in file {got.Path}: template file {TemplateDir}/{vars[$"{TemplatePrefix}template"]} does not exist.",
                            LogSeverity.Warning);
                        send.Content = send.LoadError(500);
                        send.MIMEType = "text/html";
                        send.StatusCode = 500;
                    }
                }
                catch(IOException)
                {
                    Program.Logger.LogFormatted("Template", $"Issue in file {got.Path}: template file {TemplateDir}/{vars[$"{TemplatePrefix}template"]} does not exist.",
                        LogSeverity.Warning);
                    send.Content = send.LoadError(500);
                    send.MIMEType = "text/html";
                    send.StatusCode = 500;
                }
            }
            else
            {
                Program.Logger.LogFormatted("Template", $"Issue in file {got.Path}: template not defined.", LogSeverity.Warning);
                send.Content = send.LoadError(500);
                send.MIMEType = "text/html";
                send.StatusCode = 500;
            }
        }
    }
}
