using System.Collections.Generic;
using Nandaka.Core.Helpers;

namespace Nandaka.Core.Table
{
    public sealed class Int32RegisterGroup : MultiByteRegisterGroupBase<int>
    {
        public override int Value
        {
            get => LittleEndianConverter.ToInt32(ToBytes());
            set => UpdateValue(LittleEndianConverter.GetBytes(value));
        }

        public Int32RegisterGroup(IReadOnlyCollection<Register<byte>> registers) 
            : base(registers) { }
    }
}