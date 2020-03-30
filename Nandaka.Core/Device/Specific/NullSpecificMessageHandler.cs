using Nandaka.Core.Session;

namespace Nandaka.Core.Device
{
    public class NullSpecificMessageHandler : ISpecificMessageHandler
    {
        public void OnSpecificMessageReceived(ISpecificMessage message)
        {
            // todo: add logger
        }

        public void WaitResponse()
        {
            // todo: add logger
        }
    }
}