using Nandaka.Model.Registers;

namespace Nandaka.Core.Registers
{
    // do not change name for this class (source generator)
    public sealed class LocalDeviceRegisterFactory : IRegisterFactory
    {
        public IReadOnlyRegister<T> CreateReadOnly<T>(int address, T defaultValue = default) 
            where T: struct
        {
            return new Register<T>(address, RegisterType.WriteRequest, defaultValue);
        }

        public IRegister<T> Create<T>(int address, T defaultValue = default) 
            where T: struct
        {
            return new Register<T>(address, RegisterType.ReadRequest, defaultValue);
        }
    }
}