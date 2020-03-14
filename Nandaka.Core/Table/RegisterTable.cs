﻿using System;
using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Helpers;

namespace Nandaka.Core.Table
{
    public class RegisterTable
    {
        private readonly IRegister[] _registers;

        private RegisterTable(int tableSize)
        {
            _registers = new IRegister[tableSize];
        }

        public static RegisterTable Create(int tableSize, IEnumerable<IRegister> registers)
        {
            var table = new RegisterTable(tableSize);
            table.SetRegisters(registers);
            return table;
        }

        public IRegister this[int address]
        {
            get => _registers[address];
            set => SetRegister(value);
        }

        public Register<byte> this[Register<byte> register]
        {
            get => GetRegisterChecked(register);
            set => SetRegisterChecked(value);
        }

        public Register<ushort> this[Register<ushort> register]
        {
            get => GetRegisterChecked(register);
            set => SetRegisterChecked(value);
        }

        public Register<uint> this[Register<uint> register]
        {
            get => GetRegisterChecked(register);
            set => SetRegisterChecked(value);
        }

        public Register<ulong> this[Register<ulong> register]
        {
            get => GetRegisterChecked(register);
            set => SetRegisterChecked(value);
        }

        public IRegister[] GetRawRegisters(int firstRegisterAddress, int count)
        {
            return _registers
                .Skip(firstRegisterAddress)
                .Take(count)
                .IsNullAssert()
                .ToArray();
        }

        public void SetRegisters(IEnumerable<IRegister> registers)
        {
            foreach (IRegister register in registers)
            {
                SetRegister(register);
            }
        }

        private void SetRegister(IRegister register)
        {
            switch (register)
            {
                case Register<byte> byteRegister:
                    this[byteRegister] = byteRegister;
                    break;

                case Register<ushort> uint16Register:
                    this[uint16Register] = uint16Register;
                    break;

                case Register<uint> uint32Register:
                    this[uint32Register] = uint32Register;
                    break;

                case Register<ulong> uint64Register:
                    this[uint64Register] = uint64Register;
                    break;

                default:
                    // todo: create custom exception;
                    throw new Exception();
            }
        }

        private Register<T> GetRegisterChecked<T>(Register<T> register) where T : struct
        {
            if (!(_registers[register.Address] is Register<T> tableRegister))
                // todo: create a custom exception
                throw new InvalidCastException();

            return tableRegister;
        }

        private void SetRegisterChecked<T>(Register<T> register) where T : struct
        {
            if (!(_registers[register.Address] is Register<T>))
                // todo: create a custom exception
                throw new InvalidCastException();

            _registers[register.Address] = register;
        }
    }
}
