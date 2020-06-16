using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Helpers;

namespace Nandaka.Core.Table
{
    public sealed class UInt16RegisterGroup : MultiByteRegisterGroupBase<ushort>
    {
        public UInt16RegisterGroup(IReadOnlyCollection<Register<byte>> registers)
            : base(registers) { }
        
        public static UInt16RegisterGroup CreateNew(int groupAddress, RegisterType type = RegisterType.Raw)
        {
            return new UInt16RegisterGroup(Enumerable.Range(groupAddress, sizeof(ushort))
                .Select(address => Register<byte>.CreateByte(address, type))
                .ToArray());
        }

        protected override byte[] ConvertValueToLittleEndianBytes(ushort value)
            => LittleEndianConverter.GetBytes(value);

        protected override ushort ConvertGroupToValue()
            => LittleEndianConverter.ToUInt16(ToBytes());
    }
}