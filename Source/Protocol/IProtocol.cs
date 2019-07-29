using System;
using System.Collections.Generic;
using System.Text;

namespace Nandaka.Protocol
{
    public interface IProtocol
    {
        void SendMessage(IProtocolMessage message);

        event EventHandler<IProtocolMessage> MessageRecieved;
    }
}
