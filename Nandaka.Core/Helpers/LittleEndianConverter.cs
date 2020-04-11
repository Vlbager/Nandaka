using System;
using System.Collections.Generic;
using System.Linq;

namespace Nandaka.Core.Helpers
{
    public static class LittleEndianConverter
    {
        public static short ToInt16(IEnumerable<byte> littleEndianBytes)
        {
            return BitConverter.ToInt16(littleEndianBytes.RevertBytes(), 0);
        }

        public static ushort ToUInt16(IEnumerable<byte> littleEndianBytes)
        {
            return BitConverter.ToUInt16(littleEndianBytes.RevertBytes(), 0);
        }

        public static int ToInt32(IEnumerable<byte> littleEndianBytes)
        {
            return BitConverter.ToInt32(littleEndianBytes.RevertBytes(), 0);
        }

        public static uint ToUInt32(IEnumerable<byte> littleEndianBytes)
        {
            return BitConverter.ToUInt32(littleEndianBytes.RevertBytes(), 0);
        }

        public static long ToInt64(IEnumerable<byte> littleEndianBytes)
        {
            return BitConverter.ToInt64(littleEndianBytes.RevertBytes(), 0);
        }

        public static ulong ToUInt64(IEnumerable<byte> littleEndianBytes)
        {
            return BitConverter.ToUInt64(littleEndianBytes.RevertBytes(), 0);
        }

        public static byte[] GetBytes(ushort value)
        {
            return BitConverter.GetBytes(value).OrderByDescending(b => b).ToArray();
        }

        public static byte[] GetBytes(short value)
        {
            return BitConverter.GetBytes(value).OrderByDescending(b => b).ToArray();
        }

        public static byte[] GetBytes(uint value)
        {
            return BitConverter.GetBytes(value).OrderByDescending(b => b).ToArray();
        }

        public static byte[] GetBytes(int value)
        {
            return BitConverter.GetBytes(value).OrderByDescending(b => b).ToArray();
        }

        public static byte[] GetBytes(ulong value)
        {
            return BitConverter.GetBytes(value).OrderByDescending(b => b).ToArray();
        }

        public static byte[] GetBytes(long value)
        {
            return BitConverter.GetBytes(value).OrderByDescending(b => b).ToArray();
        }

        public static byte[] GetBytes(int value, int valueSizeInBytes)
        {
            var result = new byte[valueSizeInBytes];

            var index = 0;
            while (index < valueSizeInBytes)
                result[index] = (byte) (value >> (8 * index++));

            return result;
        }
    }
}
