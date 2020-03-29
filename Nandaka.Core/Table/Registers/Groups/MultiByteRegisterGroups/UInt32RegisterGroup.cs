using System.Collections.Generic;
using Nandaka.Core.Helpers;

namespace Nandaka.Core.Table
{
    public sealed class UInt32RegisterGroup : MultiByteRegisterGroupBase<uint>
    {
        public override uint Value
        {
            get => LittleEndianConverter.ToUInt32(ToBytes());
            set => UpdateValue(LittleEndianConverter.GetBytes(value));
        }

        public UInt32RegisterGroup(IReadOnlyCollection<Register<byte>> registers) 
            : base(registers) { }
    }
}