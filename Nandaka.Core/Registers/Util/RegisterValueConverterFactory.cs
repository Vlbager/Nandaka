using System;

namespace Nandaka.Core.Registers
{
    public static class RegisterValueConverterFactory
    {
        public static IRegisterByteValueConverter<T> GetLittleEndianConverter<T>()
            where T: struct
        {
            if (BitConverter.IsLittleEndian)
                return DirectByteValueConverter<T>.Instance;
            
            return ReverseByteValueConverter<T>.Instance;
        }
        
        public static IRegisterByteValueConverter<T> GetBigEndianConverter<T>()
            where T: struct
        {
            if (BitConverter.IsLittleEndian)
                return ReverseByteValueConverter<T>.Instance;
            
            return DirectByteValueConverter<T>.Instance;
        }
    }
}