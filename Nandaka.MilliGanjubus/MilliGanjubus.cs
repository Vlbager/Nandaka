using Nandaka.Core.Network;
using Nandaka.Core.Protocol;
using Nandaka.Core.Session;

namespace Nandaka.MilliGanjubus
{
    public class MilliGanjubus : ProtocolBase<byte[]>
    {
        public MilliGanjubus(IDataPortProvider<byte[]> dataPortProvider, IComposer<IFrameworkMessage, byte[]> composer,
            IParser<byte[], IFrameworkMessage> parser) : base(dataPortProvider, composer, parser)
        {
        }
    }
}
