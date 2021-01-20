using Nandaka.Core.Logging;
using Nandaka.Core.Session;

namespace Nandaka.Core.Device
{
    public class NullSpecificMessageHandler : ISpecificMessageHandler
    {
        public void OnSpecificMessageReceived(ISpecificMessage message)
        {
            Log.AppendWarning($"Received specific message ({message.SpecificCode}) was not handled");
        }

        public void WaitResponse() { }
    }
}