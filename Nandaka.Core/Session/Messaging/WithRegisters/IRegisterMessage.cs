using System.Collections.Generic;
using Nandaka.Core.Table;

namespace Nandaka.Core.Session
{
    public interface IRegisterMessage : IFrameworkMessage
    {
        ICollection<IRegisterGroup> RegisterGroups { get; }
        OperationType OperationType { get; }
    }
}
