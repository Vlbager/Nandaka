using System;
using System.Collections.Generic;
using Nandaka.Core.Helpers;
using Nandaka.Core.Registers;
using Nandaka.Core.Session;
using Nandaka.Model.Registers;

namespace Nandaka.MilliGanjubus.Models
{
    internal sealed class MgRegisterMessage : IRegisterMessage
    {
        public IReadOnlyList<IRegister<byte>> MgRegisters { get; }

        public int SlaveDeviceAddress { get; }
        public MessageType MessageType { get; }
        public IReadOnlyList<IRegister> Registers => MgRegisters;
        public OperationType OperationType { get; }

        public MgRegisterMessage(int slaveDeviceAddress, MessageType messageType, OperationType operationType, 
                                 IReadOnlyList<IRegister<byte>> mgRegisters)
        {
            SlaveDeviceAddress = slaveDeviceAddress;
            MessageType = messageType;
            OperationType = operationType;
            MgRegisters = mgRegisters;
        }

        public static MgRegisterMessage Convert(IRegisterMessage registerMessage, IReadOnlyList<IRegister<byte>> byteRegisters)
        {
            return new MgRegisterMessage(registerMessage.SlaveDeviceAddress, registerMessage.MessageType, registerMessage.OperationType, byteRegisters);
        }
        
        public CommonMessage ConvertToCommon(IReadOnlyList<IRegister> registers)
        {
            return new CommonMessage(SlaveDeviceAddress, MessageType, OperationType, registers);
        }

        public override string ToString()
        {
            return $"Milliganjubus {MessageType.ToString()}-{OperationType.ToString()} to {SlaveDeviceAddress.ToString()} device:" +
                   $"{Environment.NewLine}{Registers.ToLogLine()}";
        }
    }
}