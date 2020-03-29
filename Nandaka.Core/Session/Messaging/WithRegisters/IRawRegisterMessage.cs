using System.Collections.Generic;
using Nandaka.Core.Table;

namespace Nandaka.Core.Session
{
    public interface IRawRegisterMessage : IFrameworkMessage
    {
        IReadOnlyCollection<IRegister> Registers { get; }
        OperationType OperationType { get; }
    }
}