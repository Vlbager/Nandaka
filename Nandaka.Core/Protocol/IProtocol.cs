using System;
using Nandaka.Core.Session;

namespace Nandaka.Core.Protocol
{
    public interface IProtocol<T>
    {
        T PreparePacket(IFrameworkMessage message);

        void SendPacket(T packet);

        event EventHandler<IFrameworkMessage> MessageReceived;
    }
}
