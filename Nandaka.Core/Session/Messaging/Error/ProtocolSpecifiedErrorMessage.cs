namespace Nandaka.Core.Session
{
    public abstract class ProtocolSpecifiedErrorMessage
    {
        public int ErrorCode { get; }

        protected ProtocolSpecifiedErrorMessage(int protocolErrorCode)
        {
            ErrorCode = protocolErrorCode;
        }
    }
}
