using Nandaka.Core.Table;

namespace Nandaka.Core.Session
{
    public interface IFrameworkMessage
    {
        int SlaveDeviceAddress { get; }
        MessageType Type { get; }
    }
}
