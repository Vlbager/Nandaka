using System;
using Nandaka.Core.Helpers;

namespace Nandaka.Core.Table
{
    public sealed class UInt32RegisterGroup<TRegisterType> : RegisterGroupBase<uint, TRegisterType>
        where TRegisterType : struct
    {
        private const int GroupSizeInBytes = sizeof(uint);

        private UInt32RegisterGroup(RegisterTable<TRegisterType> table, int address, int count,
            Func<RegisterGroupBase<uint, TRegisterType>, uint> groupConversionFunc) 
            : base(table, address, count, groupConversionFunc) { }

        public static UInt32RegisterGroup<byte> Create(RegisterTable<byte> table, int address)
        {
            const int registersCount = GroupSizeInBytes;
            return new UInt32RegisterGroup<byte>(table, address, registersCount,
                group => LittleEndianConverter.ToUInt32(group.Table.GetRegisters(group.Address, group.Count)));
        }

        public static UInt32RegisterGroup<ushort> Create(RegisterTable<ushort> table, int address)
        {
            const int registersCount = GroupSizeInBytes / sizeof(ushort);
            return new UInt32RegisterGroup<ushort>(table, address, registersCount,
                group => LittleEndianConverter.ToUInt32(group.Table.GetRegisters(group.Address, group.Count)));
        }

        public static UInt32RegisterGroup<uint> Create(RegisterTable<uint> table, int address)
        {
            const int registersCount = GroupSizeInBytes / sizeof(uint);
            return new UInt32RegisterGroup<uint>(table, address, registersCount,
                group => group.Table.GetRegister(group.Address));
        }
    }
}
