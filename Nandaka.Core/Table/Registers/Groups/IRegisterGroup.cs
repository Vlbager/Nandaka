using System.Collections.Generic;

namespace Nandaka.Core.Table
{
    public interface IRegisterGroup : IRegister
    {
        int Count { get; }
        int DataSize { get; }
        bool IsUpdated { get; set; }
        void Update(IReadOnlyCollection<IRegister> registersToUpdate);
        IReadOnlyCollection<IRegister> GetRawRegisters();
    }
}
