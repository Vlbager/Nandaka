using System.Collections.Generic;
using Nandaka.Core.Table;

namespace Nandaka.Core.Session
{
    public class ReceivedMessage : IRawRegisterMessage
    {
        public int SlaveDeviceAddress { get; }
        public MessageType Type { get; }
        public IReadOnlyCollection<IRegister> Registers { get; }
        public OperationType OperationType { get; }

        public ReceivedMessage(int slaveDeviceAddress, MessageType type, OperationType operationType, IReadOnlyCollection<IRegister> registers)
        {
            SlaveDeviceAddress = slaveDeviceAddress;
            Type = type;
            OperationType = operationType;
            Registers = registers;
        }
    }
}