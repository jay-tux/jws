using System;
using Jay.Web.Server;
using Jay.IO.Logging;
using Jay.Config;

namespace Jay.Web.Server.Dynamic
{
    public class Dynamic
    {
        //private string GetCommand(string ext) {}
        public static void Load()
        {
            JcfResult<string> state = Program.Settings.GetString("JWS.Dynamic.Enabled");
            if(state)
            {
                if((string)state == "off")
                {
                    Program.Logger.LogFormatted("_boot_dyn", "Disabling Dynamic server...", LogSeverity.Message);
                    return;
                }
                else if((string)state != "on")
                {
                    Program.Logger.LogFormatted("_boot_dyn", "Setting JWS.Dynamic.Enabled should be either 'on' or 'off'. Assuming off.", LogSeverity.Warning);
                    return;
                }
            }
            else
            {
                Program.Logger.LogFormatted("_boot_dyn", "JWS.Dynamic.Enabled is not (correctly) set. Assuming off.", LogSeverity.Warning);
            }
        }
    }
}
