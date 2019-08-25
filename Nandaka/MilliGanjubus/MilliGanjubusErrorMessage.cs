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
        
        public int DeviceAddress { get; private set; }

        public MilliGanjubusErrorMessage(MilliGanjubusErrorType type, int deviceAddress)
        {
            DeviceAddress = deviceAddress;
            ErrorType = type;
        }
    }
}
