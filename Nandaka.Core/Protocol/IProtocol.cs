using System;
using System.Collections.Generic;
using Nandaka.Core.Session;
using Nandaka.Core.Table;

namespace Nandaka.Core.Protocol
{
    public interface IProtocol
    {
        void SendMessage(IMessage message, out IReadOnlyCollection<IRegisterGroup> sentGroups);

        event EventHandler<MessageReceivedEventArgs> MessageReceived;
    }
}
