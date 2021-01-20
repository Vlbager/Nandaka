﻿using System;
using System.Collections.Generic;

namespace Nandaka.Core.Registers
{
    public interface IRegister
    {
        int Address { get; }
        RegisterType RegisterType { get; }
        int DataSize { get; }
        bool IsUpdated { get; }
        DateTime LastUpdateTime { get; }
        TimeSpan UpdateInterval { get; }
        
        event EventHandler OnRegisterChanged;
        
        byte[] ToBytes();
        void Update(IRegister updateRegister);
        void UpdateWithoutValues();
        IRegister CreateFromBytes(IReadOnlyList<byte> bytes);
        Type GetValueType();
    }

    public interface IReadOnlyRegister<out T> : IRegister
        where T: struct
    {
        T Value { get; }
    }

    public interface IRegister<T> : IReadOnlyRegister<T>
        where T: struct
    {
        new T Value { get; set; }
    }
}
