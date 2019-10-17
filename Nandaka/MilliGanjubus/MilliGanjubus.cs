using System;
using System.Collections.Generic;
using System.Text;

namespace Nandaka.MilliGanjubus
{
    public class MilliGanjubus : ProtocolBase<byte[]>
    {
        public MilliGanjubus(IDataPortProvider<byte[]> dataPortProvider, IComposer<IMessage, byte[]> composer,
            IParser<byte[], IMessage> parser) : base(dataPortProvider, composer, parser)
        {
        }

        public override IMessage GetMessage(IEnumerable<IRegister> registers, int deviceAddress, MessageType type, int errorCode = 0)
        {
            var message = new MilliGanjubusMessage(type, deviceAddress, errorCode);
            foreach (var register in registers)
            {
                message.AddRegister(register);
            }
            return message;
        }
    }
}
