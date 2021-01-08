using Nandaka.Core.Registers;

namespace Nandaka.MilliGanjubus.Models
{
    public sealed class MgMappedRegister
    {
        public IRegister UserRegister { get; }
        public IRegister<byte>[] MgRegisters { get; }

        public MgMappedRegister(IRegister userRegister, IRegister<byte>[] mgRegisters)
        {
            UserRegister = userRegister;
            MgRegisters = mgRegisters;
        }
    }
}