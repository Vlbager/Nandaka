using System;
using System.Collections.Generic;
using System.Text;

namespace Nandaka.Protocol
{
    public interface IParser<in T>
    {
        void Parse(T data);

        event EventHandler<IProtocolMessage> MessageParsed;
    }
}
