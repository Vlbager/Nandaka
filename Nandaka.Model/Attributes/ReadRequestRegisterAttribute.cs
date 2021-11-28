using System;

namespace Nandaka.Model.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ReadRequestRegisterAttribute : Attribute
    {
        public int Address { get; }

        public ReadRequestRegisterAttribute(int address)
        {
            Address = address;
        }
    }
}