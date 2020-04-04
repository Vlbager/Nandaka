using System.Collections.Generic;
using Nandaka.Core.Table;

namespace Nandaka.Core.Session
{
    public interface IRawRegisterMessage : IMessage
    {
        IReadOnlyCollection<IRegister> Registers { get; }
        OperationType OperationType { get; }
    }
}