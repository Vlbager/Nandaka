using System;

namespace Nandaka.Core.Protocol
{
    public interface IParser<in TIn, TOut> where TOut: notnull
    {
        void Parse(TIn data);

        event EventHandler<TOut> MessageParsed;
    }
}
