namespace Nandaka.Core.Table
{
    public sealed class UInt8RegisterGroup : MultiByteRegisterGroupBase<byte>
    {
        private readonly Register<byte> _register;

        public override byte Value
        {
            get => _register.Value;
            set => _register.Value = value;
        }

        public UInt8RegisterGroup(Register<byte> register) 
            : base(new []{register})
        {
            _register = register;
        }
    }
}