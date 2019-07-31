using System;
using System.Collections.Generic;
using System.Text;

namespace Nandaka.Protocol
{
    public sealed class MilliGanjubusComposer : MilliGanjubusProtocolInfo, IComposer<byte[]>
    {
        readonly MilliGanjubusProtocolInfo _protocolInfo;

        public MilliGanjubusComposer()
        {

        }

        public byte[] Compose(IProtocolMessage message)
        {
            
            //todo: who should resolve problem with oversized messages?
            throw new NotImplementedException();
        }
    }
}
