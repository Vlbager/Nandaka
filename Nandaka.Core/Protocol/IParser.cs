using System;

namespace Nandaka.Core.Protocol
{
    public interface IParser<in TIn, TOut>
    {
        void Parse(TIn data);

        event EventHandler<TOut> MessageParsed;

        int AwaitingReplyAddress { get; set; }
    }
}
