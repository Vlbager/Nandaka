using System;
using Nandaka.Core.Helpers;

namespace Nandaka.Core.Table
{
    class Int32RegisterGroup<TRegisterType> : RegisterGroupBase<int, TRegisterType>
        where TRegisterType : struct
    {
        public Int32RegisterGroup(RegisterTable<TRegisterType> table, int address, int count) : base(table, address, count)
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

        public override int Value => GetValueFromTable();

        private int GetValueFromTable()
        {
            switch (Count)
            {
                case 1:
                    TRegisterType register = Table.GetRegister(Address);
                    if (register is uint uintRegister)
                        return Convert.ToInt32(uintRegister);

                    break;

                case 2:
                    TRegisterType[] registers = Table.GetRegisters(Address, Count);
                    if (registers is ushort[] ushortRegisters)
                        return LittleEndianConverter.ToInt32(ushortRegisters);

                    break;

                case 4:
                    TRegisterType[] registers2 = Table.GetRegisters(Address, Count);
                    if (registers2 is byte[] byteRegisters)
                        return LittleEndianConverter.ToInt32(byteRegisters);

                    break;
            }

            // todo: create a custom exception.
            throw new ApplicationException($"Unsupported register type");
        }
    }
}
