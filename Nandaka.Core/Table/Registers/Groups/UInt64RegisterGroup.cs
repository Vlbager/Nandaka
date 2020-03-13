using System;
using Nandaka.Core.Helpers;

namespace Nandaka.Core.Table
{
    public sealed class UInt64RegisterGroup<TRegisterType> : RegisterGroupBase<ulong, TRegisterType>
        where TRegisterType : struct
    {
        private const int GroupSizeInBytes = sizeof(ulong);

        private UInt64RegisterGroup(RegisterTable<TRegisterType> table, int address, int count,
            Func<RegisterGroupBase<ulong, TRegisterType>, ulong> groupConversionFunc) 
            : base(table, address, count, groupConversionFunc) { }

        public static UInt64RegisterGroup<byte> Create(RegisterTable<byte> table, int address)
        {
            const int registersCount = GroupSizeInBytes;
            return new UInt64RegisterGroup<byte>(table, address, registersCount,
                group => LittleEndianConverter.ToUInt64(group.Table.GetRegisters(group.Address, group.Count)));
        }

        public static UInt64RegisterGroup<ushort> Create(RegisterTable<ushort> table, int address)
        {
            const int registersCount = GroupSizeInBytes / sizeof(ushort);
            return new UInt64RegisterGroup<ushort>(table, address, registersCount,
                group => LittleEndianConverter.ToUInt64(group.Table.GetRegisters(group.Address, group.Count)));
        }

        public static UInt64RegisterGroup<uint> Create(RegisterTable<uint> table, int address)
        {
            const int registersCount = GroupSizeInBytes / sizeof(uint);
            return new UInt64RegisterGroup<uint>(table, address, registersCount,
                group => LittleEndianConverter.ToUInt64(group.Table.GetRegisters(group.Address, group.Count)));
        }

        public static UInt64RegisterGroup<ulong> Create(RegisterTable<ulong> table, int address)
        {
            const int registersCount = GroupSizeInBytes / sizeof(ulong);
            return new UInt64RegisterGroup<ulong>(table, address, registersCount,
                group => group.Table.GetRegister(group.Address));
        }
    }
}
