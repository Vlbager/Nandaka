using System;

namespace Nandaka.Model.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class GenerateDeviceAttribute : Attribute
    {
        public Type TableType { get; }

        public GenerateDeviceAttribute(Type tableType)
        {
            TableType = tableType;
        }
    }
}