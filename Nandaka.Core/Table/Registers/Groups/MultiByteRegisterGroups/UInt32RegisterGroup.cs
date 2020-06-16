using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Helpers;

namespace Nandaka.Core.Table
{
    public sealed class UInt32RegisterGroup : MultiByteRegisterGroupBase<uint>
    {
        public UInt32RegisterGroup(IReadOnlyCollection<Register<byte>> registers) 
            : base(registers) { }
        
        public static UInt32RegisterGroup CreateNew(int groupAddress, RegisterType type = RegisterType.Raw)
        {
            return new UInt32RegisterGroup(Enumerable.Range(groupAddress, sizeof(uint))
                .Select(address => Register<byte>.CreateByte(address, type))
                .ToArray());
        }

        protected override byte[] ConvertValueToLittleEndianBytes(uint value)
            => LittleEndianConverter.GetBytes(value);

        protected override uint ConvertGroupToValue()
            => LittleEndianConverter.ToUInt32(ToBytes());
    }
}