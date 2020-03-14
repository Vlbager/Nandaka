using System.Collections.Generic;

namespace Nandaka.Core.Table
{
    public sealed class SingleRegisterGroup<TValue> : RegisterGroupBase<TValue>
        where TValue : struct
    {
        private readonly Register<TValue> _register;

        public SingleRegisterGroup(Register<TValue> register) : base(register.Address, 1)
        {
            _register = register;
        }

        public override TValue Value => _register.Value;

        public override IReadOnlyCollection<IRegister> GetRawRegisters()
        {
            return new[] {_register};
        }
    }
}
