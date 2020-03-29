using System.Collections.Generic;
using Nandaka.Core.Helpers;

namespace Nandaka.Core.Table
{
    public sealed class UInt32RegisterGroup : MultiByteRegisterGroupBase<uint>
    {
        public UInt32RegisterGroup(IReadOnlyCollection<Register<byte>> registers) 
            : base(registers) { }

        protected override byte[] ConvertValueToLittleEndianBytes(uint value)
            => LittleEndianConverter.GetBytes(value);

        protected override uint ConvertGroupToValue()
            => LittleEndianConverter.ToUInt32(ToBytes());
    }
}