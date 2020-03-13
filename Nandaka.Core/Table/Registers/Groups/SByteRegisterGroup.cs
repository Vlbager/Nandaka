using System;

namespace Nandaka.Core.Table
{
    public sealed class SByteRegisterGroup<TRegisterType> : RegisterGroupBase<sbyte, TRegisterType>
        where TRegisterType : struct
    {
        private SByteRegisterGroup(RegisterTable<TRegisterType> table, int address, int count,
            Func<RegisterGroupBase<sbyte, TRegisterType>, sbyte> groupConversionFunc)
            : base(table, address, count, groupConversionFunc) { }

        public static SByteRegisterGroup<byte> Create(RegisterTable<byte> table, int address)
        {
            return new SByteRegisterGroup<byte>(table, address, 1,
                group => Convert.ToSByte(group.Table.GetRegister(group.Address)));
        }
    }
}
