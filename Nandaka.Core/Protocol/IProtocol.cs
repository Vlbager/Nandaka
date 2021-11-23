using System;
using System.Collections.Generic;
using Nandaka.Core.Registers;
using Nandaka.Core.Session;

namespace Nandaka.Core.Protocol
{
    public interface IProtocol
    {
        IProtocolInfo Info { get; }
        bool IsResponseMayBeSkipped { get; }
        bool IsAsyncRequestsAllowed { get; }
        SentMessageResult SendMessage(IMessage message);
        event EventHandler<MessageReceivedEventArgs> MessageReceived;
    }
}
