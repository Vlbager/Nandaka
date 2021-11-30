using System.Collections.Generic;
using Nandaka.Core.Registers;
using Nandaka.Model.Registers;

namespace Nandaka.Core.Session
{
    public interface IRegisterMessage : IMessage
    {
        IReadOnlyList<IRegister> Registers { get; }
        OperationType OperationType { get; }
    }
}
