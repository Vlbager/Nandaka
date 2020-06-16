using System;
using Nandaka.Core.Table;

namespace Nandaka.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class RegisterModifyAttribute : Attribute
    {
        public abstract void Modify(IRegisterGroup registerGroup);
    }
}