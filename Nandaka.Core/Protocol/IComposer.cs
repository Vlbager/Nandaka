using System.Collections.Generic;
using Nandaka.Core.Registers;

namespace Nandaka.Core.Protocol
{
    public interface IComposer<in TIn, out TOut>
    {
        TOut Compose(TIn message, out IReadOnlyList<IRegister> composedRegisters);
    }
}
