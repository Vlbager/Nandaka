using System;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Table;

namespace Nandaka.Core.Helpers
{
    public static class RegisterExtensions
    {
        public static IRegister WithAddressAssert(this IRegister register, int address)
        {
            if (register.Address != address)
                throw new InvalidRegistersException("Assert on register address was failed");
            return register;
        }
    }
}