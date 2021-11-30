using System.Collections.Generic;
using Nandaka.Core.Registers;
using Nandaka.Model.Registers;

namespace Nandaka.Core.Session
{
    public sealed class RegisterRequestSentResult : ISentResult
    {
        public bool IsResponseRequired { get; }
        public IReadOnlyList<IRegister> RequestedRegisters { get; }

        public RegisterRequestSentResult(bool isResponseRequired, IReadOnlyList<IRegister> requestedRegisters)
        {
            IsResponseRequired = isResponseRequired;
            RequestedRegisters = requestedRegisters;
        }
    }
}