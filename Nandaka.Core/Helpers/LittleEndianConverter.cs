using System;
using System.Collections.Generic;
using System.Linq;

namespace Nandaka.Core.Helpers
{
    public static class LittleEndianConverter
    {
        public static short ToInt16(IEnumerable<byte> littleEndianBytes)
        {
            return Convert.ToInt16(littleEndianBytes.OrderByDescending(b => b));
        }

        public static ushort ToUInt16(IEnumerable<byte> littleEndianBytes)
        {
            return Convert.ToUInt16(littleEndianBytes.OrderByDescending(b => b));
        }

        public static int ToInt32(IEnumerable<byte> littleEndianBytes)
        {
            return Convert.ToInt32(littleEndianBytes.OrderByDescending(b => b));
        }

        public static uint ToUInt32(IEnumerable<byte> littleEndianBytes)
        {
            return Convert.ToUInt32(littleEndianBytes.OrderByDescending(b => b));
        }

        public static long ToInt64(IEnumerable<byte> littleEndianBytes)
        {
            return Convert.ToInt64(littleEndianBytes.OrderByDescending(b => b));
        }

        public static ulong ToUInt64(IEnumerable<byte> littleEndianBytes)
        {
            return Convert.ToUInt64(littleEndianBytes.OrderByDescending(b => b));
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
