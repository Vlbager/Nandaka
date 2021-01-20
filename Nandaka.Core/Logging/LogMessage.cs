using System;

namespace Nandaka.Core.Logging
{
    internal sealed class LogMessage
    {
        public int ThreadId { get; }
        public string Message { get; }
        public DateTime Time { get;  }
        public LogMessageType Type { get; }

        public LogMessage(int threadId, string message, LogMessageType type)
        {
            ThreadId = threadId;
            Message = message;
            Type = type;
            Time = DateTime.Now;
        }
    }
}