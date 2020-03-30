using Nandaka.Core.Session;

namespace Nandaka.Core.Device
{
    public interface ISpecificMessageHandler
    {
        void OnSpecificMessageReceived(ISpecificMessage message);
        void WaitResponse();
    }
}