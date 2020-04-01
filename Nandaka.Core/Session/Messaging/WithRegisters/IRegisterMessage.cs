using System.Collections.Generic;
using Nandaka.Core.Table;

namespace Nandaka.Core.Session
{
    public interface IRegisterMessage : IFrameworkMessage
    {
        IReadOnlyCollection<IRegisterGroup> RegisterGroups { get; }
        OperationType OperationType { get; }
    }
}
