using System;
using System.Collections.Generic;
using Nandaka.Core.Table;

namespace Nandaka.Core.Protocol
{
    public interface IProtocol<T>
    {
        IMessage GetMessage(IEnumerable<IRegisterGroup> registers, int deviceAddress, MessageType type, int errorCode = 0);
        T PreparePacket(IMessage message);

        void SendPacket(T packet);

        event EventHandler<IMessage> MessageReceived;
    }
}
