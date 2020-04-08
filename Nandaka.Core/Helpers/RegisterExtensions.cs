using System;
using Nandaka.Core.Table;

namespace Nandaka.Core.Helpers
{
    public static class RegisterExtensions
    {
        public static IRegister WithAddressAssert(this IRegister register, int address)
        {
            if (register.Address != address)
                // todo: create a custom exception
                throw new Exception("Assert on register address was failed");
            return register;
        }
    }
}