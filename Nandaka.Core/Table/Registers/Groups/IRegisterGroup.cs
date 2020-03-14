using System.Collections.Generic;

namespace Nandaka.Core.Table
{
    public interface IRegisterGroup : IRegister
    {
        int Count { get; }
        IReadOnlyCollection<IRegister> GetRawRegisters();
    }
}
