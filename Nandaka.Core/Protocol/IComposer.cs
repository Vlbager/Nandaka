using System.Collections.Generic;
using Nandaka.Core.Table;

namespace Nandaka.Core.Protocol
{
    public interface IComposer<in TIn, out TOut>
    {
        TOut Compose(TIn message, out IReadOnlyCollection<IRegisterGroup> composedGroups);
    }
}
