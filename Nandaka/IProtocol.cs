using System;
using System.Collections.Generic;
using System.Text;

namespace Nandaka
{
    public interface IProtocol
    {
        void SendMessage(IMessage message);

        event EventHandler<IMessage> MessageReceived;
    }
}
