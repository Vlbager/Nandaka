using System.Collections.Generic;

namespace Nandaka.Core.Registers
{
    public interface IRegisterByteValueConverter<T> where T: struct
    {
        byte[] ToBytes(T value);
        T FromBytes(IReadOnlyList<byte> fromBytes);
    }
}