using System;
using Nandaka.Core.Session;

namespace Nandaka.Core.Protocol
{
    public abstract class ApplicationParserBase<TIn> : IParser<TIn, IFrameworkMessage>
    {
        private readonly DataLinkParserBase<TIn> _dataLinkParser;

        protected ApplicationParserBase(DataLinkParserBase<TIn> dataLinkParser)
        {
            _dataLinkParser = dataLinkParser;
            _dataLinkParser.MessageParsed += 
                (sender, checkedMessage) => MessageParsed?.Invoke(sender, ApplicationParse(checkedMessage));
        }

        public event EventHandler<IFrameworkMessage> MessageParsed;

        public void Parse(TIn data)
        {
            _dataLinkParser.Parse(data);
        }

        protected abstract IFrameworkMessage ApplicationParse(byte[] data);
    }
}
