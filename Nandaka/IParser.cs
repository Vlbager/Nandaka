using System;
using System.Collections.Generic;
using System.Text;

namespace Nandaka
{
    public interface IParser<in T, M>
    {
        void Parse(T data);

        event EventHandler<M> MessageParsed;

        int AwaitingReplyAddress { get; set; }
    }
}
