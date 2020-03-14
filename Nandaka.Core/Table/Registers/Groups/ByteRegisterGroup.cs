using System;
using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Helpers;

namespace Nandaka.Core.Table
{
    public sealed class ByteRegisterGroup<TValue> : RegisterGroupBase<TValue>
        where TValue : struct
    {
        private readonly Func<ByteRegisterGroup<TValue>, TValue> _groupConversionFunc;

        public override TValue Value => _groupConversionFunc(this);
        public IReadOnlyCollection<Register<byte>> Registers { get; }

        private ByteRegisterGroup(IReadOnlyCollection<Register<byte>> registers, Func<ByteRegisterGroup<TValue>, TValue> groupConversionFunc)
            : base(registers.First().Address, registers.Count)
        {
            Registers = registers;
            _groupConversionFunc = groupConversionFunc;
        }

        #region Create Methods

        public static ByteRegisterGroup<byte> CreateByteGroup(Register<byte> register)
        {
            return new ByteRegisterGroup<byte>(new []{register},
                group => group.Value);
        }

        public static ByteRegisterGroup<sbyte> CreateSByteGroup(Register<byte> register)
        {
            return new ByteRegisterGroup<sbyte>(new []{register},
                group => Convert.ToSByte(group.Value));
        }

        public static ByteRegisterGroup<ushort> CreateUInt16Group(IReadOnlyCollection<Register<byte>> registers)
        {
            return new ByteRegisterGroup<ushort>(registers,
                group => LittleEndianConverter.ToUInt16(group.ToBytes()));
        }

        public static ByteRegisterGroup<short> CreateInt16Group(IReadOnlyCollection<Register<byte>> registers)
        {
            return new ByteRegisterGroup<short>(registers,
                group => LittleEndianConverter.ToInt16(group.ToBytes()));
        }

        public static ByteRegisterGroup<uint> CreateUInt32Group(IReadOnlyCollection<Register<byte>> registers)
        {
            return new ByteRegisterGroup<uint>(registers,
                group => LittleEndianConverter.ToUInt32(group.ToBytes()));
        }

        public static ByteRegisterGroup<int> CreateInt32Group(IReadOnlyCollection<Register<byte>> registers)
        {
            return new ByteRegisterGroup<int>(registers,
                group => LittleEndianConverter.ToInt32(group.ToBytes()));
        }

        public static ByteRegisterGroup<ulong> CreateUInt64Group(IReadOnlyCollection<Register<byte>> registers)
        {
            return new ByteRegisterGroup<ulong>(registers,
                group => LittleEndianConverter.ToUInt64(group.ToBytes()));
        }

        public static ByteRegisterGroup<long> CreateInt64Group(IReadOnlyCollection<Register<byte>> registers)
        {
            return new ByteRegisterGroup<long>(registers,
                group => LittleEndianConverter.ToInt64(group.ToBytes()));
        }

        #endregion

        public override IReadOnlyCollection<IRegister> GetRawRegisters() => Registers;

        public override byte[] ToBytes()
        {
            return Registers
                .OrderBy(register => register.Address)
                .Select(register => register.Value)
                .ToArray();
        }
    }
}
