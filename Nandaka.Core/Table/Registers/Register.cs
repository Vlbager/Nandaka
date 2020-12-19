using System;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Helpers;

namespace Nandaka.Core.Table
{
    public class Register<TValue> : IRwRegister<TValue>
        where TValue : struct
    {
        private TValue _value;
        public int Address { get; }
        public RegisterType RegisterType { get; }

        public TValue Value
        {
            get => _value;
            set
            {
                bool isValueChanged = Equals(_value, value);
                _value = value;
                if (isValueChanged)
                    OnRegisterChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler? OnRegisterChanged;

        private Register(int address, RegisterType registerType, TValue value)
        {
            Address = address;
            Value = value;
            RegisterType = registerType;
        }

        #region Create Methods

        public static Register<byte> CreateByte(int address, RegisterType registerType, byte value = 0)
            => new Register<byte>(address, registerType, value);

        public static Register<ushort> CreateUInt16(int address, RegisterType registerType, ushort value = 0)
            => new Register<ushort>(address, registerType, value);

        public static Register<uint> CreateUInt32(int address, RegisterType registerType, uint value = 0)
            => new Register<uint>(address, registerType, value);

        public static Register<ulong> CreateUInt64(int address, RegisterType registerType, ulong value = 0)
            => new Register<ulong>(address, registerType, value);

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
                    return new[] {byteRegister.Value};

                case Register<ushort> uint16Register:
                    return LittleEndianConverter.GetBytes(uint16Register.Value);

                case Register<uint> uint32Register:
                    return LittleEndianConverter.GetBytes(uint32Register.Value);

                case Register<ulong> uint64Register:
                    return LittleEndianConverter.GetBytes(uint64Register.Value);

                default:
                    throw new NandakaBaseException($"{typeof(T)} register cannot be converted to bytes");
            }
        }
    }
}