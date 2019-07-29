using System;
using System.Collections.Generic;
using System.Text;

namespace Nandaka.Protocol
{
    public interface IComposer<out T>
    {
        T Compose(IProtocolMessage message);
    }
}
