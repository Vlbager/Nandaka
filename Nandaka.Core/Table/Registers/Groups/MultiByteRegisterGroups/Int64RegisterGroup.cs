using System.Collections.Generic;
using Nandaka.Core.Helpers;

namespace Nandaka.Core.Table
{
    public sealed class Int64RegisterGroup : MultiByteRegisterGroupBase<long>
    {
        public Int64RegisterGroup(IReadOnlyCollection<Register<byte>> registers) 
            : base(registers) { }

        protected override byte[] ConvertValueToLittleEndianBytes(long value)
            => LittleEndianConverter.GetBytes(value);

        protected override long ConvertGroupToValue()
            => LittleEndianConverter.ToInt64(ToBytes());
    }
}