using System;
using Nandaka.Model.Registers;

namespace Nandaka.DeviceSourceGenerator
{
    internal sealed class ForeignDeviceSourceCodeBuildStrategy : ISourceBuildStrategy
    {
        public string GetRegisterInterfaceType(RegisterType propertyType)
        {
            return propertyType switch
            {
                RegisterType.ReadRequest => nameof(IReadOnlyRegister<byte>),
                RegisterType.WriteRequest => nameof(IRegister),
                _ => throw new ArgumentException(nameof(propertyType))
            };
        }

        public string GetRegisterFactoryName()
        {
            return "ForeignDeviceRegisterFactory";
        }

        public string GetRegisterFactoryMethodName(RegisterType propertyType)
        {
            return propertyType switch
            {
                RegisterType.ReadRequest => nameof(IRegisterFactory.CreateReadOnly),
                RegisterType.WriteRequest => nameof(IRegisterFactory.Create),
                _ => throw new ArgumentException(nameof(propertyType))
            };
        }
    }
}