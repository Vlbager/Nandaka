using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Table;

namespace Nandaka.Core.Session
{
    public class CommonMessage : IRegisterMessage
    {
        public int SlaveDeviceAddress { get; }
        public MessageType Type { get; }
        public OperationType OperationType { get; }
        public IReadOnlyCollection<IRegisterGroup> RegisterGroups { get; }

        public CommonMessage(int slaveDeviceAddress, MessageType type, OperationType operationType,
            IEnumerable<IRegisterGroup> registerGroups)
        {
            SlaveDeviceAddress = slaveDeviceAddress;
            Type = type;
            OperationType = operationType;
            RegisterGroups = registerGroups.ToArray();
        }

        public CommonMessage(int slaveDeviceAddress, MessageType type, OperationType operationType)
        : this(slaveDeviceAddress, type, operationType, Enumerable.Empty<IRegisterGroup>()) { }
    }
}
