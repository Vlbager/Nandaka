using Nandaka.Core.Session;

namespace Nandaka.Core.Device
{
    public class NullSpecificMessageHandler : ISpecificMessageHandler
    {
        public void OnSpecificMessageReceived(ISpecificMessage message)
        {
            Log.Instance.AppendMessage(LogMessageType.Warning, $"Received specific message ({message.SpecificCode}) was not handled");
        }

        public void WaitResponse() { }
    }
}