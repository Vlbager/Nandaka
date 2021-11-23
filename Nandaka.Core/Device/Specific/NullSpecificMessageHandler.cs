using Microsoft.Extensions.Logging;
using Nandaka.Core.Session;

namespace Nandaka.Core.Device
{
    public class NullSpecificMessageHandler : ISpecificMessageHandler
    {
        public void OnSpecificMessageReceived(ISpecificMessage message, ILogger logger)
        {
            logger.LogWarning("Received specific message was not handled: {0}", message);
        }

        public void WaitResponse() { }
    }
}