﻿using System.Collections.Generic;

namespace Nandaka.Core.Table
{
    public interface IRegisterGroup : IRegister
    {
        int Count { get; }
        int DataSize { get; }
        IReadOnlyCollection<IRegister> GetRawRegisters();
    }
}