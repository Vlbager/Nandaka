using System;
using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Registers;

namespace Nandaka.Core.Session
{
    public class CommonMessage : IRegisterMessage
    {
        public int SlaveDeviceAddress { get; }
        public MessageType MessageType { get; }
        public OperationType OperationType { get; }
        public IReadOnlyList<IRegister> Registers { get; }

        public CommonMessage(int slaveDeviceAddress, MessageType type, OperationType operationType,
                             IReadOnlyList<IRegister> registers)
        {
            SlaveDeviceAddress = slaveDeviceAddress;
            MessageType = type;
            OperationType = operationType;
            Registers = registers;
        }

        public CommonMessage(int slaveDeviceAddress, MessageType type, OperationType operationType)
        : this(slaveDeviceAddress, type, operationType, Array.Empty<IRegister>()) { }
    }
}
