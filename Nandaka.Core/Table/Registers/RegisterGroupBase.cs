using System;
using System.Collections.Generic;
using System.Linq;

namespace Nandaka.Core.Table
{
    public abstract class RegisterGroupBase<TGroupType, TRegisterType> : IRegisterGroup <TRegisterType>
        where TGroupType : struct
        where TRegisterType : struct
    {
        private readonly Func<RegisterGroupBase<TGroupType, TRegisterType>, TGroupType> _groupConversionFunc;

        public int Address { get; }
        public int Count { get; }
        public RegisterTable<TRegisterType> Table { get; }
        public TGroupType Value => _groupConversionFunc(this);

        protected RegisterGroupBase(RegisterTable<TRegisterType> table, 
            int address, int count, Func<RegisterGroupBase<TGroupType, TRegisterType>, TGroupType> groupConversionFunc)
        {
            Table = table;
            Address = address;
            Count = count;
            _groupConversionFunc = groupConversionFunc;
        }

        public IReadOnlyCollection<RawRegister<TRegisterType>> GetRawRegisters()
        {
            int firstRegisterAddress = Address;
            return Table.GetRegisters(firstRegisterAddress, Count)
                .Select(registerValue => new RawRegister<TRegisterType>(firstRegisterAddress++, registerValue))
                .ToArray();
        }
    }
}
