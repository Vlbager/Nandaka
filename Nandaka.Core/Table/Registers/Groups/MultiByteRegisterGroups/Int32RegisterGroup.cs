using System.Collections.Generic;
using Nandaka.Core.Helpers;

namespace Nandaka.Core.Table
{
    public sealed class Int32RegisterGroup : MultiByteRegisterGroupBase<int>
    {
        public Int32RegisterGroup(IReadOnlyCollection<Register<byte>> registers) 
            : base(registers) { }

        protected override byte[] ConvertValueToLittleEndianBytes(int value)
            => LittleEndianConverter.GetBytes(value);

        protected override int ConvertGroupToValue()
            => LittleEndianConverter.ToInt32(ToBytes());
    }
}