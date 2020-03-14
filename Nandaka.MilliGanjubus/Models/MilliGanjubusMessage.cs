using System;
using System.Collections.Generic;
using Nandaka.Core.Session;
using Nandaka.Core.Table;

namespace Nandaka.MilliGanjubus.Models
{
    public class MilliGanjubusMessage : IRegisterMessage
    {
        private readonly List<IRegisterGroup> _registers = new List<IRegisterGroup>();

        public int SlaveDeviceAddress { get; }

        public MilliGanjubusMessage(MessageType messageType, int deviceAddress, int errorCode = 0)
        {
            Type = messageType;
            SlaveDeviceAddress = deviceAddress;
            ErrorCode = errorCode;
        }

        public IEnumerable<IRegisterGroup> Registers => _registers;
        public int RegistersCount => _registers.Count;

        public void AddRegister(IRegisterGroup registerGroup)
        {
            _registers.Add(registerGroup);
        }

        public void RemoveRegister(IRegisterGroup registerGroup)
        {
            if (!_registers.Contains(registerGroup))
            {
                // todo: Create a custom exception
                throw new ArgumentException("Message do not contain this registerGroup");
            }

            _registers.Remove(registerGroup);
        }

        public MessageType Type { get;}

        public int ErrorCode { get; }
    }
}
