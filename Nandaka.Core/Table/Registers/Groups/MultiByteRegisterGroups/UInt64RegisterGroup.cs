using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Helpers;

namespace Nandaka.Core.Table
{
    public sealed class UInt64RegisterGroup : MultiByteRegisterGroupBase<ulong>
    {
        public UInt64RegisterGroup(IReadOnlyCollection<Register<byte>> registers) 
            : base(registers) { }
        
        public static UInt64RegisterGroup CreateNew(int groupAddress, RegisterType type = RegisterType.Raw)
        {
            return new UInt64RegisterGroup(Enumerable.Range(groupAddress, sizeof(ulong))
                .Select(address => Register<byte>.CreateByte(address, type))
                .ToArray());
        }

        protected override byte[] ConvertValueToLittleEndianBytes(ulong value)
            => LittleEndianConverter.GetBytes(value);

        protected override ulong ConvertGroupToValue()
            => LittleEndianConverter.ToUInt64(ToBytes());
    }
}