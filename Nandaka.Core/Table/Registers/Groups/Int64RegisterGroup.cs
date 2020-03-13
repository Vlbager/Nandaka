using System;
using Nandaka.Core.Helpers;

namespace Nandaka.Core.Table
{
    public sealed class Int64RegisterGroup<TRegisterType> : RegisterGroupBase<long, TRegisterType>
        where TRegisterType : struct
    {
        private const int GroupSizeInBytes = sizeof(long);

        private Int64RegisterGroup(RegisterTable<TRegisterType> table, int address, int count,
            Func<RegisterGroupBase<long, TRegisterType>, long> groupConversionFunc) 
            : base(table, address, count, groupConversionFunc) { }

        public static Int64RegisterGroup<byte> Create(RegisterTable<byte> table, int address)
        {
            const int registersCount = GroupSizeInBytes;
            return new Int64RegisterGroup<byte>(table, address, registersCount,
                group => LittleEndianConverter.ToInt64(group.Table.GetRegisters(group.Address, group.Count)));
        }

        public static Int64RegisterGroup<ushort> Create(RegisterTable<ushort> table, int address)
        {
            const int registerCount = GroupSizeInBytes / sizeof(ushort);
            return new Int64RegisterGroup<ushort>(table, address, registerCount,
                group => LittleEndianConverter.ToInt64(group.Table.GetRegisters(group.Address, group.Count)));
        }

        public static Int64RegisterGroup<uint> Create(RegisterTable<uint> table, int address)
        {
            const int registerCount = GroupSizeInBytes / sizeof(uint);
            return new Int64RegisterGroup<uint>(table, address, registerCount,
                group => LittleEndianConverter.ToInt64(group.Table.GetRegisters(group.Address, group.Count)));
        }

        public static Int64RegisterGroup<long> Create(RegisterTable<long> table, int address)
        {
            const int registerCount = GroupSizeInBytes / sizeof(long);
            return new Int64RegisterGroup<long>(table, address, registerCount,
                group => Convert.ToInt64(group.Table.GetRegister(address)));
        }
    }
}
