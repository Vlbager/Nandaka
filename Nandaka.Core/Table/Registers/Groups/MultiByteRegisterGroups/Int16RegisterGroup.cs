using System.Collections.Generic;
using Nandaka.Core.Helpers;

namespace Nandaka.Core.Table
{
    public sealed class Int16RegisterGroup : MultiByteRegisterGroupBase<short>
    {
        public override short Value
        {
            get => LittleEndianConverter.ToInt16(ToBytes());
            set => UpdateValue(LittleEndianConverter.GetBytes(value));
        }

        public Int16RegisterGroup(IReadOnlyCollection<Register<byte>> registers) 
            : base(registers) { }
    }
}