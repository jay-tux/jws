using System;
using System.IO;

namespace Jay.IO.Logging
{
    public interface IChannelLogger : ILogger
    {
        void AddChannel(StreamWriter target);
        StreamWriter GetChannel(int index);
        void RemoveChannel(int index);
        void SetDefault(int index);
        void AddPredicate(Func<string, LogSeverity, bool> predicate, int index);
        void RemovePredicates(int index);
    }
}
