using System;
using System.Collections.Generic;
using Nandaka.Core.Session;
using Nandaka.Core.Table;

namespace Nandaka.Core.Protocol
{
    public interface IProtocol
    {
        void SendAsPossible(IRegisterMessage message, out IReadOnlyCollection<IRegisterGroup> sentGroups);
        void SendMessage(IMessage message);
        event EventHandler<MessageReceivedEventArgs> MessageReceived;
    }
}
