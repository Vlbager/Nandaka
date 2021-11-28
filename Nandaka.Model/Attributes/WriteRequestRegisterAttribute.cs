using System;

namespace Nandaka.Model.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class WriteRequestRegisterAttribute : Attribute
    {
        public int Address { get; }

        public WriteRequestRegisterAttribute(int address)
        {
            Address = address;
        }
    }
}