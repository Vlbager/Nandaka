﻿using System.Collections.Generic;
using System.Linq;

namespace Nandaka.Core.Table
{
    public abstract class RegisterGroupBase<TValue> : IRegisterGroup, IValuedRegister<TValue>
        where TValue : struct
    {
        public int Address { get; }
        public int Count { get; }
        public bool IsUpdated { get; set; }
        public RegisterType RegisterType { get; }
        public abstract int DataSize { get; }
        public abstract TValue Value { get; set; }

        protected RegisterGroupBase(int address, int count, RegisterType registerType)
        {
            Address = address;
            Count = count;
            RegisterType = registerType;
        }
        
        public virtual byte[] ToBytes()
        {
            // todo: check little endianness.
            return GetRawRegisters()
                .SelectMany(register => register.ToBytes())
                .ToArray();
        }

        public abstract void Update(IReadOnlyCollection<IRegister> registersToUpdate);

        public abstract IReadOnlyCollection<IRegister> GetRawRegisters();
    }
}
