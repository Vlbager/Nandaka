using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Registers;

namespace Nandaka.Core.Helpers
{
    public static class RegisterExtensions
    {
        public static string ToLogLine(this IEnumerable<IRegister> registers)
        {
            return registers.Select(register => register.ToLogLine()).JoinString("; ");
        }

        public static string ToLogLine(this IRegister register)
        {
            return $"address: {register.Address.ToString()}, bytes: {{{register.ToBytes().JoinString(", ")}}}";
        }
    }
}