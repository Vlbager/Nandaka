using System;
using System.Collections.Generic;
using System.Text;

namespace Nandaka
{
    public interface IProtocol
    {
        void SendMessage(IProtocolMessage message);

        event EventHandler<IProtocolMessage> MessageReceived;
    }
}
