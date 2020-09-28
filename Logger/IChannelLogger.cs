using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Jay.IO.Logging
{
    public interface IChannelLogger : ILogger
    {
        void AddChannel(TextWriter target);
        TextWriter GetChannel(int index);
        void RemoveChannel(int index);
        void SetDefault(int index);
        TextWriter GetDefault();
        void AddPredicate(Func<string, LogSeverity, bool> predicate, int index);
        void RemovePredicates(int index);
    }

    public class SimpleChannelLogger : IChannelLogger
    {
        public static SimpleChannelLogger Instance = new SimpleChannelLogger();

        private List<TextWriter> _channels;
        private int _current;

        private SimpleChannelLogger()
        {
            _channels = new List<TextWriter>() { Console.Out };
            _current = 0;
        }

        public void AddChannel(TextWriter target) => _channels.Add(target);
        public TextWriter GetChannel(int index) => _channels[index];
        public TextWriter GetDefault() => _channels[_current];
        public void AddPredicate(Func<string, LogSeverity, bool> predicate, int index) => throw new NotImplementedException();
        public void RemovePredicates(int index) => throw new NotImplementedException();

        public void RemoveChannel(int index)
        {
            if(_channels.Count == 1) throw new ArgumentException("Can't remove the last channel.", "index");
            _channels.RemoveAt(index);
        }

        public void SetDefault(int index)
        {
            if(index >= _channels.Count) throw new ArgumentException("Can't set default. Index out of range.", "index");
            _current = index;
        }

        public void Log(string message) => Log(message, LogSeverity.Message);
        public void Log(object message) => Log(message.ToString());
        public void Log(object message, LogSeverity severity) => Log(message.ToString(), severity);
        public void Log(string message, LogSeverity severity) =>
            GetDefault().WriteLine($"{severity.ToString().ToUpper()}\t{message}");
    }

    public class SimplePredicateLogger : IChannelLogger
    {
        public static SimplePredicateLogger Instance = new SimplePredicateLogger();

        private TextWriter _stdout;
        private TextWriter _stderr;
        private List<Func<string, LogSeverity, bool>> _stdoutPred;
        private List<Func<string, LogSeverity, bool>> _stderrPred;

        private SimplePredicateLogger()
        {
            _stdout = Console.Out;
            _stderr = Console.Error;
            _stdoutPred = new List<Func<string, LogSeverity, bool>>() {
                (s, l) => (l == LogSeverity.Message || l == LogSeverity.Debug)
            };
            _stderrPred = new List<Func<string, LogSeverity, bool>>() {
                (s, l) => (l == LogSeverity.Error || l == LogSeverity.Warning)
            };
        }

        public void AddChannel(TextWriter target) => throw new NotImplementedException();
        public TextWriter GetDefault() => throw new NotImplementedException();
        public void RemoveChannel(int index) => throw new NotImplementedException();
        public void SetDefault(int index) => throw new NotImplementedException();

        public TextWriter GetChannel(int index)
        {
            switch(index)
            {
                case 0: return _stdout;
                case 1: return _stderr;
                default: throw new ArgumentException("Index out of range.", "index");
            }
        }

        public void AddPredicate(Func<string, LogSeverity, bool> predicate, int index)
        {
            switch(index)
            {
                case 0: _stdoutPred.Add(predicate); break;
                case 1: _stderrPred.Add(predicate); break;
                default: throw new ArgumentException("Index out of range.", "index");
            }
        }

        public void RemovePredicates(int index)
        {
            switch(index)
            {
                case 0: _stdoutPred.Clear(); break;
                case 1: _stderrPred.Clear(); break;
                default: throw new ArgumentException("Index out of range.", "index");
            }
        }

        public void Log(string message) => Log(message, LogSeverity.Message);
        public void Log(object message) => Log(message.ToString());
        public void Log(object message, LogSeverity severity) => Log(message.ToString(), severity);

        public void Log(string message, LogSeverity severity)
        {
            string fmat = $"{severity.ToString().ToUpper()}\t{message}";
            if(_stdoutPred.Any(x => x(message, severity))) _stdout.WriteLine(fmat);
            if(_stderrPred.Any(x => x(message, severity))) _stderr.WriteLine(fmat);
        }
    }
}
