using System.Collections.Generic;
using Nandaka.Core.Protocol;
using Nandaka.Core.Table;

namespace Nandaka.Core.Session
{
    public interface ISession<T>
    {
        IDevice Device { get; }
        IProtocol<T> Protocol { get; }
        void EnqueueRegisters(IEnumerable<IRegisterGroup> registers, MessageType operationType);
        void SendMessage();
    }
}
