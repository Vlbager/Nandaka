using System;
using System.Collections.Generic;
using System.Text;

namespace Nandaka.Core
{
    public interface ISession<T>
    {
        IDevice Device { get; }
        IProtocol<T> Protocol { get; }
        void EnqueueRegisters(IEnumerable<IRegister> registers, MessageType operationType);
        void SendMessage();
    }
}
