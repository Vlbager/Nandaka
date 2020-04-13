using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Helpers;

namespace Nandaka.Core.Table
{
    public sealed class Int16RegisterGroup : MultiByteRegisterGroupBase<short>
    {
        public Int16RegisterGroup(IReadOnlyCollection<Register<byte>> registers) 
            : base(registers) { }
        
        public static Int16RegisterGroup CreateNew(int groupAddress, RegisterType type)
        {
            return new Int16RegisterGroup(Enumerable.Range(groupAddress, sizeof(short))
                .Select(address => Register<byte>.CreateByte(address, type))
                .ToArray());
        }

        protected override byte[] ConvertValueToLittleEndianBytes(short value)
            => LittleEndianConverter.GetBytes(value);

        protected override short ConvertGroupToValue()
            => LittleEndianConverter.ToInt16(ToBytes());
    }
}