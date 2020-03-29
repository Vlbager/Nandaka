using System;

namespace Nandaka.Core.Table.MultiByteRegisterGroups
{
    public sealed class Int8RegisterGroup : MultiByteRegisterGroupBase<sbyte>
    {
        private readonly Register<byte> _register;

        public override sbyte Value
        {
            get => Convert.ToSByte(_register.Value);
            set => _register.Value = Convert.ToByte(value);
        }

        public Int8RegisterGroup(Register<byte> register)
            : base(new[] {register})
        {
            _register = register;
        }
    }
}