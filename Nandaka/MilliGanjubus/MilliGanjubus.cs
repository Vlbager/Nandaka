using System;
using System.Collections.Generic;
using System.Text;

namespace Nandaka.MilliGanjubus
{
    public class MilliGanjubus : ProtocolBase<byte[]>
    {
        public MilliGanjubus(IDataPortProvider<byte[]> dataPortProvider, IComposer<byte[]> composer,
            IParser<byte[], IProtocolMessage> parser) : base(dataPortProvider, composer, parser)
        {
        }
    }
}
