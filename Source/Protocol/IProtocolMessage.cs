using System;
using System.Collections.Generic;
using System.Text;

namespace Nandaka.Protocol
{
    public interface IProtocolMessage
    {
        IEnumerable<IRegister> Registers { get; }
        MessageType MessageType { get; }
    }
}
