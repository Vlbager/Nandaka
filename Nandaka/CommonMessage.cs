﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nandaka
{
    public class CommonMessage : IProtocolMessage
    {
        private readonly List<IRegister> _registers = new List<IRegister>();

        public CommonMessage(MessageType messageType)
        {
            MessageType = messageType;
        }

        public IEnumerable<IRegister> Registers => _registers;

        public void AddRegister(IRegister register)
        {
            _registers.Add(register);
        }

        public MessageType MessageType { get; private set; }
    }
}