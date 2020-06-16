using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Helpers;

namespace Nandaka.Core.Table
{
    public sealed class Int64RegisterGroup : MultiByteRegisterGroupBase<long>
    {
        public Int64RegisterGroup(IReadOnlyCollection<Register<byte>> registers) 
            : base(registers) { }
        
        public static Int64RegisterGroup CreateNew(int groupAddress, RegisterType type = RegisterType.Raw)
        {
            return new Int64RegisterGroup(Enumerable.Range(groupAddress, sizeof(long))
                .Select(address => Register<byte>.CreateByte(address, type))
                .ToArray());
        }

        protected override byte[] ConvertValueToLittleEndianBytes(long value)
            => LittleEndianConverter.GetBytes(value);

        protected override long ConvertGroupToValue()
            => LittleEndianConverter.ToInt64(ToBytes());
    }
}