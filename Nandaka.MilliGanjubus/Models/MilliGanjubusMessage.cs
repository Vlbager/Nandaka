using System;
using System.Collections.Generic;
using Nandaka.Core.Protocol;
using Nandaka.Core.Table;

namespace Nandaka.MilliGanjubus.Models
{
    public class MilliGanjubusMessage : IMessage
    {
        private readonly List<IRegister> _registers = new List<IRegister>();

        public int DeviceAddress { get; }

        public MilliGanjubusMessage(MessageType messageType, int deviceAddress, int errorCode = 0)
        {
            MessageType = messageType;
            DeviceAddress = deviceAddress;
            ErrorCode = errorCode;
        }

        public IEnumerable<IRegister> Registers => _registers;
        public int RegistersCount => _registers.Count;

        public void AddRegister(IRegister register)
        {
            _registers.Add(register);
        }

        public void RemoveRegister(IRegister register)
        {
            if (!_registers.Contains(register))
            {
                // todo: Create a custom exception
                throw new ArgumentException("Message do not contain this register");
            }

            _registers.Remove(register);
        }

        public MessageType MessageType { get;}

        public int ErrorCode { get; }
    }
}
