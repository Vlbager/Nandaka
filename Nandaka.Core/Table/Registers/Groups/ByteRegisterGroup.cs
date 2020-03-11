using System;

namespace Nandaka.Core.Table
{
    internal class ByteRegisterGroup<TRegisterType> : RegisterGroupBase<byte, TRegisterType>
        where TRegisterType : struct
    {
        public ByteRegisterGroup(RegisterTable<TRegisterType> table, int address, int count) : base(table, address, count)
        {
        }

        public override byte[] GetBytes()
        {
            return new[] {Value};
        }

        public override byte Value => GetValueFromTable();

        private byte GetValueFromTable()
        {
            TRegisterType register = Table.GetRegister(Address);
            if (register is byte byteRegister)
                return byteRegister;

            // todo: create a custom exception.
            throw new ApplicationException($"Unsupported register type");
        }
    }
}
