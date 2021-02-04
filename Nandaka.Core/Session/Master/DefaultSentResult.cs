namespace Nandaka.Core.Session
{
    public sealed class DefaultSentResult : ISentResult
    {
        public bool IsResponseRequired { get; }

        public DefaultSentResult(bool isResponseRequired)
        {
            IsResponseRequired = isResponseRequired;
        }
    }
}