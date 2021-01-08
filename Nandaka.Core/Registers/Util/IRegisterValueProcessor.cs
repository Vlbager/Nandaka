using System.Collections.Generic;

namespace Nandaka.Core.Registers
{
    public interface IRegisterValueProcessor<T> where T: struct
    {
        byte[] ToBytes(T value);
        T FromBytes(IReadOnlyList<byte> fromBytes);
    }
}