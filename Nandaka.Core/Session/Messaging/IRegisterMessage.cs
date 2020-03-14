using System.Collections.Generic;
using Nandaka.Core.Table;

namespace Nandaka.Core.Session
{
    public interface IRegisterMessage : IFrameworkMessage
    {
        void AddRegister(IRegisterGroup registerGroup);
        void RemoveRegister(IRegisterGroup registerGroup);
        IReadOnlyCollection<IRegisterGroup> Registers { get; }
        OperationType OperationType { get; }
    }
}
