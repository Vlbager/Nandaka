using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Nandaka.Core.Exceptions;

namespace Nandaka.Core.Registers
{
    internal sealed class LittleEndianRegisterValueProcessor<T> : IRegisterValueProcessor<T>
        where T: struct
    {
        private readonly int _valueSize;

        public LittleEndianRegisterValueProcessor(int valueSize)
        {
            _valueSize = valueSize;
        }

        public byte[] ToBytes(T value)
        {
            var result = new byte[_valueSize];
            unsafe
            {
                var ptr = new IntPtr(Unsafe.AsPointer(ref value));

                var offset = 0;
                
                for (int byteIndex = _valueSize - 1; byteIndex >= 0; byteIndex--)
                    result[byteIndex] = Marshal.ReadByte(ptr, offset++);
            }
            return result;
        }

        public T FromBytes(IReadOnlyList<byte> bytes)
        {
            if (bytes.Count != _valueSize)
                throw new NandakaBaseException("Wrong amount of bytes for convert to register value");

            T value = default;
            unsafe
            {
                var ptr = new IntPtr(Unsafe.AsPointer(ref value));

                var offset = 0;
                
                for (int byteIndex = bytes.Count - 1; byteIndex >= 0; byteIndex--)
                    Marshal.WriteByte(ptr, offset++, bytes[byteIndex]);
            }

            return value;
        }
    }
}