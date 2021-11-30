using System;
using System.Collections.Generic;

namespace Nandaka.Model.Registers
{
    public interface IRegister
    {
        int Address { get; }
        RegisterType RegisterType { get; }
        int DataSize { get; }
        bool IsUpdated { get; }
        DateTime LastUpdateTime { get; }
        TimeSpan UpdateInterval { get; }
        
        event EventHandler<RegisterChangedEventArgs> OnRegisterChanged;
        
        byte[] ToBytes();
        void Update(IRegister updateRegister);
        void MarkAsUpdated();
        
        IRegister CreateCopyFromBytes(IReadOnlyList<byte> bytes);
        IRegister CreateCopy();
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