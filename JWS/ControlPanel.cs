using System;

namespace Jay.Web.Server
{
    public class ControlPanel
    {
        public static void Load()
        {
            try
            {
                object s = Program.Settings["JWS.ControlPanel.State"];
                if(s is string state)
                {
                    if(s == "off")
                    {
                        Program.Logger.LogFormatted("_hook_cpanel", "Disabling ControlPanel...", LogSeverity.Message);
                        return;
                    }
                    else if(s != "on")
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
        }
    }
}
