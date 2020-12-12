using System;

namespace Jay.Web.Server.Dynamic
{
    public class Dynamic
    {
        private string GetCommand(string ext) {}
        public static void Load()
        {
            try {
                object s = Program.Settings["JWS.Dynamic.Enabled"];
                if(s is string state)
                {
                    if(state == "off")
                    {
                        Program.Logger.LogFormatted("_boot_dyn", "Disabling Dynamic server...", LogSeverity.Message);
                        return;
                    }
                    else if(state != "on")
                    {
                        Program.Logger.LogFormatted("_boot_dyn", "Setting JWS.Dynamic.Enabled should be either 'on' or 'off'. Assuming off.", LogSeverity.Warning);
                        return;
                    }
                }
                else
                {
                    Program.Logger.LogFormatted("_boot_dyn", "JWS.Dynamic.Enabled should be a string. Assuming 'off'.", LogSeverity.Warning);
                    return;
                }
            }
        }
    }
}
