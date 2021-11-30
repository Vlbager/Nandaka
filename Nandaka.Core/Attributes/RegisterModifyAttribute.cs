using System;
using Nandaka.Core.Registers;
using Nandaka.Model.Registers;

namespace Nandaka.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class RegisterModifyAttribute : Attribute
    {
        public abstract void Modify(IRegister register);
    }
}