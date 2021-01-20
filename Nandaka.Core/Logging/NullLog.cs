using System;

namespace Nandaka.Core.Logging
{
    public sealed class NullLog : ILog
    {
        public NullLog() { }
        public void Append(LogMessageType type, string message) { }
        public void Append(LogMessageType type, string message, LogLevel logLevel) { }
        public void AppendMessage(string message) { }
        public void AppendMessage(LogLevel logLevel, string message) { }
        public void AppendWarning(string message) { }
        public void AppendWarning(LogLevel logLevel, string message) { }
        public void AppendException(Exception exception, string message) { }
        public void AppendException(LogLevel logLevel, Exception exception, string message) { }
    }
}