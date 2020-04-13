namespace Nandaka.Core.Table
{
    public sealed class UInt8RegisterGroup : MultiByteRegisterGroupBase<byte>
    {
        private readonly Register<byte> _register;

        public UInt8RegisterGroup(Register<byte> register) 
            : base(new []{register})
        {
            _register = register;
        }

        public static UInt8RegisterGroup CreateNew(int address, RegisterType type)
            => new UInt8RegisterGroup(Register<byte>.CreateByte(address, type));

        protected override byte[] ConvertValueToLittleEndianBytes(byte value) 
            => new[] {value};

        protected override byte ConvertGroupToValue() 
            => _register.Value;
    }
}