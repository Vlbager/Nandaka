using System;
using System.Collections.Generic;
using System.Text;

namespace Nandaka
{
    public interface IProtocolMessage
    {
        int DeviceAddress { get; }
        IEnumerable<IRegister> Registers { get; }
        MessageType MessageType { get; }
    }
}
