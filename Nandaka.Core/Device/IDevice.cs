using Nandaka.Core.Protocol;

namespace Nandaka.Core.Device
{
    public interface IDevice
    {
        IProtocol Protocol { get; }
    }
}
