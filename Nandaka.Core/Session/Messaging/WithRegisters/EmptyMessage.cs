using System;
using System.Collections.Generic;
using Nandaka.Core.Registers;
using Nandaka.Model.Registers;

namespace Nandaka.Core.Session
{
    public sealed class EmptyMessage : IRegisterMessage
    {
        private static readonly EmptyMessage Instance = new EmptyMessage();

        private EmptyMessage()
        {
        }

        public static EmptyMessage Create() => Instance;
        
        public int SlaveDeviceAddress => 0;
        public MessageType MessageType => MessageType.None;
        public IReadOnlyList<IRegister> Registers => Array.Empty<IRegister>();
        public OperationType OperationType => OperationType.None;

        public override string ToString()
        {
            return "empty message";
        }
    }
}