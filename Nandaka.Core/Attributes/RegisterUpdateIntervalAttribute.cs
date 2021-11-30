using System;
using Nandaka.Core.Registers;
using Nandaka.Model.Registers;

namespace Nandaka.Core.Attributes
{
    public class RegisterUpdateIntervalAttribute : RegisterModifyAttribute
    {
        private readonly int _updateIntervalInMilliseconds;
        
        public RegisterUpdateIntervalAttribute(int updateIntervalInMilliseconds)
        {
            _updateIntervalInMilliseconds = updateIntervalInMilliseconds;
        }

        public override void Modify(IRegister register)
        {
            register.GetType()
                .GetProperty(nameof(IRegister.UpdateInterval))
                ?.SetValue(register, TimeSpan.FromMilliseconds(_updateIntervalInMilliseconds));
        }
    }
}