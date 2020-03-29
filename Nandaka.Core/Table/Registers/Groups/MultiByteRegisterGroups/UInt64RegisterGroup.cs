using System.Collections.Generic;
using Nandaka.Core.Helpers;

namespace Nandaka.Core.Table
{
    public sealed class UInt64RegisterGroup : MultiByteRegisterGroupBase<ulong>
    {
        public override ulong Value
        {
            get => LittleEndianConverter.ToUInt64(ToBytes());
            set => UpdateValue(LittleEndianConverter.GetBytes(value));
        }

        public UInt64RegisterGroup(IReadOnlyCollection<Register<byte>> registers) 
            : base(registers) { }
    }
}