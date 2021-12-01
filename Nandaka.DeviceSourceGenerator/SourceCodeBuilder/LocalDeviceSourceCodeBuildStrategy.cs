using System;
using Nandaka.Model.Registers;

namespace Nandaka.DeviceSourceGenerator
{
    internal sealed class LocalDeviceSourceCodeBuildStrategy : ISourceBuildStrategy
    {
        public string GetRegisterInterfaceType(RegisterType propertyType)
        {
            return propertyType switch
            {
                RegisterType.ReadRequest => nameof(IRegister),
                RegisterType.WriteRequest => nameof(IReadOnlyRegister<byte>),
                _ => throw new ArgumentException(nameof(propertyType))
            };
        }

        public string GetRegisterFactoryName()
        {
            return "LocalDeviceRegisterFactory";
        }

        public string GetRegisterFactoryMethodName(RegisterType propertyType)
        {
            return propertyType switch
            {
                RegisterType.ReadRequest => nameof(IRegisterFactory.Create),
                RegisterType.WriteRequest => nameof(IRegisterFactory.CreateReadOnly),
                _ => throw new ArgumentException(nameof(propertyType))
            };
        }
    }
}