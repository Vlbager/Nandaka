using System;
using Nandaka.Core.Helpers;

namespace Nandaka.Core.Table
{
    public sealed class Int16RegisterGroup<TRegisterType> : RegisterGroupBase<short, TRegisterType>
        where TRegisterType : struct
    {
        private const int GroupSizeInBytes = sizeof(short); 

        private Int16RegisterGroup(RegisterTable<TRegisterType> table, int address, int count,
            Func<RegisterGroupBase<short, TRegisterType>, short> groupConversionFunc) 
            : base(table, address, count, groupConversionFunc) { }

        public static Int16RegisterGroup<byte> Create(RegisterTable<byte> table, int address)
        {
            const int registersCount = GroupSizeInBytes;
            return new Int16RegisterGroup<byte>(table, address, registersCount,
                group => LittleEndianConverter.ToInt16(group.Table.GetRegisters(group.Address, group.Count)));
        }

        public static Int16RegisterGroup<ushort> Create(RegisterTable<ushort> table, int address)
        {
            const int registersCount = GroupSizeInBytes / sizeof(short);
            return new Int16RegisterGroup<ushort>(table, address, registersCount,
                group => Convert.ToInt16(group.Table.GetRegister(group.Address)));
        }
    }
}
