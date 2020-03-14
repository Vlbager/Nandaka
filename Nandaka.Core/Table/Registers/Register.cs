using System;
using Nandaka.Core.Helpers;

namespace Nandaka.Core.Table
{
    public class Register<TValue> : IValuedRegister<TValue>
        where TValue : struct
    {
        public int Address { get; }
        public TValue Value { get; set; }

        private Register(int address, TValue value)
        {
            Address = address;
            Value = value;
        }

        #region Create Methods

        public static Register<byte> CreateByte(int address, byte value = 0) => new Register<byte>(address, value);
        public static Register<sbyte> CreateSByte(int address, sbyte value = 0) => new Register<sbyte>(address, value);
        public static Register<ushort> CreateUInt16(int address, ushort value = 0) => new Register<ushort>(address, value);
        public static Register<short> CreateInt16(int address, short value = 0) => new Register<short>(address, value);
        public static Register<uint> CreateUInt32(int address, uint value = 0) => new Register<uint>(address, value);
        public static Register<int> CreateInt32(int address, int value = 0) => new Register<int>(address, value);
        public static Register<ulong> CreateUInt64(int address, ulong value = 0) => new Register<ulong>(address, value);
        public static Register<long> CreateInt64(int address, long value = 0) => new Register<long>(address, value); 

        #endregion

        public byte[] ToBytes()
        {
            return ConvertToBytes(this);
        }

        private static byte[] ConvertToBytes<T>(Register<T> register) where T : struct
        {
            switch (register)
            {
                case Register<byte> byteRegister:
                    return new[] { byteRegister.Value };

                case Register<ushort> uint16Register:
                    return LittleEndianConverter.GetBytes(uint16Register.Value);

                case Register<uint> uint32Register:
                    return LittleEndianConverter.GetBytes(uint32Register.Value);

                case Register<ulong> uint64Register:
                    return LittleEndianConverter.GetBytes(uint64Register.Value);

                default:
                    // todo: create custom exception;
                    throw new Exception();
            }
        }
    }
}
