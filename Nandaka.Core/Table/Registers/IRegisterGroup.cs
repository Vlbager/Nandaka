using System.Collections.Generic;

namespace Nandaka.Core.Table
{
    public interface IRegisterGroup<TRegisterType> : IRegister
        where TRegisterType : struct
    {
        int Count { get; }
        IReadOnlyCollection<RawRegister<TRegisterType>> GetRawRegisters();
    }
}
