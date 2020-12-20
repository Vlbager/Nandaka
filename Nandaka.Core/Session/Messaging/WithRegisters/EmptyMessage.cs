using System;
using System.Collections.Generic;
using Nandaka.Core.Table;

namespace Nandaka.Core.Session
{
    public class EmptyMessage : IRegisterMessage
    {
        public int SlaveDeviceAddress => 0;
        public MessageType MessageType => MessageType.None;
        public IReadOnlyCollection<IRegisterGroup> RegisterGroups => Array.Empty<IRegisterGroup>();
        public OperationType OperationType => OperationType.None;
        
        public EmptyMessage() { }
    }
}