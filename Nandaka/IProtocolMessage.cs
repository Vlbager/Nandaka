using System;
using System.Collections.Generic;
using System.Text;

namespace Nandaka
{
    public interface IProtocolMessage
    {
        IEnumerable<IRegister> Registers { get; }
        MessageType MessageType { get; }
    }
}
