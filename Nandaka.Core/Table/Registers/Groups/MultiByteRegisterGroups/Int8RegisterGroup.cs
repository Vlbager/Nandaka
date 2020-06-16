using System;

namespace Nandaka.Core.Table
{
    public sealed class Int8RegisterGroup : MultiByteRegisterGroupBase<sbyte>
    {
        private readonly Register<byte> _register;

        public Int8RegisterGroup(Register<byte> register)
            : base(new[] {register})
        {
            _register = register;
        }
        
        public static Int8RegisterGroup CreateNew(int address, RegisterType type = RegisterType.Raw)
            => new Int8RegisterGroup(Register<byte>.CreateByte(address, type));

        protected override byte[] ConvertValueToLittleEndianBytes(sbyte value) 
            => new[] { Convert.ToByte(value) };

        protected override sbyte ConvertGroupToValue() 
            => Convert.ToSByte(_register.Value);
    }
}