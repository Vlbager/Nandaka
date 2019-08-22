using System;
using System.Collections.Generic;
using System.Text;

namespace Nandaka
{
    public interface IParser<in TIn, TOut>
    {
        void Parse(TIn data);

        event EventHandler<TOut> MessageParsed;

        int AwaitingReplyAddress { get; set; }
    }
}
