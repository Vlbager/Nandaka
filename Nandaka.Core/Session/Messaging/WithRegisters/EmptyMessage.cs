using System;
using System.Collections.Generic;
using Nandaka.Core.Registers;

namespace Nandaka.Core.Session
{
    public class EmptyMessage : IRegisterMessage
    {
        public int SlaveDeviceAddress => 0;
        public MessageType MessageType => MessageType.None;
        public IReadOnlyList<IRegister> Registers => Array.Empty<IRegister>();
        public OperationType OperationType => OperationType.None;
    }
}