using System;

namespace Nandaka.Core.Logging
{
    public sealed class PrefixLog : ILog
    {
        private readonly ILog _inner;
        private readonly string _prefix;

        public PrefixLog(ILog innerLog, string prefix)
        {
            _inner = innerLog;
            _prefix = prefix;
        }

        public PrefixLog(string prefix) 
            : this(Log.Instance, prefix)
        { }

        public void AppendMessage(string message)
        {
            _inner.AppendMessage(GetMessageWithPrefix(message));
        }
        
        public void AppendMessage(LogLevel logLevel, string message)
        {
            _inner.AppendMessage(logLevel, GetMessageWithPrefix(message));
        }
        
        public void AppendWarning(string message)
        {
            _inner.AppendWarning(GetMessageWithPrefix(message));
        }
        
        public void AppendWarning(LogLevel logLevel, string message)
        {
            _inner.AppendWarning(logLevel, GetMessageWithPrefix(message));
        }

        public void AppendException(Exception exception, string message)
        {
            _inner.AppendException(exception, GetMessageWithPrefix(message));
        }
        
        public void AppendException(LogLevel logLevel, Exception exception, string message)
        {
            _inner.AppendException(exception, GetMessageWithPrefix(message));
        }

        private string GetMessageWithPrefix(string message)
        {
            return $"{_prefix} {message}";
        }
    }
}