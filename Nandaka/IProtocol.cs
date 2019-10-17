using System;
using System.Collections.Generic;
using System.Text;

namespace Nandaka
{
    public interface IProtocol<T>
    {
        IMessage GetMessage(IEnumerable<IRegister> registers, int deviceAddress, MessageType type, int errorCode = 0);
        T PreparePacket(IMessage message);

        void SendPacket(T packet);

        event EventHandler<IMessage> MessageReceived;
    }
}
