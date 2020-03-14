﻿using System.Collections.Generic;
using Nandaka.Core.Network;
using Nandaka.Core.Protocol;
using Nandaka.Core.Session;
using Nandaka.Core.Table;
using Nandaka.MilliGanjubus.Models;

namespace Nandaka.MilliGanjubus
{
    public class MilliGanjubus : ProtocolBase<byte[]>
    {
        public MilliGanjubus(IDataPortProvider<byte[]> dataPortProvider, IComposer<IRegisterMessage, byte[]> composer,
            IParser<byte[], IRegisterMessage> parser) : base(dataPortProvider, composer, parser)
        {
        }

        public override IRegisterMessage GetMessage(IEnumerable<IRegisterGroup> registers, int deviceAddress, MessageType type, int errorCode = 0)
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
