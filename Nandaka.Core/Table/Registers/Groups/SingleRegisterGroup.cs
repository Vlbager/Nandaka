using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Nandaka.Core.Table
{
    public sealed class SingleRegisterGroup<TValue> : RegisterGroupBase<TValue>
        where TValue : struct
    {
        private readonly Register<TValue> _register;

        public override int DataSize => Marshal.SizeOf<TValue>();
        public override TValue Value => _register.Value;

        public SingleRegisterGroup(Register<TValue> register) : base(register.Address,1, register.RegisterType)
        {
            _register = register;
        }

        public override IReadOnlyCollection<IRegister> GetRawRegisters()
        {
            return new[] {_register};
        }
    }
}
