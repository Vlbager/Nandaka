namespace Nandaka.Core.Session
{
    public class ProtocolSpecifiedErrorMessage
    {
        public int ErrorCode { get; }

        public ProtocolSpecifiedErrorMessage(int protocolErrorCode)
        {
            ErrorCode = protocolErrorCode;
        }

        public virtual string ToLogLine()
        {
            return $"ErrorCode: {ErrorCode.ToString()}";
        }
    }
}
