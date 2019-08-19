using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nandaka.MilliGanjubus
{
    public class MilliGanjubusErrorMessage : IProtocolMessage
    {
        public IEnumerable<IRegister> Registers => Enumerable.Empty<IRegister>();

        public MessageType MessageType => MessageType.ErrorMessage;

        public MilliGanjubusErrorType ErrorType { get; private set; }
        
        public int DeviceAddress { get; }

        public MilliGanjubusErrorMessage(MilliGanjubusErrorType type)
        {
            ErrorType = type;
        }
    }
}
