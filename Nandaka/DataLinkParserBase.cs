using System;
using System.Collections.Generic;
using System.Text;

namespace Nandaka
{
    abstract class DataLinkParserBase<T> : IParser<T, byte[]>
    {
        public int AwaitingReplyAddress { get ; set; }

        public event EventHandler<byte[]> MessageParsed;

        public abstract void Parse(T data);
    }
}
