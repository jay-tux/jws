using System;
using Jay.IO.Logging;
using System.Linq;
using System.Text;

namespace Jay.Web.Server
{
    public static class Templating
    {
        public static string MIMETemplate;
        public static string MIMEData;

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

            Program.Logger.LogFormatted("_hook_template", "Attempting to hook into Comms (2 hooks).", LogSeverity.Debug);
            Response.Hook(
                ((req, resp) => resp.MIMEType == MIMETemplate),
                ((req, resp) => {
                    resp.MIMEType = "text/html";
                    resp.Buffer = Encoding.UTF8.GetBytes(resp.LoadError(403));
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
            //
        }
    }
}
