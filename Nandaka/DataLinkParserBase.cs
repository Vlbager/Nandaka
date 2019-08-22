using System;
using System.Collections.Generic;
using System.Text;

namespace Nandaka
{
    public abstract class DataLinkParserBase<T> : IParser<T, byte[]>
    {
        public int AwaitingReplyAddress { get ; set; }

        public event EventHandler<byte[]> MessageParsed;

        protected virtual void OnMessageParsed(byte[] checkedMessage)
        {
            MessageParsed?.Invoke(this, checkedMessage);
        }

        public abstract void Parse(T data);
    }
}
