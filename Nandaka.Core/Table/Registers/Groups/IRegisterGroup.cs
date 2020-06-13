using System;
using System.Collections.Generic;

namespace Nandaka.Core.Table
{
    public interface IRegisterGroup : IRegister
    {
        int Count { get; }
        int DataSize { get; }
        bool IsUpdated { get; set; }
        DateTime LastUpdateTime { get; }
        TimeSpan UpdateInterval { get; }
        void Update(IReadOnlyCollection<IRegister> registersToUpdate);
        void UpdateWithoutValues();
        IReadOnlyCollection<IRegister> GetRawRegisters();
    }
}
