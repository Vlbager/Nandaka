using System;

namespace Nandaka.Model.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class WriteRequestRegisterAttribute : Attribute
    {
        public int Address { get; }

        public WriteRequestRegisterAttribute(int address)
        {
            Address = address;
        }
    }
}