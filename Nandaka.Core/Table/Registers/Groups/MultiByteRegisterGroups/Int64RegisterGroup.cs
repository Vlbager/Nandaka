using System.Collections.Generic;
using Nandaka.Core.Helpers;

namespace Nandaka.Core.Table
{
    public sealed class Int64RegisterGroup : MultiByteRegisterGroupBase<long>
    {
        public override long Value
        {
            get => LittleEndianConverter.ToInt64(ToBytes());
            set => UpdateValue(LittleEndianConverter.GetBytes(value));
        }

        public Int64RegisterGroup(IReadOnlyCollection<Register<byte>> registers) 
            : base(registers) { }
    }
}