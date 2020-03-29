using System.Collections.Generic;
using Nandaka.Core.Helpers;

namespace Nandaka.Core.Table
{
    public sealed class UInt64RegisterGroup : MultiByteRegisterGroupBase<ulong>
    {
        public UInt64RegisterGroup(IReadOnlyCollection<Register<byte>> registers) 
            : base(registers) { }

        protected override byte[] ConvertValueToLittleEndianBytes(ulong value)
            => LittleEndianConverter.GetBytes(value);

        protected override ulong ConvertGroupToValue()
            => LittleEndianConverter.ToUInt64(ToBytes());
    }
}