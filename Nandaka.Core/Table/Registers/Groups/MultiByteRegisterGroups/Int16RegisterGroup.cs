using System.Collections.Generic;
using Nandaka.Core.Helpers;

namespace Nandaka.Core.Table
{
    public sealed class Int16RegisterGroup : MultiByteRegisterGroupBase<short>
    {
        public Int16RegisterGroup(IReadOnlyCollection<Register<byte>> registers) 
            : base(registers) { }

        protected override byte[] ConvertValueToLittleEndianBytes(short value)
            => LittleEndianConverter.GetBytes(value);

        protected override short ConvertGroupToValue()
            => LittleEndianConverter.ToInt16(ToBytes());
    }
}