using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;

namespace Nandaka.Core.Table
{
    [SuppressMessage("ReSharper", "InconsistentlySynchronizedField")]
    public sealed class SingleRegisterGroup<TValue> : RegisterGroupBase<TValue>
        where TValue : struct
    {
        private readonly object _syncRoot;
        private readonly Register<TValue> _register;

        public override int DataSize => Marshal.SizeOf<TValue>();
        public override int Version { get; protected set; }

        public override TValue Value
        {
            get => _register.Value;
            set
            {
                lock (_syncRoot)
                {
                    _register.Value = value;
                    Version++;
                    IsUpdated = false;
                }
            }
        }

        public SingleRegisterGroup(Register<TValue> register) : base(register.Address,1, register.RegisterType)
        {
            _register = register;
            _syncRoot = new object();
        }

        public override void Update(IReadOnlyCollection<IRegister> registersToUpdate)
        {
            IRegister singleRegister = registersToUpdate.Single();
            if (!(singleRegister is Register<TValue> valuedRegister))
                // todo: create a custom exception
                throw new Exception("Wrong register type");

            lock (_syncRoot)
            {
                _register.Value = valuedRegister.Value;
                Version++;
                IsUpdated = true;
            }
        }

        public override IReadOnlyCollection<IRegister> GetRawRegisters()
        {
            return new[] {_register};
        }
    }
}
