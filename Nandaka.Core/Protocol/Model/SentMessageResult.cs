using System;
using System.Collections.Generic;
using Nandaka.Core.Registers;
using Nandaka.Model.Registers;

namespace Nandaka.Core.Protocol
{
    public sealed class SentMessageResult
    {
        public IReadOnlyList<IRegister> SentRegisters { get; }
        public string ErrorMessage { get; }

        private SentMessageResult(IReadOnlyList<IRegister> sentRegisters, string errorMessage)
        {
            SentRegisters = sentRegisters;
            ErrorMessage = errorMessage;
        }

        public static SentMessageResult CreateSuccessResult()
        {
            return new SentMessageResult(Array.Empty<IRegister>(), String.Empty);
        }

        public static SentMessageResult CreateSuccessResult(IReadOnlyList<IRegister> sentRegisters)
        {
            return new SentMessageResult(sentRegisters, String.Empty);
        }

        public static SentMessageResult CreateErrorResult(string errorMessage)
        {
            return new SentMessageResult(Array.Empty<IRegister>(), String.Empty);
        }
    }
}