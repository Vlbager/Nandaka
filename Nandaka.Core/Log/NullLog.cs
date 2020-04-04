namespace Nandaka.Core
{
    public class NullLog : ILog
    {
        public NullLog() { }
        public void AppendMessage(LogMessageType type, string message) { }
    }
}