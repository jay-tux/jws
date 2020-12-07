using System;
using Jay.Ext;
using Jay.IO.Logging;
using System.IO;
using System.Collections.Generic;

namespace Jay.Web.Server
{
    public class JWSChannelLogger : ILogger, IDisposable
    {
        public List<TextWriter> Channels;
        public List<List<Func<string, LogSeverity, bool>>> Predicates;
        private bool _disp = false;

        public JWSChannelLogger() {}

        public void Log(string message) => Log(message, LogSeverity.Message);
        public void Log(object message) => Log(message.ToString(), LogSeverity.Message);
        public void Log(object message, LogSeverity severity) => Log(message.ToString(), severity);
        public void Log(string message, LogSeverity severity)
        {
            Predicates.Enumerate((l, ind) =>
                l.ForEach(p => {
                        if(p(message, severity)) Channels[ind].WriteLine(message);
                })
            );
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disp)
        {
            if(!_disp)
            {
                Channels.ForEach(x => {
                    try
                    {
                        x.Dispose();
                    }
                    catch(ObjectDisposedException) {}
                    catch(Exception e)
                    {
                        Console.Error.WriteLine($"Failed to finalize one of the streams: {e.Message}.");
                    }
                });

                if(disp)
                {
                    Channels.Clear();
                    Predicates.Clear();
                    Channels = null;
                    Predicates = null;
                }

                _disp = true;
            }
        }

        ~JWSChannelLogger() => Dispose(false);
    }
}
