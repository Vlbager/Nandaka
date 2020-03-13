using System;
using Nandaka.Core.Helpers;

namespace Nandaka.Core.Table
{
    public sealed class UInt16RegisterGroup<TRegisterType> : RegisterGroupBase<ushort, TRegisterType>
        where TRegisterType : struct
    {
        private const int GroupSizeInBytes = sizeof(ushort);

        private UInt16RegisterGroup(RegisterTable<TRegisterType> table, int address, int count,
            Func<RegisterGroupBase<ushort, TRegisterType>, ushort> groupConversionFunc)
            : base(table, address, count, groupConversionFunc) { }

        public static UInt16RegisterGroup<byte> Create(RegisterTable<byte> table, int address)
        {
            const int registerCount = GroupSizeInBytes;
            return new UInt16RegisterGroup<byte>(table, address, registerCount,
                group => LittleEndianConverter.ToUInt16(group.Table.GetRegisters(address, registerCount)));
        }

        public static UInt16RegisterGroup<ushort> Create(RegisterTable<ushort> table, int address)
        {
            const int registersCount = GroupSizeInBytes / sizeof(ushort);
            return new UInt16RegisterGroup<ushort>(table, address, registersCount,
                group => group.Table.GetRegister(address));
        }
    }
}
