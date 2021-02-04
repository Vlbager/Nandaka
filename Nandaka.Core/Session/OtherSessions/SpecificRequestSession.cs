using System;
using Nandaka.Core.Device;
using Nandaka.Core.Logging;
using Nandaka.Core.Protocol;

namespace Nandaka.Core.Session
{
    public sealed class SpecificRequestSession : RequestSessionBase<ErrorMessage, DefaultSentResult>
    {
        protected override ILog Log { get; }
        
        public SpecificRequestSession(IProtocol protocol, TimeSpan requestTimeout, NandakaDevice device) 
            : base(protocol, device, requestTimeout)
        {
            FilterRules.Add(message => message is ISpecificMessage or ErrorMessage);
        }

        protected override ErrorMessage GetNextMessage()
        {
            throw new NotImplementedException();
        }

        protected override DefaultSentResult SendRequest(ErrorMessage message)
        {
            throw new NotImplementedException();
        }

        protected override void ProcessResponse(IMessage message, DefaultSentResult sentResult)
        {
            throw new NotImplementedException();
        }
    }
}