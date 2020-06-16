using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Helpers;

namespace Nandaka.Core.Table
{
    public sealed class Int32RegisterGroup : MultiByteRegisterGroupBase<int>
    {
        public Int32RegisterGroup(IReadOnlyCollection<Register<byte>> registers) 
            : base(registers) { }
        
        public static Int32RegisterGroup CreateNew(int groupAddress, RegisterType type = RegisterType.Raw)
        {
            return new Int32RegisterGroup(Enumerable.Range(groupAddress, sizeof(int))
                .Select(address => Register<byte>.CreateByte(address, type))
                .ToArray());
        }

        protected override byte[] ConvertValueToLittleEndianBytes(int value)
            => LittleEndianConverter.GetBytes(value);

        protected override int ConvertGroupToValue()
            => LittleEndianConverter.ToInt32(ToBytes());
    }
}