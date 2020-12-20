using System.Collections.Generic;
using Nandaka.Core.Table;

namespace Nandaka.Core.Session
{
    public class ReceivedRegisterMessage : IReceivedMessage
    {
        public int SlaveDeviceAddress { get; }
        public MessageType MessageType { get; }
        public IReadOnlyList<IRegister> Registers { get; }
        public OperationType OperationType { get; }

        public ReceivedRegisterMessage(int slaveDeviceAddress, MessageType type, OperationType operationType, IReadOnlyList<IRegister> registers)
        {
            SlaveDeviceAddress = slaveDeviceAddress;
            MessageType = type;
            OperationType = operationType;
            Registers = registers;
        }
    }
}