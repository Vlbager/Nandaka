using System;
using System.Collections.Generic;
using System.Text;

namespace Nandaka
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
