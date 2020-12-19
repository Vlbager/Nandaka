using Nandaka.Core.Exceptions;
using Nandaka.Core.Table;

namespace Nandaka.Core.Helpers
{
    public static class RegisterExtensions
    {
        public static IRegister WithAddressAssert(this IRegister register, int address)
        {
            if (register.Address != address)
                throw new InvalidRegistersReceivedException("Assert on register address was failed");
            return register;
        }
    }
}