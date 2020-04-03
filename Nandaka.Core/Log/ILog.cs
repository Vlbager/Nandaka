namespace Nandaka.Core
{
    public interface ILog
    {
        void AppendMessage(LogMessageType type, string message);
    }
}