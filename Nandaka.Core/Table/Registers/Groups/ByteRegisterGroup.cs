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

        public override int DataSize => Registers.Count;
        public override TValue Value => _groupConversionFunc(this);
        public IReadOnlyCollection<Register<byte>> Registers { get; }

        private ByteRegisterGroup(IReadOnlyCollection<Register<byte>> registers, Func<ByteRegisterGroup<TValue>, TValue> groupConversionFunc)
            : base(registers.First().Address, registers.Count, registers.First().RegisterType)
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
            => Create<ushort>(registers, group => LittleEndianConverter.ToUInt16(group.ToBytes()));

        public static ByteRegisterGroup<short> CreateInt16Group(IReadOnlyCollection<Register<byte>> registers)
            => Create<short>(registers, group => LittleEndianConverter.ToInt16(group.ToBytes()));
        
        public static ByteRegisterGroup<uint> CreateUInt32Group(IReadOnlyCollection<Register<byte>> registers)
            => Create<uint>(registers, group => LittleEndianConverter.ToUInt32(group.ToBytes()));

        public static ByteRegisterGroup<int> CreateInt32Group(IReadOnlyCollection<Register<byte>> registers)
            => Create<int>(registers, group => LittleEndianConverter.ToInt32(group.ToBytes()));

        public static ByteRegisterGroup<ulong> CreateUInt64Group(IReadOnlyCollection<Register<byte>> registers)
            => Create<ulong>(registers, group => LittleEndianConverter.ToUInt64(group.ToBytes()));

        public static ByteRegisterGroup<long> CreateInt64Group(IReadOnlyCollection<Register<byte>> registers)
            => Create<long>(registers, group => LittleEndianConverter.ToInt64(group.ToBytes()));
        
        private static ByteRegisterGroup<T> Create<T>(IReadOnlyCollection<Register<byte>> registers,
            Func<ByteRegisterGroup<T>, T> groupConversionFunc) where T : struct
        {
            AssertRegistersType(registers);
            return new ByteRegisterGroup<T>(registers, groupConversionFunc);
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

        private static void AssertRegistersType(IReadOnlyCollection<IRegister> registers)
        {
            RegisterType type = registers.First().RegisterType;
            if (registers.Any(register => register.RegisterType != type))
                // todo: create a custom exception.
                throw new Exception("Registers in group must have same type");
        }
    }
}
