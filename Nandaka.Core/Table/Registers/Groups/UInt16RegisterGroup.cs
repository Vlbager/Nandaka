using System;
using Nandaka.Core.Helpers;

namespace Nandaka.Core.Table
{
    internal class UInt16RegisterGroup<TRegisterType> : RegisterGroupBase<ushort, TRegisterType>
        where TRegisterType : struct
    {
        public UInt16RegisterGroup(RegisterTable<TRegisterType> table, int address, int count) : base(table, address, count)
        {
        }

        public override byte[] GetBytes()
        {
            var registers = Table.GetRegisters(Address, Count) as byte[];
            if (registers == null)
                // todo: create a custom exception.
                throw new ApplicationException($"Unsupported register type");

            return registers;
        }

        public override ushort Value => GetValueFromTable();

        private ushort GetValueFromTable()
        {
            switch (Count)
            {
                case 1:
                    TRegisterType register = Table.GetRegister(Address);
                    if (register is ushort ushortRegister)
                        return ushortRegister;

                    break;

                case 2:
                    TRegisterType[] registers = Table.GetRegisters(Address, Count);
                    if (registers is byte[] byteRegisters)
                        return LittleEndianConverter.ToUInt16(byteRegisters);

                    break;
            }

            // todo: create a custom exception.
            throw new ApplicationException($"Unsupported register type");
        }
    }
}
