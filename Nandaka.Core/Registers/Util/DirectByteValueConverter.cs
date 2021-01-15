using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Nandaka.Core.Exceptions;

namespace Nandaka.Core.Registers
{
    internal sealed class DirectByteValueConverter<T> : IRegisterByteValueConverter<T>
        where T: struct
    {
        private static readonly int ValueSize = Marshal.SizeOf<T>();

        private static readonly Lazy<DirectByteValueConverter<T>> LazyInstance =
            new Lazy<DirectByteValueConverter<T>>(() => new DirectByteValueConverter<T>());
        
        private DirectByteValueConverter()
        { }

        public static DirectByteValueConverter<T> Instance => LazyInstance.Value;

        public byte[] ToBytes(T value)
        {
            var result = new byte[ValueSize];
            unsafe
            {
                var ptr = new IntPtr(Unsafe.AsPointer(ref value));

                var offset = 0;
                
                for (int byteIndex = 0; byteIndex < ValueSize; byteIndex++)
                    result[byteIndex] = Marshal.ReadByte(ptr, offset++);
            }
            return result;
        }

        public T FromBytes(IReadOnlyList<byte> bytes)
        {
            if (bytes.Count != ValueSize)
                throw new NandakaBaseException("Wrong amount of bytes for convert to register value");

            T value = default;
            unsafe
            {
                var ptr = new IntPtr(Unsafe.AsPointer(ref value));

                var offset = 0;
                
                for (int byteIndex = 0; byteIndex < ValueSize; byteIndex++)
                    Marshal.WriteByte(ptr, offset++, bytes[byteIndex]);
            }

            return value;
        }
    }
}