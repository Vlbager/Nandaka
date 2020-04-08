using System.Collections.Generic;
using Nandaka.Core.Table;

namespace Nandaka.Core.Session
{
    public interface IReceivedMessage : IMessage
    {
        IReadOnlyList<IRegister> Registers { get; }
        OperationType OperationType { get; }
    }
}