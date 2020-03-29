using System;
using System.Collections.Generic;
using System.Linq;

namespace Nandaka.Core.Table
{
    public abstract class MultiByteRegisterGroupBase<TValue> : RegisterGroupBase<TValue>
        where TValue : struct
    {
        public override int DataSize => Registers.Count;
        public IReadOnlyCollection<Register<byte>> Registers { get; }

        protected MultiByteRegisterGroupBase(IReadOnlyCollection<Register<byte>> registers)
            : base(registers.First().Address, registers.Count, registers.First().RegisterType)
        {
            AssertRegistersType(registers);
            Registers = registers;
        }

        public override IReadOnlyCollection<IRegister> GetRawRegisters() => Registers;

        public override byte[] ToBytes()
        {
            return Registers
                .OrderBy(register => register.Address)
                .Select(register => register.Value)
                .ToArray();
        }

        protected override void UpdateRegister(IRegister registerToUpdate)
        {
            if (!(registerToUpdate is Register<byte> byteRegister))
                // todo: create a custom exception
                throw new Exception("Wrong register type");

            Register<byte> storedRegister = Registers.Single(register => register.Address == byteRegister.Address);

            storedRegister.Value = byteRegister.Value;
        }

        protected void UpdateValue(byte[] littleEndianBytes)
        {
            var index = 0;
            Register<byte>[] registers = Enumerable.Range(Address, Count)
                .Select(address => Register<byte>.CreateByte(address, RegisterType.Raw, littleEndianBytes[index++]))
                .ToArray();

            Update(registers);
        }

        private static void AssertRegistersType(IReadOnlyCollection<IRegister> registers)
        {
            RegisterType type = registers.First().RegisterType;
            if (registers.Any(register => register.RegisterType != type))
                // todo: create a custom exception.
                throw new Exception("Registers in group must have same type");
        }
    }
}
