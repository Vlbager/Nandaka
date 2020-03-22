using Nandaka.Core.Device;
using Nandaka.Core.Protocol;

namespace Nandaka.Core.Session
{
    public interface ISession
    {
        IProtocol Protocol { get; }
        void SendMessage(SlaveDevice device);
    }
}
