using System;
using Nandaka.Core.Table;

namespace Nandaka.Core.Attributes
{
    public class RegisterUpdateIntervalAttribute : RegisterModifyAttribute
    {
        private readonly int _updateIntervalInMilliseconds;
        
        public RegisterUpdateIntervalAttribute(int updateIntervalInMilliseconds)
        {
            _updateIntervalInMilliseconds = updateIntervalInMilliseconds;
        }

        public override void Modify(IRegisterGroup registerGroup)
        {
            registerGroup.GetType()
                .GetProperty(nameof(IRegisterGroup.UpdateInterval))
                ?.SetValue(registerGroup, TimeSpan.FromMilliseconds(_updateIntervalInMilliseconds));
        }
    }
}