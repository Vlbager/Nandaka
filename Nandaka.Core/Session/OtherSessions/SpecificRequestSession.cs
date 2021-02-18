using System;

namespace Nandaka.Core.Session
{
    public sealed class SpecificRequestSession : IRequestSession<ISpecificMessage, DefaultSentResult>
    {
        public ISpecificMessage GetNextMessage()
        {
            throw new NotImplementedException();
        }

        public DefaultSentResult SendRequest(ISpecificMessage message)
        {
            throw new NotImplementedException();
        }

        public void ProcessResponse(IMessage message, DefaultSentResult sentResult)
        {
            throw new NotImplementedException();
        }
    }
}