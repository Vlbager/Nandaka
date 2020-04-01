﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Nandaka.Core.Table
{
    public abstract class MultiByteRegisterGroupBase<TValue> : RegisterGroupBase<TValue>
        where TValue : struct
    {
        private readonly object _syncRoot;
        private readonly IReadOnlyCollection<Register<byte>> _registers;

        public override int DataSize => _registers.Count;
        public override int Version { get; protected set; }
        
        public override TValue Value
        {
            get => GetValue();
            set => SetValue(value);
        }

        protected MultiByteRegisterGroupBase(IReadOnlyCollection<Register<byte>> registers)
            : base(registers.First().Address, registers.Count, registers.First().RegisterType)
        {
            AssertRegistersType(registers);
            _registers = registers;
            _syncRoot = new object();
        }

        public override IReadOnlyCollection<IRegister> GetRawRegisters() => _registers;

        public override byte[] ToBytes()
        {
            // todo: check for orderBy
            return _registers
                .OrderBy(register => register.Address)
                .Select(register => register.Value)
                .ToArray();
        }

        public override void Update(IReadOnlyCollection<IRegister> registersToUpdate)
        {
            if (!(registersToUpdate is IReadOnlyCollection<Register<byte>> byteRegistersToUpdate))
                // todo: create a custom exception
                throw new Exception("Wrong registers type");

            lock (_syncRoot)
            {
                foreach (Register<byte> storedRegister in _registers)
                {
                    Register<byte> registerToUpdate = byteRegistersToUpdate
                            .Single(register => register.Address == storedRegister.Address);

                    storedRegister.Value = registerToUpdate.Value;
                }

                Version++;
                IsUpdated = true;
            }
        }

        private void SetValue(TValue value)
        {
            byte[] littleEndianBytes = ConvertValueToLittleEndianBytes(value);

            lock (_syncRoot)
            {
                var index = 0;
                foreach (Register<byte> register in _registers)
                    register.Value = littleEndianBytes[index++];

                Version++;
                IsUpdated = false;
            }
        }

        private TValue GetValue()
        {
            lock (_syncRoot)
            {
                return ConvertGroupToValue();
            }
        }

        protected abstract byte[] ConvertValueToLittleEndianBytes(TValue value);

        protected abstract TValue ConvertGroupToValue();

        private static void AssertRegistersType(IReadOnlyCollection<IRegister> registers)
        {
            RegisterType type = registers.First().RegisterType;
            if (registers.Any(register => register.RegisterType != type))
                // todo: create a custom exception.
                throw new Exception("Registers in group must have same type");
        }
    }
}
