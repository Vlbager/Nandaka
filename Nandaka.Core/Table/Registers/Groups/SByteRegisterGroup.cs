using System;

namespace Nandaka.Core.Table
{
    internal class SByteRegisterGroup<TRegisterType> : RegisterGroupBase<sbyte, TRegisterType>
        where TRegisterType : struct
    {
        public SByteRegisterGroup(RegisterTable<TRegisterType> table, int address, int count) : base(table, address, count)
        {
        }

        public override byte[] GetBytes()
        {
            return new[] {GetByteFromTable()};
        }

        public override sbyte Value => GetValueFromTable();

        private byte GetByteFromTable()
        {
            TRegisterType register = Table.GetRegister(Address);
            if (!(register is byte byteRegister))
                // todo: create a custom exception.
                throw new ApplicationException($"Unsupported register type");

            return byteRegister;
        }

        private sbyte GetValueFromTable()
        {
            byte byteRegister = GetByteFromTable();
            return Convert.ToSByte(byteRegister);
        }
    }
}
