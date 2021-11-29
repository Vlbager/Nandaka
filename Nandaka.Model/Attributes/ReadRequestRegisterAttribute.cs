using System;

namespace Nandaka.Model.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ReadRequestRegisterAttribute : Attribute
    {
        public int Address { get; }

        public ReadRequestRegisterAttribute(int address)
        {
            Address = address;
        }
    }
}