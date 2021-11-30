using System.Collections.Generic;
using Nandaka.Core.Helpers;
using Nandaka.Core.Registers;
using Nandaka.Model.Registers;

namespace Nandaka.Core.Util
{
    internal sealed class RegistersLogMessage
    {
        private readonly IReadOnlyCollection<IRegister> _registers;

        public RegistersLogMessage(IReadOnlyCollection<IRegister> registers)
        {
            _registers = registers;
        }

        public override string ToString()
        {
            return _registers.ToLogLine();
        }
    }
}