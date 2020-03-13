using System;

namespace Nandaka.Core.Table
{
    public sealed class ByteRegisterGroup<TRegisterType> : RegisterGroupBase<byte, TRegisterType>
        where TRegisterType : struct
    {
        private ByteRegisterGroup(RegisterTable<TRegisterType> table, int address, int count,
            Func<RegisterGroupBase<byte, TRegisterType>, byte> groupConversionFunc)
            : base(table, address, count, groupConversionFunc) { }

        public static ByteRegisterGroup<byte> Create(RegisterTable<byte> table, int address)
        {
            return new ByteRegisterGroup<byte>(table, address, 1,
                group => group.Table.GetRegister(group.Address));
        }
    }
}
