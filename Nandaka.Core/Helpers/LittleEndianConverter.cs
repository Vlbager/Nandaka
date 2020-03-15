using System;

namespace Nandaka.Core.Helpers
{
    public static class LittleEndianConverter
    {
        public static short ToInt16(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public static ushort ToUInt16(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public static int ToInt32(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public static int ToInt32(ushort[] values)
        {
            throw new NotImplementedException();
        }

        public static uint ToUInt32(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public static uint ToUInt32(ushort[] values)
        {
            throw new NotImplementedException();
        }

        public static long ToInt64(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public static long ToInt64(ushort[] values)
        {
            throw new NotImplementedException();
        }

        public static long ToInt64(uint[] values)
        {
            throw new NotImplementedException();
        }

        public static ulong ToUInt64(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public static ulong ToUInt64(ushort[] values)
        {
            throw new NotImplementedException();
        }

        public static ulong ToUInt64(uint[] values)
        {
            throw new NotImplementedException();
        }

        public static byte[] GetBytes(ushort value)
        {
            throw new NotImplementedException();
        }

        public static byte[] GetBytes(uint value)
        {
            throw new NotImplementedException();
        }

        public static byte[] GetBytes(ulong value)
        {
            throw new NotImplementedException();
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
