using Nandaka.Core.Network;
using Nandaka.Core.Protocol;
using Nandaka.Core.Session;
using Nandaka.MilliGanjubus.Components;

namespace Nandaka.MilliGanjubus
{
    public class MilliGanjubus : ProtocolBase<byte[]>
    {
        private MilliGanjubus(IDataPortProvider<byte[]> dataPortProvider, IComposer<IMessage, byte[]> composer,
            IParser<byte[], MessageReceivedEventArgs> parser) : base(dataPortProvider, composer, parser)
        {
        }

        public static MilliGanjubus Create(IDataPortProvider<byte[]> dataPortProvider)
        {
            var protocolInfo = new MilliGanjubusInfo();
            return new MilliGanjubus(dataPortProvider, new MilliGanjubusComposer(protocolInfo), new MilliGanjubusApplicationParser(protocolInfo));
        }
    }
}
