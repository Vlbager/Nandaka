namespace Nandaka.Core
{
    public class PrefixLog : ILog
    {
        private readonly ILog _inner;
        private readonly string _prefix;

        public PrefixLog(ILog innerLog, string prefix)
        {
            _inner = innerLog;
            _prefix = prefix;
        }

        public void AppendMessage(LogMessageType type, string message)
        {
            _inner.AppendMessage(type, _prefix + message);
        }
    }
}