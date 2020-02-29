using System;

namespace Nandaka.Core.Protocol
{
    public abstract class DataLinkParserBase<TIn> : IParser<TIn, byte[]>
    {
        public int AwaitingReplyAddress { get ; set; }

        public event EventHandler<byte[]> MessageParsed;

        protected virtual void OnMessageParsed(byte[] checkedMessage)
        {
            MessageParsed?.Invoke(this, checkedMessage);
        }

        public abstract void Parse(TIn data);
    }
}
