using System.Collections.Generic;
using Nandaka.Core.Helpers;

namespace Nandaka.Core.Table
{
    public sealed class UInt16RegisterGroup : MultiByteRegisterGroupBase<ushort>
    {
        public UInt16RegisterGroup(IReadOnlyCollection<Register<byte>> registers)
            : base(registers) { }

        protected override byte[] ConvertValueToLittleEndianBytes(ushort value)
            => LittleEndianConverter.GetBytes(value);

        protected override ushort ConvertGroupToValue()
            => LittleEndianConverter.ToUInt16(ToBytes());
    }
}