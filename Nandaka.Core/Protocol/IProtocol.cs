using System;
using System.Collections.Generic;
using Nandaka.Core.Registers;
using Nandaka.Core.Session;

namespace Nandaka.Core.Protocol
{
    public interface IProtocol
    {
        void SendAsPossible(IRegisterMessage message, out IReadOnlyList<IRegister> sentRegisters);
        void SendMessage(IMessage message);
        event EventHandler<MessageReceivedEventArgs> MessageReceived;
    }
}
