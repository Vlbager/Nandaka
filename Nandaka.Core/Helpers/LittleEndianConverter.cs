namespace Nandaka.Core.Helpers
{
    public static class LittleEndianConverter
    {
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
