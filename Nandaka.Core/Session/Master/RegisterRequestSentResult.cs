using System.Collections.Generic;

namespace Nandaka.Core.Session
{
    public sealed class RegisterRequestSentResult : ISentResult
    {
        public bool IsResponseRequired { get; }
        public IReadOnlyList<int> RequestedAddresses { get; }

        public RegisterRequestSentResult(bool isResponseRequired, IReadOnlyList<int> requestedAddresses)
        {
            IsResponseRequired = isResponseRequired;
            RequestedAddresses = requestedAddresses;
        }
    }
}