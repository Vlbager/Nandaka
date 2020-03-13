using System;
using Nandaka.Core.Helpers;

namespace Nandaka.Core.Table
{
    public sealed class Int32RegisterGroup<TRegisterType> : RegisterGroupBase<int, TRegisterType>
        where TRegisterType : struct
    {
        private const int GroupSizeInBytes = sizeof(int);

        private Int32RegisterGroup(RegisterTable<TRegisterType> table, int address, int count,
            Func<RegisterGroupBase<int, TRegisterType>, int> groupConversionFunc)
            : base(table, address, count, groupConversionFunc) { }

        public static Int32RegisterGroup<byte> Create(RegisterTable<byte> table, int address)
        {
            const int registersCount = GroupSizeInBytes;
            return new Int32RegisterGroup<byte>(table, address, registersCount,
                group => LittleEndianConverter.ToInt32(group.Table.GetRegisters(group.Address, group.Count)));
        }

        public static Int32RegisterGroup<ushort> Create(RegisterTable<ushort> table, int address)
        {
            const int registersCount = GroupSizeInBytes / sizeof(ushort);
            return new Int32RegisterGroup<ushort>(table, address, registersCount,
                group => LittleEndianConverter.ToInt32(group.Table.GetRegisters(group.Address, group.Count)));
        }

        public static Int32RegisterGroup<uint> Create(RegisterTable<uint> table, int address)
        {
            const int registerCount = GroupSizeInBytes / sizeof(uint);
            return new Int32RegisterGroup<uint>(table, address, registerCount,
                group => Convert.ToInt32(group.Table.GetRegister(address)));
        }
    }
}
