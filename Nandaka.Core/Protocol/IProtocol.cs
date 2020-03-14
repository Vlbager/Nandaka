using System;
using System.Collections.Generic;
using Nandaka.Core.Session;
using Nandaka.Core.Table;

namespace Nandaka.Core.Protocol
{
    public interface IProtocol<T>
    {
        IRegisterMessage GetMessage(IEnumerable<IRegisterGroup> registers, int deviceAddress, MessageType type, int errorCode = 0);
        T PreparePacket(IRegisterMessage message);

        void SendPacket(T packet);

        event EventHandler<IRegisterMessage> MessageReceived;
    }
}
