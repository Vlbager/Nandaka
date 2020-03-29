using System.Collections.Generic;
using Nandaka.Core.Helpers;

namespace Nandaka.Core.Table
{
    public sealed class UInt16RegisterGroup : MultiByteRegisterGroupBase<ushort>
    {
        public override ushort Value
        {
            get => LittleEndianConverter.ToUInt16(ToBytes());
            set => UpdateValue(LittleEndianConverter.GetBytes(value));
        }

        public UInt16RegisterGroup(IReadOnlyCollection<Register<byte>> registers) 
            : base(registers) { }
    }
}